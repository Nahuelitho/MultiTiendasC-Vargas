using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;

public class CuentaController(ContextoAplicacion db, ServicioSesion sesion) : Controller
{
    public IActionResult Registro() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(string nombre, string correo, string contrasena)
    {
        nombre = nombre?.Trim() ?? string.Empty;
        correo = correo?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewData["Error"] = "Todos los campos son obligatorios.";
            return View();
        }

        if (!new EmailAddressAttribute().IsValid(correo))
        {
            ViewData["Error"] = "Ingresá un correo electrónico válido.";
            return View();
        }

        if (contrasena.Length < 6)
        {
            ViewData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
            return View();
        }

        if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
        {
            ViewData["Error"] = "Ya existe una cuenta con ese correo.";
            return View();
        }

        var rolCliente = await db.Roles.FirstOrDefaultAsync(r => r.Codigo == "CLIENTE" && r.Activo);
        if (rolCliente is null)
        {
            ViewData["Error"] = "El registro no está disponible en este momento.";
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

        // Asigna el rol Cliente con alcance global (EmpresaId = null).
        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = rolCliente.Id, EmpresaId = null });
        await db.SaveChangesAsync();

        await sesion.IniciarSesionAsync(HttpContext, usuario);
        return RedirectToAction("Inicio", "Inicio");
    }

    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        correo = correo?.Trim() ?? string.Empty;
        var clave = contrasena ?? string.Empty;

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        if (usuario == null || !PasswordHasher.Verify(clave, usuario.ContrasenaHash))
        {
            ViewData["Error"] = "Correo o contraseña incorrectos.";
            return View();
        }

        await sesion.IniciarSesionAsync(HttpContext, usuario);
        return RedirectToAction("Inicio", "Inicio");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Inicio", "Inicio");
    }

    public IActionResult AccesoDenegado() => View();
}
