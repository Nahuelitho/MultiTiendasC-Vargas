namespace MultiTenantCSharpVargas.ViewModels;

public class PaginacionVista
{
    // Propiedades originales tuyas
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public string Controlador { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public Dictionary<string, string> Parametros { get; set; } = new();

    // Propiedades que espera _Paginacion.cshtml (alias en ingles)
    public int Page { get => Pagina; set => Pagina = value; }
    public int TotalPages { get => TotalPaginas; set => TotalPaginas = value; }
    
    public int PageSize { get; set; } = 10;
    public int TotalItems { get; set; } = 0;

    // Propiedades que espera Inicio.cshtml (compatibilidad con CatalogoVista)
    public string Vista { get; set; } = "empresas";
    public string? Busqueda { get; set; }
}
