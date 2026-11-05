namespace MultiTenantCSharpVargas.Models;

public class ModeloError
{
    public int CodigoEstado { get; set; } = 500;
    public string Titulo { get; set; } = "Ocurrió un error";
    public string Mensaje { get; set; } = "No pudimos completar la solicitud. Intentá nuevamente en unos instantes.";
    public string? IdentificadorSolicitud { get; set; }

    public bool MostrarIdentificador => !string.IsNullOrEmpty(IdentificadorSolicitud);
    public bool EsErrorDelCliente => CodigoEstado is >= 400 and < 500;
}
