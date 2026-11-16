namespace MultiTenantCSharpVargas.ViewModels;

public class DetalleEmpresaVista
{
    public TarjetaEmpresaVista Empresa { get; set; } = new();
    public List<TarjetaProductoVista> Productos { get; set; } = new();

    // Valoración del usuario logueado (null si todavía no valoró).
    public int? MiValoracion { get; set; }
    public string? MiComentario { get; set; }

    // Últimas opiniones de otros clientes.
    public List<ComentarioValoracionVista> Comentarios { get; set; } = new();
}

public class ComentarioValoracionVista
{
    public string Usuario { get; set; } = string.Empty;
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime Fecha { get; set; }
}
