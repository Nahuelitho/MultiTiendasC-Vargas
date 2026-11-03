using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;

public class CuentaController(ContextoAplicacion db) : Controller
{
    public IActionResult Registro() => View();

    [HttpPost]
    public async Task<IActionResult> Registro(string nombre, string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
        {
            ViewData["Error"] = "Todos los campos son obligatorios.";
            return View();
        }

        if (await db.Usuarios.AnyAsync(u => u.Correo == correo))
        {
            ViewData["Error"] = "Ya existe una cuenta con ese correo.";
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

        // Asigna rol Cliente (Id = 4), alcance global (EmpresaId = null).
        db.UsuarioRoles.Add(new UsuarioRol { UsuarioId = usuario.Id, RolId = 4, EmpresaId = null });
        await db.SaveChangesAsync();

        await IniciarSesion(usuario);
        return RedirectToAction("Inicio", "Inicio");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        if (usuario == null || !PasswordHasher.Verify(contrasena, usuario.ContrasenaHash))
        {
            ViewData["Error"] = "Correo o contraseña incorrectos.";
            return View();
        }

        await IniciarSesion(usuario);
        return RedirectToAction("Inicio", "Inicio");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Inicio", "Inicio");
    }

    public IActionResult AccesoDenegado() => View();

    private async Task IniciarSesion(Usuario usuario)
    {
        var roles = await db.UsuarioRoles
            .Where(ur => ur.UsuarioId == usuario.Id)
            .Select(ur => ur.Rol.Codigo)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo)
        };
        claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
    }
}
