using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.ViewModels;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;


[Authorize(Roles = "SUPER_ADMIN")]
public class SuperAdminController(ContextoAplicacion db, ILogger<SuperAdminController> registro) : Controller
{
    private const int TamanoPaginaAdmin = 10;
    private const int TamanoPaginaRoles = 8;

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

    public async Task<IActionResult> Usuarios(int pagina = 1)
    {
        var consulta = db.Usuarios
            .AsNoTracking()
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Nombre);

        var (paginaActual, totalPaginas, total) = await CalcularPaginaAsync(consulta, pagina, TamanoPaginaAdmin);

        var usuarios = await consulta
            .Skip((paginaActual - 1) * TamanoPaginaAdmin)
            .Take(TamanoPaginaAdmin)
            .ToListAsync();

        ViewBag.Paginacion = new PaginacionVista
        {
            Pagina = paginaActual,
            TotalPaginas = totalPaginas,
            Controlador = "SuperAdmin",
            Accion = nameof(Usuarios)
        };
        ViewBag.TotalRegistros = total;

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

    // CRUD de roles con Vue + AJAX (los endpoints están más abajo).
    public IActionResult RolesVue() => View();

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

    public async Task<IActionResult> Empresas(string? q = null, int pagina = 1)
    {
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var consulta = db.Empresas
            .AsNoTracking()
            .Include(e => e.Propietario)
            .AsQueryable();

        if (q is not null)
        {
            consulta = consulta.Where(e =>
                e.Nombre.Contains(q) ||
                e.Localidad.Contains(q) ||
                e.Propietario.Nombre.Contains(q) ||
                e.Propietario.Correo.Contains(q));
        }

        consulta = consulta
            .OrderBy(e => e.Estado)
            .ThenBy(e => e.Nombre);

        var (paginaActual, totalPaginas, total) = await CalcularPaginaAsync(consulta, pagina, TamanoPaginaAdmin);

        var empresas = await consulta
            .Skip((paginaActual - 1) * TamanoPaginaAdmin)
            .Take(TamanoPaginaAdmin)
            .ToListAsync();

        var parametros = new Dictionary<string, string>();
        if (q is not null)
        {
            parametros["q"] = q;
        }

        ViewBag.Paginacion = new PaginacionVista
        {
            Pagina = paginaActual,
            TotalPaginas = totalPaginas,
            Controlador = "SuperAdmin",
            Accion = nameof(Empresas),
            Parametros = parametros
        };
        ViewBag.TotalRegistros = total;
        ViewBag.Busqueda = q;

        return View(empresas);
    }

    // Búsqueda asíncrona usada por los autocompletados de empresa.
    [HttpGet]
    public async Task<IActionResult> BuscarEmpresas(string? q = null, int limite = 10)
    {
        limite = Math.Clamp(limite, 1, 25);

        var consulta = db.Empresas.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var termino = q.Trim();
            consulta = consulta.Where(e =>
                e.Nombre.Contains(termino) ||
                e.Localidad.Contains(termino));
        }

        var empresas = await consulta
            .OrderBy(e => e.Nombre)
            .Take(limite)
            .Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Localidad,
                Estado = e.Estado.ToString()
            })
            .ToListAsync();

        return Json(empresas);
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

    // ----------------------------- API JSON de Roles (Vue) -----------------------------

    [HttpGet]
    public async Task<IActionResult> ApiRoles(string? q = null, int pagina = 1, int tamano = TamanoPaginaRoles)
    {
        tamano = Math.Clamp(tamano, 1, 50);
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

        var consulta = db.Roles.AsNoTracking();

        if (q is not null)
        {
            consulta = consulta.Where(r => r.Nombre.Contains(q) || r.Codigo.Contains(q));
        }

        var (paginaActual, totalPaginas, total) = await CalcularPaginaAsync(consulta, pagina, tamano);

        var roles = await consulta
            .OrderBy(r => r.Id)
            .Skip((paginaActual - 1) * tamano)
            .Take(tamano)
            .Select(r => new RolApiVista
            {
                Id = r.Id,
                Nombre = r.Nombre,
                Codigo = r.Codigo,
                Activo = r.Activo,
                Usuarios = r.UsuarioRoles.Count
            })
            .ToListAsync();

        foreach (var rol in roles)
        {
            rol.Protegido = rol.Codigo == "SUPER_ADMIN";
        }

        return Json(new { pagina = paginaActual, totalPaginas, total, roles });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiCrearRol([FromBody] RolEntradaApiDto? entrada)
    {
        var nombre = entrada?.Nombre?.Trim() ?? string.Empty;
        var codigo = NormalizarCodigo(entrada?.Codigo);

        var error = ValidarDatosRol(nombre, codigo);
        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        if (await db.Roles.AnyAsync(r => r.Codigo == codigo))
        {
            return Conflict(new { mensaje = "Ya existe un rol con ese código." });
        }

        var rol = new Rol { Nombre = nombre, Codigo = codigo, Activo = true };

        try
        {
            db.Roles.Add(rol);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo crear el rol {Codigo}.", codigo);
            return StatusCode(500, new { mensaje = "No pudimos crear el rol. Intentá nuevamente." });
        }

        return Json(new
        {
            mensaje = "Rol creado correctamente.",
            rol = new RolApiVista
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Codigo = rol.Codigo,
                Activo = rol.Activo,
                Usuarios = 0,
                Protegido = rol.Codigo == "SUPER_ADMIN"
            }
        });
    }

    private string NormalizarCodigo(object codigo)
    {
        throw new NotImplementedException();
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiActualizarRol(int id, [FromBody] RolEntradaApiDto? entrada)
    {
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == id);
        if (rol is null)
        {
            return NotFound(new { mensaje = "El rol no existe." });
        }

        var nombre = entrada?.Nombre?.Trim() ?? string.Empty;
        var codigo = NormalizarCodigo(entrada?.Codigo);
        var activo = entrada?.Activo ?? rol.Activo;

        var error = ValidarDatosRol(nombre, codigo);
        if (error is not null)
        {
            return BadRequest(new { mensaje = error });
        }

        if (rol.Codigo == "SUPER_ADMIN" && (codigo != "SUPER_ADMIN" || !activo))
        {
            return BadRequest(new { mensaje = "El rol SUPER_ADMIN no puede cambiar de código ni desactivarse." });
        }

        if (await db.Roles.AnyAsync(r => r.Id != id && r.Codigo == codigo))
        {
            return Conflict(new { mensaje = "Ya existe otro rol con ese código." });
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
            registro.LogError(ex, "No se pudo actualizar el rol {Id}.", id);
            return StatusCode(500, new { mensaje = "No pudimos guardar los cambios del rol." });
        }

        var usuarios = await db.UsuarioRoles.CountAsync(ur => ur.RolId == id);

        return Json(new
        {
            mensaje = "Rol actualizado correctamente.",
            rol = new RolApiVista
            {
                Id = rol.Id,
                Nombre = rol.Nombre,
                Codigo = rol.Codigo,
                Activo = rol.Activo,
                Usuarios = usuarios,
                Protegido = rol.Codigo == "SUPER_ADMIN"
            }
        });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiEliminarRol(int id)
    {
        var rol = await db.Roles
            .Include(r => r.UsuarioRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (rol is null)
        {
            return NotFound(new { mensaje = "El rol no existe." });
        }

        if (rol.Codigo == "SUPER_ADMIN")
        {
            return BadRequest(new { mensaje = "El rol SUPER_ADMIN no se puede eliminar ni desactivar." });
        }

        bool eliminado;

        if (rol.UsuarioRoles.Count > 0)
        {
            // Conserva el historial: si tiene usuarios asignados, solo se desactiva.
            rol.Activo = false;
            eliminado = false;
        }
        else
        {
            db.Roles.Remove(rol);
            eliminado = true;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo eliminar el rol {Id}.", id);
            return StatusCode(500, new { mensaje = "No podemos eliminar el rol. Intentá nuevamente." });
        }

        return Json(new
        {
            mensaje = eliminado
                ? "Rol eliminado correctamente."
                : "El rol tiene usuarios asignados, así que se desactivó para conservar el historial.",
            eliminado
        });
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
    }

    private static async Task<(int Pagina, int TotalPaginas, int Total)> CalcularPaginaAsync<T>(
        IQueryable<T> consulta, int pagina, int tamano)
    {
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamano));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        return (pagina, totalPaginas, total);
    }

    private static string? ValidarDatosRol(string nombre, string codigo)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
        {
            return "El nombre y el código son obligatorios.";
        }

        if (nombre.Length > 40 || codigo.Length > 40)
        {
            return "El nombre y el código no pueden superar los 40 caracteres.";
        }

        if (!Regex.IsMatch(codigo, "^[A-Z0-9_]+$"))
        {
            return "El código solo puede contener letras, números y guiones bajos.";
        }

        return null;
    }

    private static string NormalizarCodigo(string? codigo) =>
        (codigo ?? string.Empty).Trim().ToUpperInvariant().Replace(' ', '_');
}

public class RolEntradaApiDto
{
    public string? Nombre { get; set; }
    public string? Codigo { get; set; }
    public bool? Activo { get; set; }
}

internal class RolApiVista
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Codigo { get; set; }
    public bool Activo { get; set; }
    public int Usuarios { get; set; }
    public bool Protegido { get; internal set; }
}