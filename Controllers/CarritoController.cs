using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.ViewModels;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "CLIENTE")]
public class CarritoController(ContextoAplicacion db) : Controller
{
    private const int CantidadMaxima = 99;

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var modelo = new CarritoVista
        {
            Items = await db.CarritoItems.AsNoTracking()
                .Where(item => item.Carrito.UsuarioId == UsuarioId)
                .OrderBy(item => item.Id)
                .Select(item => new CarritoItemVista
                {
                    Id = item.Id,
                    ProductoId = item.ProductoId,
                    EmpresaId = item.EmpresaId,
                    Nombre = item.Nombre,
                    EmpresaNombre = item.Producto.Empresa.Nombre,
                    PrecioUnitario = item.PrecioUnitario,
                    Cantidad = item.Cantidad,
                    Subtotal = item.Subtotal,
                    Stock = item.Producto.Stock,
                    RutaImagen = item.Producto.Imagenes
                        .OrderByDescending(imagen => imagen.EsPrincipal)
                        .Select(imagen => imagen.RutaArchivo)
                        .FirstOrDefault()
                })
                .ToListAsync()
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
    {
        var producto = await db.Productos.AsNoTracking()
            .Include(producto => producto.Empresa)
            .FirstOrDefaultAsync(producto => producto.Id == productoId);

        if (producto is null || !producto.Activo || producto.Empresa.Estado != EstadoEmpresa.Activa)
        {
            TempData["Error"] = "Ese producto no está disponible en este momento.";
            return RedirectToAction(nameof(Index));
        }

        if (cantidad < 1)
        {
            cantidad = 1;
        }

        var carrito = await ObtenerOCrearCarritoAsync();

        // Si el producto ya está en el carrito se suma la cantidad en vez de duplicar la fila.
        var item = await db.CarritoItems
            .FirstOrDefaultAsync(item => item.CarritoId == carrito.Id && item.ProductoId == producto.Id);

        var cantidadActual = item?.Cantidad ?? 0;
        var cantidadFinal = Math.Min(cantidadActual + cantidad, CantidadMaxima);

        if (cantidadFinal > producto.Stock)
        {
            var disponibles = Math.Max(0, producto.Stock - cantidadActual);
            TempData["Error"] = disponibles == 0
                ? $"Ya tenés todas las unidades disponibles de \"{producto.Nombre}\" en el carrito."
                : $"Solo podés agregar {disponibles} unidad(es) más de \"{producto.Nombre}\".";
            return RedirectToAction(nameof(Index));
        }

        if (item is null)
        {
            db.CarritoItems.Add(new CarritoItem
            {
                CarritoId = carrito.Id,
                EmpresaId = producto.EmpresaId,
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                PrecioUnitario = producto.Precio,
                Cantidad = cantidadFinal,
                Subtotal = producto.Precio * cantidadFinal
            });
        }
        else
        {
            // Se toma el precio actual del producto por si el comerciante lo cambió.
            item.Cantidad = cantidadFinal;
            item.PrecioUnitario = producto.Precio;
            item.Subtotal = producto.Precio * cantidadFinal;
        }

        carrito.FechaUltimaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync();

        TempData["Mensaje"] = $"Agregaste \"{producto.Nombre}\" al carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarCantidad(int id, int cantidad)
    {
        var item = await db.CarritoItems
            .Include(item => item.Producto)
            .FirstOrDefaultAsync(item => item.Id == id && item.Carrito.UsuarioId == UsuarioId);

        if (item is null)
        {
            return NotFound();
        }

        if (item.Producto.Stock < 1)
        {
            db.CarritoItems.Remove(item);
            await db.SaveChangesAsync();
            TempData["Error"] = $"\"{item.Nombre}\" se quedó sin stock y lo quitamos del carrito.";
            return RedirectToAction(nameof(Index));
        }

        cantidad = Math.Clamp(cantidad, 1, CantidadMaxima);

        if (cantidad > item.Producto.Stock)
        {
            cantidad = item.Producto.Stock;
            TempData["Error"] = $"Solo hay {item.Producto.Stock} unidad(es) de \"{item.Nombre}\" disponibles.";
        }

        item.Cantidad = cantidad;
        item.Subtotal = item.PrecioUnitario * cantidad;

        var carrito = await db.Carritos.FindAsync(item.CarritoId);
        if (carrito is not null)
        {
            carrito.FechaUltimaActualizacion = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        TempData["Mensaje"] = $"Actualizaste la cantidad de \"{item.Nombre}\".";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Quitar(int id)
    {
        var item = await db.CarritoItems
            .FirstOrDefaultAsync(item => item.Id == id && item.Carrito.UsuarioId == UsuarioId);

        if (item is null)
        {
            return NotFound();
        }

        db.CarritoItems.Remove(item);
        await db.SaveChangesAsync();

        TempData["Mensaje"] = $"Quitaste \"{item.Nombre}\" del carrito.";
        return RedirectToAction(nameof(Index));
    }

    // Cada cliente tiene un único carrito: si no existe, se crea en este momento.
    private async Task<Carrito> ObtenerOCrearCarritoAsync()
    {
        var carrito = await db.Carritos
            .FirstOrDefaultAsync(carrito => carrito.UsuarioId == UsuarioId);

        if (carrito is null)
        {
            carrito = new Carrito
            {
                UsuarioId = UsuarioId,
                FechaCreacion = DateTime.UtcNow
            };
            db.Carritos.Add(carrito);
            await db.SaveChangesAsync();
        }

        return carrito;
    }
}
