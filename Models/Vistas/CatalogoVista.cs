namespace MultiTenantCSharpVargas.Models.Vistas;

public class CatalogoVista
{
    public string Vista { get; set; } = "empresas";
    public string? Busqueda { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public List<TarjetaEmpresaVista> Empresas { get; set; } = [];
    public List<TarjetaProductoVista> Productos { get; set; } = [];
    public string? MensajeError { get; set; }

    public bool ViendoEmpresas => Vista == "empresas";
}

public class TarjetaEmpresaVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? MediosDePago { get; set; }
    public bool OfreceDelivery { get; set; }
    public int CantidadProductos { get; set; }
    public int CantidadValoraciones { get; set; }
    public double PromedioValoracion { get; set; }

    public string Inicial => string.IsNullOrWhiteSpace(Nombre)
        ? "?"
        : Nombre.Trim()[..1].ToUpperInvariant();

    public bool EstaValidada => CantidadValoraciones >= 3 && PromedioValoracion >= 4;
}

public class TarjetaProductoVista
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;
    public string EmpresaNombre { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string? RutaImagen { get; set; }

    public string Inicial => string.IsNullOrWhiteSpace(Nombre)
        ? "?"
        : Nombre.Trim()[..1].ToUpperInvariant();

    public bool TieneStock => Stock > 0;
}

public class DetalleEmpresaVista
{
    public TarjetaEmpresaVista Empresa { get; set; } = new();
    public List<TarjetaProductoVista> Productos { get; set; } = [];
}

public class DetalleProductoVista
{
    public TarjetaProductoVista Producto { get; set; } = new();
    public string EmpresaDescripcion { get; set; } = string.Empty;
    public string EmpresaDireccion { get; set; } = string.Empty;
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaMediosDePago { get; set; }
    public bool EmpresaOfreceDelivery { get; set; }
    public int EmpresaCantidadProductos { get; set; }
}

public class PaginacionVista
{
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
    public string Vista { get; set; } = "empresas";
    public string? Busqueda { get; set; }
}
