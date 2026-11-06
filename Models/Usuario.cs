using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

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

    [NotMapped]
    public IFormFile? AvatarFile { get; set; }

    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
    public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
    public ICollection<Valoracion> Valoraciones { get; set; } = new List<Valoracion>();
    public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<Carrito> Carritos { get; set; } = new List<Carrito>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

    // --- ALIAS COMPATIBLES PARA QUE NO SE ROMPA TU SERVICIO JWT ---
    [NotMapped]
    public string Email 
    { 
        get => Correo; 
        set => Correo = value; 
    }

    [NotMapped]
    public List<string> Roles { get; set; } = new();
}