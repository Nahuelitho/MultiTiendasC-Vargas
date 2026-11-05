using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "SUPER_ADMIN")]
public class SuperAdminController(ContextoAplicacion db, ILogger<SuperAdminController> registro) : Controller
{
    private int UsuarioIdActual => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsuarios = await db.Usuarios.CountAsync();
        ViewBag.TotalEmpresas = await db.Empresas.CountAsync();
        ViewBag.TotalEmpresasPendientes = await db.Empresas.CountAsync(e => e.Estado == EstadoEmpresa.Pendiente);
        ViewBag.TotalPedidos = await db.Pedidos.CountAsync();
        ViewBag.TotalRoles = await db.Roles.CountAsync();
        return View();
    }

    // ----------------------------- Usuarios -----------------------------

    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
        return View(usuarios);
    }

    public async Task<IActionResult> CrearUsuario()
    {
        await PrepararCombosAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(string nombre, string correo, string contrasena, int rolId, int? empresaId)
    {
        nombre = nombre?.Trim() ?? string.Empty;
        correo = correo?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            return await VolverConErrorAsync("Todos los campos son obligatorios.");
        }

        if (!new EmailAddressAttribute().IsValid(correo))
        {
            return await VolverConErrorAsync("Ingresá un correo electrónico válido.");
        }

        if (contrasena.Length < 6)
        {
            return await VolverConErrorAsync("La contraseña debe tener al menos 6 caracteres.");
        }

        if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
        {
            return await VolverConErrorAsync("Ya existe un usuario con ese correo.");
        }

        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == rolId && r.Activo);
        if (rol is null)
        {
            return await VolverConErrorAsync("El rol seleccionado no existe o está inactivo.");
        }

        // Solo el rol de comerciante se asocia a una empresa.
        if (rol.Codigo != "COMERCIANTE")
        {
            empresaId = null;
        }
        else if (empresaId.HasValue && !await db.Empresas.AnyAsync(e => e.Id == empresaId.Value))
        {
            return await VolverConErrorAsync("La empresa seleccionada no existe.");
        }

        var usuario = new Usuario
        {
            Nombre = nombre,
            Correo = correo,
            ContrasenaHash = PasswordHasher.Hash(contrasena),
            FechaAlta = DateTime.UtcNow
        };

        try
        {
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rol.Id, EmpresaId = empresaId });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo crear el usuario {Correo}.", correo);
            return await VolverConErrorAsync("No pudimos crear el usuario. Verificá los datos e intentá nuevamente.");
        }

        TempData["Mensaje"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Usuarios));
    }

    // ----------------------------- Roles -----------------------------

    public async Task<IActionResult> Roles()
    {
        var roles = await db.Roles
            .AsNoTracking()
            .Include(r => r.UsuarioRoles)
            .OrderBy(r => r.Id)
            .ToListAsync();
        return View(roles);
    }

    public IActionResult CrearRol() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearRol(string nombre, string codigo)
    {
        nombre = nombre?.Trim() ?? string.Empty;
        codigo = NormalizarCodigo(codigo);

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
        {
            ViewData["Error"] = "El nombre y el código son obligatorios.";
            return View();
        }

        if (codigo.Length > 40 || nombre.Length > 40)
        {
            ViewData["Error"] = "El nombre y el código no pueden superar los 40 caracteres.";
            return View();
        }

        if (!Regex.IsMatch(codigo, "^[A-Z0-9_]+$"))
        {
            ViewData["Error"] = "El código solo puede contener letras, números y guiones bajos.";
            return View();
        }

        if (await db.Roles.AnyAsync(r => r.Codigo == codigo))
        {
            ViewData["Error"] = "Ya existe un rol con ese código.";
            return View();
        }

        try
        {
            db.Roles.Add(new Rol { Nombre = nombre, Codigo = codigo, Activo = true });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo crear el rol {Codigo}.", codigo);
            ViewData["Error"] = "No pudimos crear el rol. Intentá nuevamente.";
            return View();
        }

        TempData["Mensaje"] = "Rol creado correctamente.";
        return RedirectToAction(nameof(Roles));
    }

    public async Task<IActionResult> EditarRol(int id)
    {
        var rol = await db.Roles.FindAsync(id);
        return rol is null ? NotFound() : View(rol);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarRol(int id, string nombre, string codigo, bool activo)
    {
        var rol = await db.Roles.FindAsync(id);
        if (rol is null)
        {
            return NotFound();
        }

        nombre = nombre?.Trim() ?? string.Empty;
        codigo = NormalizarCodigo(codigo);

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
        {
            ViewData["Error"] = "El nombre y el código son obligatorios.";
            return View(rol);
        }

        if (!Regex.IsMatch(codigo, "^[A-Z0-9_]+$"))
        {
            ViewData["Error"] = "El código solo puede contener letras, números y guiones bajos.";
            return View(rol);
        }

        // El rol de Super Administrador no puede perder su código ni desactivarse.
        if (rol.Codigo == "SUPER_ADMIN" && (codigo != "SUPER_ADMIN" || !activo))
        {
            ViewData["Error"] = "El rol SUPER_ADMIN no puede cambiar de código ni desactivarse.";
            return View(rol);
        }

        if (await db.Roles.AnyAsync(r => r.Id != id && r.Codigo == codigo))
        {
            ViewData["Error"] = "Ya existe otro rol con ese código.";
            return View(rol);
        }

        rol.Nombre = nombre;
        rol.Codigo = codigo;
        rol.Activo = activo;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo editar el rol {Id}.", id);
            ViewData["Error"] = "No pudimos guardar los cambios del rol.";
            return View(rol);
        }

        TempData["Mensaje"] = "Rol actualizado correctamente.";
        return RedirectToAction(nameof(Roles));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoRol(int id)
    {
        var rol = await db.Roles.FindAsync(id);
        if (rol is null)
        {
            return NotFound();
        }

        if (rol.Codigo == "SUPER_ADMIN" && rol.Activo)
        {
            TempData["Error"] = "El rol SUPER_ADMIN no puede desactivarse.";
            return RedirectToAction(nameof(Roles));
        }

        rol.Activo = !rol.Activo;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo cambiar el estado del rol {RolId}.", id);
            TempData["Error"] = "No pudimos cambiar el estado del rol.";
            return RedirectToAction(nameof(Roles));
        }

        TempData["Mensaje"] = rol.Activo ? "Rol activado." : "Rol desactivado.";
        return RedirectToAction(nameof(Roles));
    }

    public async Task<IActionResult> UsuarioRoles(int id)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Empresa)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
        {
            return NotFound();
        }

        ViewBag.Roles = await db.Roles.Where(r => r.Activo).OrderBy(r => r.Nombre).ToListAsync();
        ViewBag.Empresas = await db.Empresas.OrderBy(e => e.Nombre).ToListAsync();
        return View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarRol(int usuarioId, int rolId, int? empresaId)
    {
        if (!await db.Usuarios.AnyAsync(u => u.Id == usuarioId))
        {
            TempData["Error"] = "El usuario no existe.";
            return RedirectToAction(nameof(Usuarios));
        }

        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == rolId && r.Activo);
        if (rol is null)
        {
            TempData["Error"] = "El rol no existe o está inactivo.";
            return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
        }

        // El rol de Super Administrador siempre es global.
        if (rol.Codigo == "SUPER_ADMIN")
        {
            empresaId = null;
        }

        if (empresaId.HasValue && !await db.Empresas.AnyAsync(e => e.Id == empresaId.Value))
        {
            TempData["Error"] = "La empresa seleccionada no existe.";
            return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
        }

        var yaAsignado = await db.UsuarioRoles.AnyAsync(ur =>
            ur.UsuarioId == usuarioId && ur.RolId == rolId && ur.EmpresaId == empresaId);

        if (yaAsignado)
        {
            TempData["Error"] = "El usuario ya tiene ese rol con el mismo alcance.";
            return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
        }

        try
        {
            db.UsuarioRoles.Add(new UsuarioRol
            {
                UsuarioId = usuarioId,
                RolId = rolId,
                EmpresaId = empresaId
            });
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo asignar el rol {RolId} al usuario {UsuarioId}.", rolId, usuarioId);
            TempData["Error"] = "No pudimos asignar el rol. Intentá nuevamente.";
            return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
        }

        TempData["Mensaje"] = "Rol asignado correctamente. El usuario debe volver a iniciar sesión para verlo reflejado.";
        return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuitarRol(int id)
    {
        var usuarioRol = await db.UsuarioRoles
            .Include(ur => ur.Rol)
            .FirstOrDefaultAsync(ur => ur.Id == id);

        if (usuarioRol is null)
        {
            return NotFound();
        }

        if (usuarioRol.Rol.Codigo == "SUPER_ADMIN")
        {
            if (usuarioRol.UsuarioId == UsuarioIdActual)
            {
                TempData["Error"] = "No podés quitarte tu propio rol de Super Administrador.";
                return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioRol.UsuarioId });
            }

            var totalSuperAdmins = await db.UsuarioRoles
                .CountAsync(ur => ur.Rol.Codigo == "SUPER_ADMIN" && ur.Rol.Activo);

            if (totalSuperAdmins <= 1)
            {
                TempData["Error"] = "Debe existir al menos un Super Administrador activo.";
                return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioRol.UsuarioId });
            }
        }

        var usuarioId = usuarioRol.UsuarioId;
        db.UsuarioRoles.Remove(usuarioRol);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo quitar el rol {UsuarioRolId}.", id);
            TempData["Error"] = "No pudimos quitar el rol. Intentá nuevamente.";
            return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
        }

        TempData["Mensaje"] = "Rol quitado correctamente.";
        return RedirectToAction(nameof(UsuarioRoles), new { id = usuarioId });
    }

    // ----------------------------- Empresas -----------------------------

    public async Task<IActionResult> Empresas()
    {
        var empresas = await db.Empresas
            .AsNoTracking()
            .Include(e => e.Propietario)
            .OrderBy(e => e.Estado)
            .ThenBy(e => e.Nombre)
            .ToListAsync();
        return View(empresas);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoEmpresa(int id, EstadoEmpresa estado)
    {
        if (!Enum.IsDefined(estado))
        {
            TempData["Error"] = "El estado seleccionado no es válido.";
            return RedirectToAction(nameof(Empresas));
        }

        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.Id == id);
        if (empresa is null)
        {
            return NotFound();
        }

        var estadoAnterior = empresa.Estado;
        empresa.Estado = estado;

        if (estadoAnterior != estado)
        {
            var (titulo, mensaje) = estado switch
            {
                EstadoEmpresa.Activa => (
                    "Empresa aprobada",
                    $"Tu empresa \"{empresa.Nombre}\" fue aprobada y ya aparece en el catálogo público."),
                EstadoEmpresa.Suspendida => (
                    "Empresa suspendida",
                    $"Tu empresa \"{empresa.Nombre}\" fue suspendida temporalmente. Contactá al administrador para más información."),
                EstadoEmpresa.EnRevision => (
                    "Empresa en revisión",
                    $"La empresa \"{empresa.Nombre}\" quedó en revisión por el administrador."),
                _ => (
                    "Estado de empresa actualizado",
                    $"El estado de \"{empresa.Nombre}\" cambió a {estado}.")
            };

            db.Notificaciones.Add(new Notificacion
            {
                UsuarioId = empresa.PropietarioId,
                EmpresaId = empresa.Id,
                Titulo = titulo,
                Mensaje = mensaje,
                Fecha = DateTime.UtcNow
            });
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo cambiar el estado de la empresa {EmpresaId}.", id);
            TempData["Error"] = "No pudimos actualizar la empresa. Intentá nuevamente.";
            return RedirectToAction(nameof(Empresas));
        }

        TempData["Mensaje"] = $"Estado de la empresa actualizado a {estado}.";
        return RedirectToAction(nameof(Empresas));
    }

    // ----------------------------- Auxiliares -----------------------------

    private async Task<IActionResult> VolverConErrorAsync(string mensaje)
    {
        ViewData["Error"] = mensaje;
        await PrepararCombosAsync();
        return View("CrearUsuario");
    }

    private async Task PrepararCombosAsync()
    {
        ViewBag.Roles = await db.Roles.Where(r => r.Activo).OrderBy(r => r.Id).ToListAsync();
        ViewBag.Empresas = await db.Empresas.OrderBy(e => e.Nombre).ToListAsync();
    }

    private static string NormalizarCodigo(string? codigo) =>
        (codigo ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
}
