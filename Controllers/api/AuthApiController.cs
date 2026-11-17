using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly ContextoAplicacion _ctx;
    private readonly ServicioJwt _jwt;

    public AuthApiController(ContextoAplicacion ctx, ServicioJwt jwt)
    {
        _ctx = ctx;
        _jwt = jwt;
    }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var usuario = await _ctx.Usuarios
            .Include(u => u.UsuarioRoles)
            .ThenInclude(ur => ur.Rol)
            .FirstOrDefaultAsync(u => u.Correo == dto.Email);

        if (usuario is null || !PasswordHasher.Verify(dto.Password, usuario.ContrasenaHash))
        {
            return Unauthorized(new { error = "Credenciales inválidas" });
        }

        var roles = usuario.UsuarioRoles
            .Where(ur => ur.Rol.Activo)
            .Select(ur => ur.Rol.Codigo)
            .ToList();

        var token = _jwt.GenerarToken(usuario, roles);
        return Ok(new { token, usuario = new { usuario.Id, usuario.Nombre, usuario.Correo, roles }, expiresIn = 28800 });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}
