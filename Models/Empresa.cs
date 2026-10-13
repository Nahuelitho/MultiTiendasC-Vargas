using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

public class Empresa
{
    public int Id { get; set; }
    public int PropietarioId { get; set; }

    [Required, StringLength(120)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required, StringLength(150)]
    public string Direccion { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Localidad { get; set; } = string.Empty;

    [Precision(9, 6)]
    public decimal? Latitud { get; set; }

    [Precision(9, 6)]
    public decimal? Longitud { get; set; }

    [StringLength(100)]
    public string? Telefono { get; set; }

    [StringLength(250)]
    public string? MediosDePago { get; set; }

    public bool OfreceDelivery { get; set; }
    public EstadoEmpresa Estado { get; set; } = EstadoEmpresa.Pendiente;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public Usuario Propietario { get; set; } = null!;
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
    public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
}
