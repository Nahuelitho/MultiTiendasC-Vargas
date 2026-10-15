using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MultiTenantCSharpVargas.Models;

public class Producto
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }

    [Required, StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Precision(10, 2)]
    public decimal Precio { get; set; }

    public int Stock { get; set; }
    public bool Activo { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public ICollection<ImagenProducto> Imagenes { get; set; } = new List<ImagenProducto>();
    public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
}
