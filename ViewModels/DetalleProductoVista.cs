namespace MultiTenantCSharpVargas.ViewModels;

public class DetalleProductoVista
{
    public TarjetaProductoVista Producto { get; set; } = new();
    public string EmpresaDescripcion { get; set; } = string.Empty;
    public string EmpresaDireccion { get; set; } = string.Empty;
    public string? EmpresaTelefono { get; set; }
    public string? EmpresaWhatsApp { get; set; }
    public string? EmpresaNumeroWhatsApp => string.IsNullOrWhiteSpace(EmpresaWhatsApp)
        ? null
        : new string(EmpresaWhatsApp.Where(char.IsDigit).ToArray());
    public bool EmpresaTieneWhatsApp => EmpresaNumeroWhatsApp is { Length: >= 8 };
    public string EmpresaEnlaceWhatsApp => $"https://wa.me/{EmpresaNumeroWhatsApp}";
    public string? EmpresaMediosDePago { get; set; }
    public bool EmpresaOfreceDelivery { get; set; }
    public int EmpresaCantidadProductos { get; set; }
}