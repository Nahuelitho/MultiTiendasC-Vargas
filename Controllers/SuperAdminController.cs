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

    public IActionResult CrearUsuario()
    {
        ViewBag.Roles = db.Roles.OrderBy(r => r.Id).ToList();
        ViewBag.Empresas = db.Empresas.OrderBy(e => e.Nombre).ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario(string nombre, string correo, string contrasena, int rolId, int? empresaId)
    {
        if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
        {
            ViewData["Error"] = "Ya existe un usuario con ese correo.";
            ViewBag.Roles = db.Roles.OrderBy(r => r.Id).ToList();
            ViewBag.Empresas = db.Empresas.OrderBy(e => e.Nombre).ToList();
            return View();
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

        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rolId, EmpresaId = empresaId });
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Usuarios));
    }
}
