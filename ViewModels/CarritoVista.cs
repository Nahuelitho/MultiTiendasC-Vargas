namespace MultiTenantCSharpVargas.ViewModels;

public class CarritoVista
{
    public List<CarritoItemVista> Items { get; set; } = new();
    public decimal Total => Items.Sum(item => item.Subtotal);
    public int CantidadTotal => Items.Sum(item => item.Cantidad);
}

public class CarritoItemVista
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string EmpresaNombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    public int Stock { get; set; }
    public string? RutaImagen { get; set; }

    public string Inicial => string.IsNullOrWhiteSpace(Nombre) ? "?" : Nombre.Trim()[0].ToString().ToUpper();
}
