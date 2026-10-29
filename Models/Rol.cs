using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class Rol
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string Nombre { get; set; } = string.Empty; // SuperAdministrador, Administrador, Comerciante, Cliente

    [Required, StringLength(40)]
    public string Codigo { get; set; } = string.Empty; // SUPER_ADMIN, ADMIN, COMERCIANTE, CLIENTE

    public bool Activo { get; set; } = true;

    public ICollection<UsuarioRol> UsuarioRoles { get; set; } = new List<UsuarioRol>();
}
