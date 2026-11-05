using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Services;

public class ServicioSesion(ContextoAplicacion db)
{
    public async Task IniciarSesionAsync(HttpContext contexto, Usuario usuario)
    {
        var roles = await db.UsuarioRoles
            .Where(usuarioRol => usuarioRol.UsuarioId == usuario.Id && usuarioRol.Rol.Activo)
            .Select(usuarioRol => usuarioRol.Rol.Codigo)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo)
        };
        claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await contexto.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidad));
    }
}
