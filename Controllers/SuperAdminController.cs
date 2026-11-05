using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "SUPER_ADMIN")]
public class SuperAdminController(ContextoAplicacion db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalUsuarios = await db.Usuarios.CountAsync();
        ViewBag.TotalEmpresas = await db.Empresas.CountAsync();
        ViewBag.TotalPedidos = await db.Pedidos.CountAsync();
        return View();
    }

    public async Task<IActionResult> Usuarios()
    {
        var usuarios = await db.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .OrderBy(u => u.Nombre)
            .ToListAsync();
        return View(usuarios);
    }

    public async Task<IActionResult> Empresas()
    {
        var empresas = await db.Empresas.OrderBy(e => e.Nombre).ToListAsync();
        return View(empresas);
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

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rol.Id, EmpresaId = empresaId });
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Usuarios));
    }

    private async Task<IActionResult> VolverConErrorAsync(string mensaje)
    {
        ViewData["Error"] = mensaje;
        await PrepararCombosAsync();
        return View();
    }

    private async Task PrepararCombosAsync()
    {
        ViewBag.Roles = await db.Roles.Where(r => r.Activo).OrderBy(r => r.Id).ToListAsync();
        ViewBag.Empresas = await db.Empresas.OrderBy(e => e.Nombre).ToListAsync();
    }
}
