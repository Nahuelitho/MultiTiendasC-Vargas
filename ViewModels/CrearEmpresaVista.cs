using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.ViewModels;

public class CrearEmpresaVista
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(120)]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Descripcion { get; set; }
    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(150)]
    public string Direccion { get; set; } = string.Empty;
    [Required(ErrorMessage = "La localidad es obligatoria")]
    [StringLength(100)]
    public string Localidad { get; set; } = string.Empty;
    [StringLength(100)]
    public string? Telefono { get; set; }
    [StringLength(250)]
    public string? MediosDePago { get; set; }
    public bool OfreceDelivery { get; set; } = false;
}
