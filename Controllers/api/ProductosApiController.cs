using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Controllers.Api;

[ApiController]
[Route("api/productos")]
public class ProductosApiController : ControllerBase
{
    private readonly ContextoAplicacion _ctx;
    public ProductosApiController(ContextoAplicacion ctx) => _ctx = ctx;

    // GET api/productos?page=1&pageSize=10&search=pan&empresaId=1
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? empresaId = null)
    {
        if(page < 1) page = 1;
        if(pageSize < 1 || pageSize > 50) pageSize = 10;

        var q = _ctx.Productos.Include(p => p.Empresa).Include(p => p.Imagenes).Where(p => p.Activo);
        if(!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.Nombre.Contains(search) || (p.Descripcion != null && p.Descripcion.Contains(search)));
        if(empresaId.HasValue) q = q.Where(p => p.EmpresaId == empresaId.Value);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(p => p.Id)
            .Skip((page-1)*pageSize).Take(pageSize)
            .Select(p => new { 
                p.Id, 
                p.Nombre, 
                p.Precio, 
                p.Stock, 
                p.Activo,
                p.EmpresaId, 
                empresa = p.Empresa.Nombre, 
                imagen = p.Imagenes.FirstOrDefault(i => i.EsPrincipal) != null ? p.Imagenes.FirstOrDefault(i => i.EsPrincipal)!.RutaArchivo : null,
                tieneStock = p.Stock > 0
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, totalPages = (int)Math.Ceiling(total/(double)pageSize), data = items });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var p = await _ctx.Productos.Include(x=>x.Imagenes).Include(x=>x.Empresa).FirstOrDefaultAsync(x=>x.Id==id);
        return p == null ? NotFound() : Ok(p);
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProductoDto dto)
    {
        if(!ModelState.IsValid) return BadRequest(ModelState);
        var empresa = await _ctx.Empresas.FirstOrDefaultAsync(e => e.Id == dto.EmpresaId);
        if(empresa == null) return BadRequest(new { error = "Empresa no encontrada" });

        var producto = new Producto {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion,
            Precio = dto.Precio,
            Stock = dto.Stock,
            EmpresaId = dto.EmpresaId,
            Activo = true
        };
        _ctx.Productos.Add(producto);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Editar(int id, [FromBody] ProductoDto dto)
    {
        var p = await _ctx.Productos.FindAsync(id);
        if(p == null) return NotFound();
        p.Nombre = dto.Nombre;
        p.Descripcion = dto.Descripcion;
        p.Precio = dto.Precio;
        p.Stock = dto.Stock;
        await _ctx.SaveChangesAsync();
        return Ok(p);
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles="COMERCIANTE,SUPER_ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id){
        var p = await _ctx.Productos.FindAsync(id);
        if(p==null) return NotFound();
        // Baja lógica si tiene relaciones, como pide la consigna
        var tieneDenuncias = await _ctx.Set<Denuncia>().AnyAsync(d => d.ProductoId == id);
        if(tieneDenuncias){
            p.Activo = false;
            await _ctx.SaveChangesAsync();
            return Ok(new { mensaje = "Dado de baja por tener historial" });
        }
        _ctx.Productos.Remove(p);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // BÚSQUEDA ASÍNCRONA - Requisito de consigna
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarAsync([FromQuery] string q)
    {
        if(string.IsNullOrWhiteSpace(q) || q.Length < 2) return Ok(Array.Empty<object>());
        var res = await _ctx.Productos.Where(p => p.Activo && p.Nombre.Contains(q)).Take(10)
            .Select(p => new { p.Id, p.Nombre, p.Precio, empresa = p.Empresa.Nombre }).ToListAsync();
        return Ok(res);
    }
}
public record ProductoDto(string Nombre, string? Descripcion, decimal Precio, int Stock, int EmpresaId);
