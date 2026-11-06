using System.Security.Claims;
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
    private const long TamanioMaximoImagen = 2 * 1024 * 1024;
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private readonly ContextoAplicacion _ctx;
    private readonly IWebHostEnvironment _entorno;

    public ProductosApiController(ContextoAplicacion ctx, IWebHostEnvironment entorno)
    {
        _ctx = ctx;
        _entorno = entorno;
    }

    // GET api/productos?page=1&pageSize=10&search=pan&empresaId=1
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? empresaId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        var q = _ctx.Productos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.Imagenes)
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(search))
        {
            q = q.Where(p => p.Nombre.Contains(search) ||
                             (p.Descripcion != null && p.Descripcion.Contains(search)));
        }

        if (empresaId.HasValue)
        {
            q = q.Where(p => p.EmpresaId == empresaId.Value);
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Nombre,
                p.Descripcion,
                p.Precio,
                p.Stock,
                p.Activo,
                p.EmpresaId,
                empresa = p.Empresa.Nombre,
                imagen = p.Imagenes
                    .OrderByDescending(i => i.EsPrincipal)
                    .Select(i => i.RutaArchivo)
                    .FirstOrDefault(),
                tieneStock = p.Stock > 0
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            data = items
        });
    }

    // GET api/productos/buscar?q=pan
    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarAsync([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
        {
            return Ok(Array.Empty<object>());
        }

        var resultados = await _ctx.Productos
            .AsNoTracking()
            .Where(p => p.Activo && p.Nombre.Contains(q))
            .OrderBy(p => p.Nombre)
            .Take(10)
            .Select(p => new
            {
                p.Id,
                p.Nombre,
                p.Precio,
                empresa = p.Empresa.Nombre
            })
            .ToListAsync();

        return Ok(resultados);
    }

    // GET api/productos/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var producto = await _ctx.Productos
            .AsNoTracking()
            .Include(p => p.Imagenes)
            .Include(p => p.Empresa)
            .FirstOrDefaultAsync(p => p.Id == id);

        return producto is null ? NotFound() : Ok(producto);
    }

    // POST api/productos (JSON)
    [Authorize(Roles = "COMERCIANTE,SUPER_ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProductoDto dto)
    {
        var empresa = await ObtenerEmpresaAsync(dto.EmpresaId);
        if (empresa is null)
        {
            return BadRequest(new { error = "No se encontró una empresa asociada al usuario." });
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre?.Trim() ?? string.Empty,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock,
            EmpresaId = empresa.Id,
            Activo = true
        };

        if (!ValidarProducto(producto, out var error))
        {
            return BadRequest(new { error });
        }

        _ctx.Productos.Add(producto);
        await _ctx.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, producto);
    }

    // POST api/productos/con-imagen (FormData: nombre, descripcion, precio, stock, Imagen)
    [Authorize(Roles = "COMERCIANTE,SUPER_ADMIN")]
    [HttpPost("con-imagen")]
    public async Task<IActionResult> CrearConImagen([FromForm] ProductoFormDto dto)
    {
        var empresa = await ObtenerEmpresaAsync(dto.EmpresaId);
        if (empresa is null)
        {
            return BadRequest(new { error = "No se encontró una empresa asociada al usuario." });
        }

        var producto = new Producto
        {
            Nombre = dto.Nombre?.Trim() ?? string.Empty,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Precio = dto.Precio,
            Stock = dto.Stock,
            EmpresaId = empresa.Id,
            Activo = true
        };

        if (!ValidarProducto(producto, out var error))
        {
            return BadRequest(new { error });
        }

        if (dto.Imagen is not null && dto.Imagen.Length > 0)
        {
            var errorImagen = ValidarImagen(dto.Imagen);
            if (errorImagen is not null)
            {
                return BadRequest(new { error = errorImagen });
            }
        }

        _ctx.Productos.Add(producto);
        await _ctx.SaveChangesAsync();

        if (dto.Imagen is not null && dto.Imagen.Length > 0)
        {
            var ruta = await GuardarImagenAsync(producto.Id, dto.Imagen);
            _ctx.ImagenesProducto.Add(new ImagenProducto
            {
                ProductoId = producto.Id,
                RutaArchivo = ruta,
                EsPrincipal = true,
                FechaCarga = DateTime.UtcNow
            });
            await _ctx.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(Obtener), new { id = producto.Id }, new
        {
            producto.Id,
            producto.Nombre,
            producto.Descripcion,
            producto.Precio,
            producto.Stock,
            producto.EmpresaId,
            producto.Activo
        });
    }

    // POST api/productos/5/imagen (FormData con IFormFile Imagen)
    [Authorize(Roles = "COMERCIANTE,SUPER_ADMIN")]
    [HttpPost("{id:int}/imagen")]
    public async Task<IActionResult> SubirImagen(int id, [FromForm] IFormFile? Imagen)
    {
        var producto = await _ctx.Productos
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
        {
            return NotFound(new { error = "Producto no encontrado." });
        }

        if (!await PuedeEditarAsync(producto))
        {
            return Forbid();
        }

        if (Imagen is null || Imagen.Length == 0)
        {
            return BadRequest(new { error = "Tenés que adjuntar una imagen." });
        }

        var errorImagen = ValidarImagen(Imagen);
        if (errorImagen is not null)
        {
            return BadRequest(new { error = errorImagen });
        }

        var ruta = await GuardarImagenAsync(producto.Id, Imagen);
        var imagen = new ImagenProducto
        {
            ProductoId = producto.Id,
            RutaArchivo = ruta,
            EsPrincipal = !producto.Imagenes.Any(i => i.EsPrincipal),
            FechaCarga = DateTime.UtcNow
        };

        _ctx.ImagenesProducto.Add(imagen);
        await _ctx.SaveChangesAsync();

        return Ok(new { imagen.Id, imagen.RutaArchivo, imagen.EsPrincipal });
    }

    // PUT api/productos/5
    [Authorize(Roles = "COMERCIANTE,SUPER_ADMIN")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, [FromBody] ProductoDto dto)
    {
        var producto = await _ctx.Productos.FindAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        if (!await PuedeEditarAsync(producto))
        {
            return Forbid();
        }

        producto.Nombre = dto.Nombre?.Trim() ?? string.Empty;
        producto.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        producto.Precio = dto.Precio;
        producto.Stock = dto.Stock;

        if (!ValidarProducto(producto, out var error))
        {
            return BadRequest(new { error });
        }

        await _ctx.SaveChangesAsync();
        return Ok(producto);
    }

    // DELETE api/productos/5
    [Authorize(Roles = "COMERCIANTE,SUPER_ADMIN")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await _ctx.Productos
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto is null)
        {
            return NotFound();
        }

        if (!await PuedeEditarAsync(producto))
        {
            return Forbid();
        }

        // Baja lógica si tiene historial, como pide la consigna.
        var tieneDenuncias = await _ctx.Denuncias.AnyAsync(d => d.ProductoId == id);
        if (tieneDenuncias)
        {
            producto.Activo = false;
            await _ctx.SaveChangesAsync();
            return Ok(new { mensaje = "Dado de baja por tener historial" });
        }

        foreach (var imagen in producto.Imagenes)
        {
            EliminarArchivo(imagen.RutaArchivo);
        }

        _ctx.Productos.Remove(producto);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // ----------------------------- Auxiliares -----------------------------

    /// <summary>
    /// Resuelve la empresa del usuario logueado (cookie o JWT). Si el usuario no
    /// tiene empresa propia, se usa la empresa indicada o la primera disponible.
    /// El cliente nunca necesita enviar EmpresaId.
    /// </summary>
    private async Task<Empresa?> ObtenerEmpresaAsync(int? empresaIdSolicitada = null)
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var usuarioId))
        {
            var empresaPropia = await _ctx.Empresas
                .OrderBy(e => e.Id)
                .FirstOrDefaultAsync(e => e.PropietarioId == usuarioId);

            if (empresaPropia is not null)
            {
                return empresaPropia;
            }
        }

        if (empresaIdSolicitada.HasValue)
        {
            var empresaSolicitada = await _ctx.Empresas.FindAsync(empresaIdSolicitada.Value);
            if (empresaSolicitada is not null)
            {
                return empresaSolicitada;
            }
        }

        return await _ctx.Empresas.OrderBy(e => e.Id).FirstOrDefaultAsync();
    }

    private async Task<bool> PuedeEditarAsync(Producto producto)
    {
        if (User.IsInRole("SUPER_ADMIN"))
        {
            return true;
        }

        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var usuarioId))
        {
            return false;
        }

        return await _ctx.Empresas.AnyAsync(e => e.Id == producto.EmpresaId && e.PropietarioId == usuarioId);
    }

    private static bool ValidarProducto(Producto producto, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(producto.Nombre))
        {
            error = "El nombre del producto es obligatorio.";
            return false;
        }

        if (producto.Nombre.Length > 150)
        {
            error = "El nombre del producto no puede superar los 150 caracteres.";
            return false;
        }

        if (producto.Precio < 0)
        {
            error = "El precio no puede ser negativo.";
            return false;
        }

        if (producto.Stock < 0)
        {
            error = "El stock no puede ser negativo.";
            return false;
        }

        return true;
    }

    private static string? ValidarImagen(IFormFile archivo)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(extension))
        {
            return "La imagen debe ser JPG, JPEG, PNG, WEBP o GIF.";
        }

        if (archivo.Length > TamanioMaximoImagen)
        {
            return "La imagen supera el máximo de 2 MB.";
        }

        return null;
    }

    private async Task<string> GuardarImagenAsync(int productoId, IFormFile archivo)
    {
        var carpeta = Path.Combine(_entorno.WebRootPath, "imagenes", "productos");
        Directory.CreateDirectory(carpeta);

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreArchivo = $"producto_{productoId}_{Guid.NewGuid():N}{extension}";
        var rutaFisica = Path.Combine(carpeta, nombreArchivo);

        await using (var stream = System.IO.File.Create(rutaFisica))
        {
            await archivo.CopyToAsync(stream);
        }

        return $"/imagenes/productos/{nombreArchivo}";
    }

    private void EliminarArchivo(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            var raiz = Path.GetFullPath(_entorno.WebRootPath) + Path.DirectorySeparatorChar;
            var ruta = Path.GetFullPath(Path.Combine(
                _entorno.WebRootPath,
                url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            if (ruta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }
        }
        catch (IOException)
        {
            // Si el archivo no se puede borrar, la baja del producto igual se completa.
        }
    }
}

/// <summary>DTO para crear/editar productos por JSON.</summary>
public record ProductoDto(
    string Nombre,
    string? Descripcion,
    decimal Precio,
    int Stock,
    int? EmpresaId = null);

/// <summary>DTO para crear productos con imagen (multipart/form-data).</summary>
public class ProductoFormDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int? EmpresaId { get; set; }
    public IFormFile? Imagen { get; set; }
}
