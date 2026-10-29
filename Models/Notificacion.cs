using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; } // destinatario
    public int EmpresaId { get; set; }
    public int? PedidoId { get; set; }

    [Required, StringLength(160)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    public bool Leida { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public Empresa Empresa { get; set; } = null!;
    public Pedido? Pedido { get; set; }
}
