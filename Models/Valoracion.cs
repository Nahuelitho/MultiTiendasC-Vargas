using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

[Index(nameof(UsuarioId), nameof(EmpresaId), IsUnique = true)]
public class Valoracion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }

    [Range(1, 5)]
    public int Puntuacion { get; set; }

    [StringLength(500)]
    public string? Comentario { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Empresa Empresa { get; set; } = null!;
}
