using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

public class PedidoItem
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int EmpresaId { get; set; } // multi-tenant: a que empresa pertenece este item
    public int? ProductoId { get; set; } // nullable: historial aunque se borre el producto

    [Required, StringLength(160)]
    public string Nombre { get; set; } = string.Empty; // snapshot del nombre al momento de comprar

    [Precision(12, 2)]
    public decimal PrecioUnitario { get; set; }

    public int Cantidad { get; set; }

    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    public Pedido Pedido { get; set; } = null!;
    public Producto? Producto { get; set; }
}
