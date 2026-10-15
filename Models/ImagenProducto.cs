using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class ImagenProducto
{
    public int Id { get; set; }
    public int ProductoId { get; set; }

    [Required, StringLength(250)]
    public string RutaArchivo { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    public Producto Producto { get; set; } = null!;
}
