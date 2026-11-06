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
    public string? MediosDePago { get; set; }
    public bool OfreceDelivery { get; set; }
    public int CantidadProductos { get; set; }
    public int CantidadValoraciones { get; set; }
    public double PromedioValoracion {get; set;}
    
    public bool EstaValidada { get; set; }
}

