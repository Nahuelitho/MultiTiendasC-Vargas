namespace MultiTenantCSharpVargas.Models;

public class ModeloError
{
    public string? IdentificadorSolicitud { get; set; }

    public bool MostrarIdentificador => !string.IsNullOrEmpty(IdentificadorSolicitud);
}
