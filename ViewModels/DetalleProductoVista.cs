namespace MultiTenantCSharpVargas.ViewModels;

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