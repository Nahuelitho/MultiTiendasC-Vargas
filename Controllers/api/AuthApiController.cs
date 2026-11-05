using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthApiController : ControllerBase
{
    private readonly ContextoAplicacion _ctx;
    private readonly ServicioJwt _jwt;

    public AuthApiController(ContextoAplicacion ctx, ServicioJwt jwt){
        _ctx = ctx; 
        _jwt = jwt;
    }

    public record LoginDto(string Correo, string Contrasena);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _ctx.Usuarios.FirstOrDefaultAsync(u => u.Correo == dto.Correo);
        
        // Tu hasher es estático y el método es Verify
        if(user == null || !PasswordHasher.Verify(dto.Contrasena, user.ContrasenaHash))
            return Unauthorized(new { mensaje = "Credenciales inválidas" });

        var roles = await _ctx.UsuarioRoles
            .Where(ur => ur.UsuarioId == user.Id)
            .Include(ur => ur.Rol)
            .Where(ur => ur.Rol.Activo)
            .Select(ur => ur.Rol.Codigo)
            .ToListAsync();

        var token = _jwt.GenerarToken(user, roles);
        return Ok(new { token, usuario = new { user.Id, user.Nombre, user.Correo, roles } });
    }
}