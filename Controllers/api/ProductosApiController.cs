using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;

namespace MultiTenantCSharpVargas.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class ProductosApiController : ControllerBase
{
    private readonly ContextoAplicacion _ctx;
    public ProductosApiController(ContextoAplicacion ctx) => _ctx = ctx;

    // GET api/productos?page=1&pageSize=10&search=pan
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? empresaId = null)
    {
        var q = _ctx.Productos.Include(p => p.Empresa).Where(p => p.Activo);
        if(!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Nombre.Contains(search) || p.Descripcion.Contains(search));
        if(empresaId.HasValue) q = q.Where(p => p.EmpresaId == empresaId.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.Id)
            .Skip((page-1)*pageSize).Take(pageSize)
            .Select(p => new { p.Id, p.Nombre, p.Precio, p.Stock, p.EmpresaId, empresa = p.Empresa.Nombre, imagen = p.Imagenes.FirstOrDefault(i => i.EsPrincipal).RutaArchivo })
            .ToListAsync();

        return Ok(new { total, page, pageSize, totalPages = (int)Math.Ceiling(total/(double)pageSize), data = items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var p = await _ctx.Productos.Include(p=>p.Imagenes).FirstOrDefaultAsync(x=>x.Id==id);
        return p == null ? NotFound() : Ok(p);
    }

    [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProductoDto dto)
    {
        // tu lógica de creación
        return Ok(new { mensaje = "Creado" });
    }

    [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] ProductoDto dto) => Ok();

    [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id){
        var p = await _ctx.Productos.FindAsync(id);
        if(p==null) return NotFound();
        _ctx.Productos.Remove(p);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // BÚSQUEDA ASÍNCRONA - Para el requisito
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarAsync([FromQuery] string q)
    {
        if(string.IsNullOrWhiteSpace(q) || q.Length < 2) return Ok(Array.Empty<object>());
        var res = await _ctx.Productos.Where(p => p.Nombre.Contains(q)).Take(10)
            .Select(p => new { p.Id, p.Nombre, p.Precio }).ToListAsync();
        return Ok(res);
    }
}
public record ProductoDto(string Nombre, string Descripcion, decimal Precio, int Stock, int EmpresaId);