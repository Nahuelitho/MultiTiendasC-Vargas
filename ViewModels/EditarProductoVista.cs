using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.ViewModels;

public class EditarProductoVista : CrearProductoVista
{
    [Required]
    public int Id { get; set; }
    public List<ImagenProductoVista> Imagenes { get; set; } = new();
}
