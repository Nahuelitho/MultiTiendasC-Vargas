namespace MultiTenantCSharpVargas.ViewModels;

public class TarjetaEmpresaVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Inicial => string.IsNullOrEmpty(Nombre) ? "?" : Nombre.Substring(0,1).ToUpper();
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? NumeroWhatsApp => string.IsNullOrWhiteSpace(WhatsApp)
        ? null
        : new string(WhatsApp.Where(char.IsDigit).ToArray());
    public bool TieneWhatsApp => NumeroWhatsApp is { Length: >= 8 };
    public string EnlaceWhatsApp => $"https://wa.me/{NumeroWhatsApp}";
    public string? MediosDePago { get; set; }
    public bool OfreceDelivery { get; set; }
    public int CantidadProductos { get; set; }
    public int CantidadValoraciones { get; set; }
    public double PromedioValoracion {get; set;}
    
    public bool EstaValidada { get; set; }
}

