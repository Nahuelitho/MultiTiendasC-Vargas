using System.Collections;

namespace MultiTenantCSharpVargas.ViewModels;

public class CatalogoVista : IEnumerable<TarjetaProductoVista>
{
    public string Vista { get; set; } = "empresas";
    public string? Busqueda { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; } = 1;
    public string? MensajeError { get; set; }
    public List<TarjetaEmpresaVista> Empresas { get; set; } = new();
    public List<TarjetaProductoVista> Productos { get; set; } = new();
    public bool ViendoEmpresas => !string.Equals(Vista, "productos", StringComparison.OrdinalIgnoreCase);

    // Para que Productos.cshtml pueda hacer Model.Any() y foreach(var p in Model)
    public IEnumerator<TarjetaProductoVista> GetEnumerator() => Productos.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
