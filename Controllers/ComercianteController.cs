using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Models.Vistas;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "COMERCIANTE")]
public class ComercianteController(
    ContextoAplicacion db,
    IWebHostEnvironment entorno,
    ILogger<ComercianteController> registro) : Controller
{
    private const long TamanioMaximoImagen = 2 * 1024 * 1024;
    private const int MaximoImagenes = 6;
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var empresa = await db.Empresas
            .AsNoTracking()
            .Include(e => e.Productos)
            .ThenInclude(p => p.Imagenes)
            .FirstOrDefaultAsync(e => e.PropietarioId == UsuarioId);
        return View(empresa);
    }

    public async Task<IActionResult> CrearEmpresa()
    {
        if (await db.Empresas.AnyAsync(e => e.PropietarioId == UsuarioId))
        {
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearEmpresa(Empresa empresa)
    {
        if (await db.Empresas.AnyAsync(e => e.PropietarioId == UsuarioId))
        {
            return RedirectToAction(nameof(Index));
        }

        empresa.Nombre = empresa.Nombre?.Trim() ?? string.Empty;
        empresa.Descripcion = string.IsNullOrWhiteSpace(empresa.Descripcion) ? null : empresa.Descripcion.Trim();
        empresa.Direccion = empresa.Direccion?.Trim() ?? string.Empty;
        empresa.Localidad = empresa.Localidad?.Trim() ?? string.Empty;
        empresa.Telefono = string.IsNullOrWhiteSpace(empresa.Telefono) ? null : empresa.Telefono.Trim();
        empresa.MediosDePago = string.IsNullOrWhiteSpace(empresa.MediosDePago) ? null : empresa.MediosDePago.Trim();

        if (!ModelState.IsValid)
        {
            return View(empresa);
        }

        // Ignora cualquier identificador enviado por el navegador.
        empresa.Id = 0;
        empresa.PropietarioId = UsuarioId;
        empresa.Estado = EstadoEmpresa.Pendiente;
        empresa.FechaAlta = DateTime.UtcNow;

        try
        {
            db.Empresas.Add(empresa);
            await db.SaveChangesAsync();

            // Vincula el rol COMERCIANTE del usuario a la empresa creada.
            var usuarioRol = await db.UsuarioRoles
                .Include(ur => ur.Rol)
                .FirstOrDefaultAsync(ur => ur.UsuarioId == UsuarioId && ur.Rol.Codigo == "COMERCIANTE");
            if (usuarioRol != null)
            {
                usuarioRol.EmpresaId = empresa.Id;
                await db.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo crear la empresa del usuario {UsuarioId}.", UsuarioId);
            ModelState.AddModelError(string.Empty, "No pudimos crear la empresa. Intentá nuevamente.");
            return View(empresa);
        }

        TempData["Mensaje"] = "Empresa creada. Queda pendiente de aprobación por el Super Administrador.";
        return RedirectToAction(nameof(Index));
    }

    // ----------------------------- Productos -----------------------------

    public async Task<IActionResult> Productos()
    {
        var empresa = await ObtenerEmpresaAsync();
        if (empresa is null)
        {
            TempData["Error"] = "Primero tenés que crear tu empresa para poder publicar productos.";
            return RedirectToAction(nameof(Index));
        }

        var productos = await db.Productos
            .AsNoTracking()
            .Where(p => p.EmpresaId == empresa.Id)
            .Include(p => p.Empresa)
            .Include(p => p.Imagenes)
            .OrderByDescending(p => p.Activo)
            .ThenBy(p => p.Nombre)
            .ToListAsync();

        ViewBag.Empresa = empresa;
        return View(productos.Select(Mapear).ToList());
    }

    public async Task<IActionResult> CrearProducto()
    {
        if (await ObtenerEmpresaAsync() is null)
        {
            TempData["Error"] = "Primero tenés que crear tu empresa para poder publicar productos.";
            return RedirectToAction(nameof(Index));
        }

        return View(new Producto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearProducto(Producto producto, List<IFormFile>? imagenes)
    {
        var empresa = await ObtenerEmpresaAsync();
        if (empresa is null)
        {
            TempData["Error"] = "Primero tenés que crear tu empresa para poder publicar productos.";
            return RedirectToAction(nameof(Index));
        }

        Normalizar(producto);
        ValidarProducto(producto);

        var errorImagenes = ValidarImagenes(imagenes);
        if (errorImagenes is not null)
        {
            ModelState.AddModelError(string.Empty, errorImagenes);
        }

        if (!ModelState.IsValid)
        {
            return View(producto);
        }

        producto.Id = 0;
        producto.EmpresaId = empresa.Id;
        producto.Activo = true;

        try
        {
            db.Productos.Add(producto);
            await db.SaveChangesAsync();

            await GuardarImagenesAsync(producto, imagenes);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo crear el producto para la empresa {EmpresaId}.", empresa.Id);
            ModelState.AddModelError(string.Empty, "No pudimos guardar el producto. Intentá nuevamente.");
            return View(producto);
        }

        TempData["Mensaje"] = "Producto creado correctamente.";
        return RedirectToAction(nameof(Productos));
    }

    public async Task<IActionResult> EditarProducto(int id)
    {
        var producto = await ObtenerProductoPropioAsync(id);
        return producto is null ? NotFound() : View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarProducto(int id, Producto producto, List<IFormFile>? imagenes)
    {
        var productoDb = await ObtenerProductoPropioAsync(id);
        if (productoDb is null)
        {
            return NotFound();
        }

        productoDb.Nombre = producto.Nombre;
        productoDb.Descripcion = producto.Descripcion;
        productoDb.Precio = producto.Precio;
        productoDb.Stock = producto.Stock;

        Normalizar(productoDb);
        ValidarProducto(productoDb);

        var errorImagenes = ValidarImagenes(imagenes);
        if (errorImagenes is not null)
        {
            ModelState.AddModelError(string.Empty, errorImagenes);
        }

        if (!ModelState.IsValid)
        {
            return View(productoDb);
        }

        try
        {
            await db.SaveChangesAsync();

            await GuardarImagenesAsync(productoDb, imagenes);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo editar el producto {ProductoId}.", id);
            ModelState.AddModelError(string.Empty, "No pudimos guardar los cambios. Intentá nuevamente.");
            return View(productoDb);
        }

        TempData["Mensaje"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstadoProducto(int id)
    {
        var producto = await ObtenerProductoPropioAsync(id, incluirImagenes: false);
        if (producto is null)
        {
            return NotFound();
        }

        producto.Activo = !producto.Activo;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo cambiar el estado del producto {ProductoId}.", id);
            TempData["Error"] = "No pudimos cambiar el estado del producto.";
            return RedirectToAction(nameof(Productos));
        }

        TempData["Mensaje"] = producto.Activo ? "Producto activado." : "Producto dado de baja.";
        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarProducto(int id)
    {
        var producto = await ObtenerProductoPropioAsync(id);
        if (producto is null)
        {
            return NotFound();
        }

        var tieneReferencias =
            await db.PedidoItems.AnyAsync(item => item.ProductoId == id) ||
            await db.CarritoItems.AnyAsync(item => item.ProductoId == id) ||
            await db.Denuncias.AnyAsync(denuncia => denuncia.ProductoId == id);

        try
        {
            if (tieneReferencias)
            {
                producto.Activo = false;
                await db.SaveChangesAsync();
                TempData["Mensaje"] = "El producto tiene pedidos o denuncias asociadas, así que se dio de baja sin borrar el historial.";
            }
            else
            {
                foreach (var imagen in producto.Imagenes)
                {
                    EliminarArchivo(imagen.RutaArchivo);
                }

                db.ImagenesProducto.RemoveRange(producto.Imagenes);
                db.Productos.Remove(producto);
                await db.SaveChangesAsync();
                TempData["Mensaje"] = "Producto eliminado correctamente.";
            }
        }
        catch (DbUpdateException ex)
        {
            registro.LogError(ex, "No se pudo eliminar el producto {ProductoId}.", id);
            TempData["Error"] = "No pudimos eliminar el producto porque tiene datos asociados. Se dio de baja.";
            producto.Activo = false;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Productos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        var imagen = await db.ImagenesProducto
            .Include(i => i.Producto)
            .ThenInclude(p => p.Imagenes)
            .FirstOrDefaultAsync(i => i.Id == id);

        var empresa = await ObtenerEmpresaAsync();
        if (imagen is null || empresa is null || imagen.Producto.EmpresaId != empresa.Id)
        {
            return NotFound();
        }

        var producto = imagen.Producto;
        EliminarArchivo(imagen.RutaArchivo);
        db.ImagenesProducto.Remove(imagen);

        if (imagen.EsPrincipal)
        {
            var reemplazo = producto.Imagenes
                .Where(otra => otra.Id != imagen.Id)
                .OrderBy(otra => otra.Id)
                .FirstOrDefault();

            if (reemplazo is not null)
            {
                reemplazo.EsPrincipal = true;
            }
        }

        await db.SaveChangesAsync();
        TempData["Mensaje"] = "Imagen eliminada.";
        return RedirectToAction(nameof(EditarProducto), new { id = producto.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarImagenPrincipal(int id)
    {
        var imagen = await db.ImagenesProducto
            .Include(i => i.Producto)
            .ThenInclude(p => p.Imagenes)
            .FirstOrDefaultAsync(i => i.Id == id);

        var empresa = await ObtenerEmpresaAsync();
        if (imagen is null || empresa is null || imagen.Producto.EmpresaId != empresa.Id)
        {
            return NotFound();
        }

        foreach (var otra in imagen.Producto.Imagenes)
        {
            otra.EsPrincipal = otra.Id == imagen.Id;
        }

        await db.SaveChangesAsync();
        TempData["Mensaje"] = "Imagen principal actualizada.";
        return RedirectToAction(nameof(EditarProducto), new { id = imagen.ProductoId });
    }

    // ----------------------------- Auxiliares -----------------------------

    private async Task<Empresa?> ObtenerEmpresaAsync() =>
        await db.Empresas.AsNoTracking().FirstOrDefaultAsync(e => e.PropietarioId == UsuarioId);

    private async Task<Producto?> ObtenerProductoPropioAsync(int id, bool incluirImagenes = true)
    {
        var empresa = await ObtenerEmpresaAsync();
        if (empresa is null)
        {
            return null;
        }

        IQueryable<Producto> consulta = db.Productos;
        if (incluirImagenes)
        {
            consulta = consulta.Include(p => p.Imagenes);
        }

        return await consulta.FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresa.Id);
    }

    private static void Normalizar(Producto producto)
    {
        producto.Nombre = producto.Nombre?.Trim() ?? string.Empty;
        producto.Descripcion = string.IsNullOrWhiteSpace(producto.Descripcion) ? null : producto.Descripcion.Trim();
    }

    private void ValidarProducto(Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
        {
            ModelState.AddModelError(nameof(Producto.Nombre), "El nombre del producto es obligatorio.");
        }

        if (producto.Precio < 0)
        {
            ModelState.AddModelError(nameof(Producto.Precio), "El precio no puede ser negativo.");
        }

        if (producto.Stock < 0)
        {
            ModelState.AddModelError(nameof(Producto.Stock), "El stock no puede ser negativo.");
        }
    }

    private static string? ValidarImagenes(IEnumerable<IFormFile>? imagenes)
    {
        var archivos = imagenes?.Where(archivo => archivo.Length > 0).ToList() ?? [];
        if (archivos.Count == 0)
        {
            return null;
        }

        if (archivos.Count > MaximoImagenes)
        {
            return $"Podés subir como máximo {MaximoImagenes} imágenes por producto.";
        }

        foreach (var archivo in archivos)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension))
            {
                return $"La imagen \"{archivo.FileName}\" debe ser JPG, PNG, WEBP o GIF.";
            }

            if (archivo.Length > TamanioMaximoImagen)
            {
                return $"La imagen \"{archivo.FileName}\" supera el máximo de 2 MB.";
            }
        }

        return null;
    }

    private async Task GuardarImagenesAsync(Producto producto, IEnumerable<IFormFile>? imagenes)
    {
        var archivos = imagenes?.Where(archivo => archivo.Length > 0).ToList() ?? [];
        if (archivos.Count == 0)
        {
            return;
        }

        var carpeta = Path.Combine(entorno.WebRootPath, "imagenes", "productos");
        Directory.CreateDirectory(carpeta);

        var yaTienePrincipal = await db.ImagenesProducto
            .AnyAsync(imagen => imagen.ProductoId == producto.Id && imagen.EsPrincipal);

        foreach (var archivo in archivos.Take(MaximoImagenes))
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var nombreArchivo = $"producto_{producto.Id}_{Guid.NewGuid():N}{extension}";
            var ruta = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = System.IO.File.Create(ruta))
            {
                await archivo.CopyToAsync(stream);
            }

            db.ImagenesProducto.Add(new ImagenProducto
            {
                ProductoId = producto.Id,
                RutaArchivo = $"/imagenes/productos/{nombreArchivo}",
                EsPrincipal = !yaTienePrincipal,
                FechaCarga = DateTime.UtcNow
            });

            yaTienePrincipal = true;
        }
    }

    private void EliminarArchivo(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            var raiz = Path.GetFullPath(entorno.WebRootPath) + Path.DirectorySeparatorChar;
            var ruta = Path.GetFullPath(Path.Combine(
                entorno.WebRootPath,
                url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

            if (ruta.StartsWith(raiz, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(ruta))
            {
                System.IO.File.Delete(ruta);
            }
        }
        catch (IOException ex)
        {
            registro.LogWarning(ex, "No se pudo eliminar el archivo {Ruta}.", url);
        }
    }

    private static TarjetaProductoVista Mapear(Producto producto) => new()
    {
        Id = producto.Id,
        EmpresaId = producto.EmpresaId,
        Nombre = producto.Nombre,
        Descripcion = producto.Descripcion,
        Precio = producto.Precio,
        Stock = producto.Stock,
        Activo = producto.Activo,
        EmpresaNombre = producto.Empresa?.Nombre ?? string.Empty,
        Localidad = producto.Empresa?.Localidad ?? string.Empty,
        RutaImagen = producto.Imagenes
            .OrderByDescending(imagen => imagen.EsPrincipal)
            .Select(imagen => imagen.RutaArchivo)
            .FirstOrDefault()
    };
}
