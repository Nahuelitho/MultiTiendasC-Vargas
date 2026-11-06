namespace MultiTenantCSharpVargas.ViewModels;

public class ImagenProductoVista
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string RutaArchivo { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;
}
