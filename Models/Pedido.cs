using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

public class Pedido
{
    public int Id { get; set; }

    [Required, StringLength(20)]
    public string Codigo { get; set; } = string.Empty; // PED-0001

    public int UsuarioId { get; set; }
    public int EmpresaId { get; set; }
    public int? GrupoPedidoId { get; set; } // agrupa compras de varias tiendas en un mismo carrito

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required, StringLength(30)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmado, EnPreparacion, Listo, Entregado, Cancelado

    [Precision(12, 2)]
    public decimal Total { get; set; }

    [StringLength(160)]
    public string? DireccionEntrega { get; set; }

    [StringLength(50)]
    public string? TelefonoContacto { get; set; }

    [StringLength(50)]
    public string? FormaPago { get; set; }

    [StringLength(500)]
    public string? Notas { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Empresa Empresa { get; set; } = null!;
    public Pedido? GrupoPedido { get; set; }
    public ICollection<Pedido> PedidosGrupo { get; set; } = new List<Pedido>();
    public ICollection<PedidoItem> Items { get; set; } = new List<PedidoItem>();
}
