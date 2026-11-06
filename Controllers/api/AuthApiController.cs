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
            .Include(u => u.Roles) // si tenés tabla intermedia UsuarioRol
            .FirstOrDefaultAsync(u => u.Correo == dto.Email);
            
        if(usuario == null) return Unauthorized(new { error = "Credenciales inválidas" });

        // TODO: verifica con BCrypt - por ahora lo dejamos pasar
        // bool passOk = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);

        // Armamos la lista de roles como string
        var roles = new List<string>();
        // Si tu Usuario tiene propiedad Rol:
        // roles.Add(usuario.Rol.ToString());
        // Si tenés relación muchos a muchos, mapeala acá
        
        // Fallback para que no falle si aún no tenés roles cargados
        if(!roles.Any()) roles.Add("COMERCIANTE"); 

        var token = _jwt.GenerarToken(usuario, roles);
        return Ok(new { token, usuario = new { usuario.Id, usuario.Nombre, usuario.Correo, roles }, expiresIn = 28800 });
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "Bearer")]
    public IActionResult Me()
    {
        return Ok(new { 
            id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            nombre = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            rol = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
        });
    }
}