using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

public class CarritoItem
{
    public int Id { get; set; }
    public int CarritoId { get; set; }
    public int EmpresaId { get; set; }
    public int ProductoId { get; set; }

    [Required, StringLength(160)]
    public string Nombre { get; set; } = string.Empty;

    [Precision(12, 2)]
    public decimal PrecioUnitario { get; set; }

    public int Cantidad { get; set; }

    [Precision(12, 2)]
    public decimal Subtotal { get; set; }

    public Carrito Carrito { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
}
