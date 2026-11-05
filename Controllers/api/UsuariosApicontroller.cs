using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;

namespace MultiTenantCSharpVargas.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class UsuariosApiController : ControllerBase {
    private readonly ContextoAplicacion _ctx;
    public UsuariosApiController(ContextoAplicacion ctx) => _ctx = ctx;
    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q){
        if(string.IsNullOrWhiteSpace(q)) return Ok(Array.Empty<object>());
        var users = await _ctx.Usuarios.Where(u => u.Nombre.Contains(q) || u.Correo.Contains(q)).Take(10)
            .Select(u => new { u.Id, u.Nombre, u.Correo, u.AvatarUrl }).ToListAsync();
        return Ok(users);
    }
}