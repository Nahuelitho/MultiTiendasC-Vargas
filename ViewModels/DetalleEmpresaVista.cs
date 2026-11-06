namespace MultiTenantCSharpVargas.ViewModels;

public class DetalleEmpresaVista
{
    public TarjetaEmpresaVista Empresa { get; set; } = new();
    public List<TarjetaProductoVista> Productos { get; set; } = new();
}