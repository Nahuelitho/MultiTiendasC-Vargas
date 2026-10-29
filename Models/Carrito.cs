using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class Carrito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaUltimaActualizacion { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public ICollection<CarritoItem> Items { get; set; } = new List<CarritoItem>();
}
