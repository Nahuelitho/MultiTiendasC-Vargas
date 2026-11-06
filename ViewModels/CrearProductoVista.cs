using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.ViewModels;

public class CrearProductoVista
{
    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Descripcion { get; set; }
    [Required]
    [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Precio { get; set; }
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;
}
