using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class Denuncia
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }
    public int? ProductoId { get; set; }

    public MotivoDenuncia Motivo { get; set; }

    [StringLength(1000)]
    public string? Descripcion { get; set; }

    public EstadoDenuncia Estado { get; set; } = EstadoDenuncia.Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Empresa Empresa { get; set; } = null!;
    public Producto? Producto { get; set; }
}
