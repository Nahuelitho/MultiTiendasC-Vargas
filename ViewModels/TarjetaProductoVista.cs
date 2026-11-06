namespace MultiTenantCSharpVargas.ViewModels;

public class TarjetaProductoVista
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; }
    public string EmpresaNombre { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public string? RutaImagen { get; set; }

    // Propiedades que te faltaban y que usan las vistas
    public string Inicial => string.IsNullOrWhiteSpace(Nombre) ? "?" : Nombre.Trim()[0].ToString().ToUpper();
    public bool TieneStock => Stock > 0;
}
