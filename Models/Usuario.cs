using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string ContrasenaHash { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AvatarUrl { get; set; }

    public RolUsuario Rol { get; set; } = RolUsuario.Cliente;
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
    public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
    public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
}
