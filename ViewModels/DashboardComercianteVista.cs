using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.ViewModels;

public class DashboardComercianteVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Localidad { get; set; } = string.Empty;
    public EstadoEmpresa Estado { get; set; }
    public List<Producto> Productos { get; set; } = new();
}
