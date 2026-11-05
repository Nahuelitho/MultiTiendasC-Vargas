using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Models.Vistas;

namespace MultiTenantCSharpVargas.Controllers;

public class InicioController(ContextoAplicacion db, ILogger<InicioController> registro) : Controller
{
    private const int TamanoPagina = 9;

    public async Task<IActionResult> Inicio(string vista = "empresas", string? q = null, int pagina = 1)
    {
        vista = string.Equals(vista, "productos", StringComparison.OrdinalIgnoreCase) ? "productos" : "empresas";
        q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();
        pagina = pagina < 1 ? 1 : pagina;

        var modelo = new CatalogoVista
        {
            Vista = vista,
            Busqueda = q,
            Pagina = pagina
        };

        try
        {
            if (modelo.ViendoEmpresas)
            {
                await CargarEmpresasAsync(modelo, q, pagina);
            }
            else
            {
                await CargarProductosAsync(modelo, q, pagina);
            }
        }
        catch (Exception ex)
        {
            registro.LogError(ex, "No se pudo cargar el catálogo público.");
            modelo.MensajeError = "No pudimos cargar el catálogo en este momento. Verificá la conexión con la base de datos.";
            modelo.Empresas.Clear();
            modelo.Productos.Clear();
            modelo.TotalRegistros = 0;
            modelo.TotalPaginas = 1;
        }

        return View(modelo);
    }

    private async Task CargarEmpresasAsync(CatalogoVista modelo, string? q, int pagina)
    {
        var consulta = db.Empresas.AsNoTracking()
            .Where(empresa => empresa.Estado == EstadoEmpresa.Activa);

        if (q is not null)
        {
            consulta = consulta.Where(empresa =>
                empresa.Nombre.Contains(q) ||
                empresa.Localidad.Contains(q) ||
                (empresa.Descripcion != null && empresa.Descripcion.Contains(q)));
        }

        var total = await consulta.CountAsync();
        modelo.TotalRegistros = total;
        modelo.TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        modelo.Pagina = Math.Min(pagina, modelo.TotalPaginas);

        modelo.Empresas = await consulta
            .OrderBy(empresa => empresa.Nombre)
            .Skip((modelo.Pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .Select(empresa => new TarjetaEmpresaVista
            {
                Id = empresa.Id,
                Nombre = empresa.Nombre,
                Descripcion = empresa.Descripcion,
                Direccion = empresa.Direccion,
                Localidad = empresa.Localidad,
                Telefono = empresa.Telefono,
                MediosDePago = empresa.MediosDePago,
                OfreceDelivery = empresa.OfreceDelivery,
                CantidadProductos = empresa.Productos.Count(producto => producto.Activo),
                CantidadValoraciones = empresa.Valoraciones.Count(),
                PromedioValoracion = empresa.Valoraciones.Any()
                    ? empresa.Valoraciones.Average(valoracion => (double)valoracion.Puntuacion)
                    : 0
            })
            .ToListAsync();
    }

    private async Task CargarProductosAsync(CatalogoVista modelo, string? q, int pagina)
    {
        var consulta = db.Productos.AsNoTracking()
            .Where(producto => producto.Activo && producto.Empresa.Estado == EstadoEmpresa.Activa);

        if (q is not null)
        {
            consulta = consulta.Where(producto =>
                producto.Nombre.Contains(q) ||
                producto.Empresa.Nombre.Contains(q) ||
                producto.Empresa.Localidad.Contains(q));
        }

        var total = await consulta.CountAsync();
        modelo.TotalRegistros = total;
        modelo.TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        modelo.Pagina = Math.Min(pagina, modelo.TotalPaginas);

        modelo.Productos = await consulta
            .OrderBy(producto => producto.Nombre)
            .Skip((modelo.Pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .Select(producto => new TarjetaProductoVista
            {
                Id = producto.Id,
                EmpresaId = producto.EmpresaId,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Activo = producto.Activo,
                EmpresaNombre = producto.Empresa.Nombre,
                Localidad = producto.Empresa.Localidad,
                RutaImagen = producto.Imagenes
                    .OrderByDescending(imagen => imagen.EsPrincipal)
                    .Select(imagen => imagen.RutaArchivo)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<IActionResult> Empresa(int id)
    {
        var empresa = await db.Empresas.AsNoTracking()
            .Where(e => e.Id == id && e.Estado == EstadoEmpresa.Activa)
            .Select(e => new TarjetaEmpresaVista
            {
                Id = e.Id,
                Nombre = e.Nombre,
                Descripcion = e.Descripcion,
                Direccion = e.Direccion,
                Localidad = e.Localidad,
                Telefono = e.Telefono,
                MediosDePago = e.MediosDePago,
                OfreceDelivery = e.OfreceDelivery
            })
            .FirstOrDefaultAsync();

        if (empresa is null)
        {
            return NotFound();
        }

        // Los agregados se calculan por separado: combinar varios COUNT/AVG
        // con una subconsulta de imagen en una sola consulta no lo traduce Pomelo.
        empresa.CantidadProductos = await db.Productos
            .CountAsync(producto => producto.EmpresaId == id && producto.Activo);
        empresa.CantidadValoraciones = await db.Valoraciones
            .CountAsync(valoracion => valoracion.EmpresaId == id);
        empresa.PromedioValoracion = empresa.CantidadValoraciones > 0
            ? await db.Valoraciones
                .Where(valoracion => valoracion.EmpresaId == id)
                .AverageAsync(valoracion => (double)valoracion.Puntuacion)
            : 0;

        // Se consulta aparte para que Pomelo traduzca correctamente la
        // subconsulta de la imagen principal.
        var productos = await db.Productos.AsNoTracking()
            .Where(producto => producto.EmpresaId == id && producto.Activo)
            .OrderBy(producto => producto.Nombre)
            .Select(producto => new TarjetaProductoVista
            {
                Id = producto.Id,
                EmpresaId = producto.EmpresaId,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Activo = producto.Activo,
                EmpresaNombre = empresa.Nombre,
                Localidad = empresa.Localidad,
                RutaImagen = producto.Imagenes
                    .OrderByDescending(imagen => imagen.EsPrincipal)
                    .Select(imagen => imagen.RutaArchivo)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return View(new DetalleEmpresaVista
        {
            Empresa = empresa,
            Productos = productos
        });
    }

    public async Task<IActionResult> Producto(int id)
    {
        var producto = await db.Productos.AsNoTracking()
            .Where(p => p.Id == id && p.Activo && p.Empresa.Estado == EstadoEmpresa.Activa)
            .Select(p => new DetalleProductoVista
            {
                Producto = new TarjetaProductoVista
                {
                    Id = p.Id,
                    EmpresaId = p.EmpresaId,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    EmpresaNombre = p.Empresa.Nombre,
                    Localidad = p.Empresa.Localidad,
                    RutaImagen = p.Imagenes
                        .OrderByDescending(imagen => imagen.EsPrincipal)
                        .Select(imagen => imagen.RutaArchivo)
                        .FirstOrDefault()
                },
                EmpresaDescripcion = p.Empresa.Descripcion ?? string.Empty,
                EmpresaDireccion = p.Empresa.Direccion,
                EmpresaTelefono = p.Empresa.Telefono,
                EmpresaMediosDePago = p.Empresa.MediosDePago,
                EmpresaOfreceDelivery = p.Empresa.OfreceDelivery,
                EmpresaCantidadProductos = 0
            })
            .FirstOrDefaultAsync();

        if (producto is null)
        {
            return NotFound();
        }

        producto.EmpresaCantidadProductos = await db.Productos
            .CountAsync(otro => otro.EmpresaId == producto.Producto.EmpresaId && otro.Activo);

        return View(producto);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? id)
    {
        var codigo = id is >= 400 and <= 599 ? id.Value : 500;

        if (codigo == 500)
        {
            var excepcion = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (excepcion?.Error is not null)
            {
                registro.LogError(excepcion.Error, "Error no controlado en {Ruta}.", excepcion.Path);
            }
        }

        Response.StatusCode = codigo;

        var modelo = codigo switch
        {
            400 => new ModeloError
            {
                CodigoEstado = codigo,
                Titulo = "Solicitud inválida",
                Mensaje = "La solicitud no es válida o venció el formulario. Volvé a intentarlo."
            },
            403 => new ModeloError
            {
                CodigoEstado = codigo,
                Titulo = "Acceso denegado",
                Mensaje = "No tenés permisos para ver esta sección."
            },
            404 => new ModeloError
            {
                CodigoEstado = codigo,
                Titulo = "Página no encontrada",
                Mensaje = "El recurso que buscás no existe o ya no está disponible."
            },
            405 => new ModeloError
            {
                CodigoEstado = codigo,
                Titulo = "Acción no permitida",
                Mensaje = "No se puede realizar esa acción sobre este recurso."
            },
            _ => new ModeloError
            {
                CodigoEstado = codigo,
                Titulo = "Ocurrió un error",
                Mensaje = "No pudimos completar la solicitud. Si el problema continúa, avisá al administrador."
            }
        };

        modelo.IdentificadorSolicitud = HttpContext.TraceIdentifier;
        return View("Error", modelo);
    }
}
