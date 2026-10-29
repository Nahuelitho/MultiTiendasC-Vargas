using System.ComponentModel.DataAnnotations;

namespace MultiTenantCSharpVargas.Models;

public class UsuarioRol
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public int? EmpresaId { get; set; } // NULL = rol global (SuperAdministrador)

    public Usuario Usuario { get; set; } = null!;
    public Rol Rol { get; set; } = null!;
    public Empresa? Empresa { get; set; }
}
