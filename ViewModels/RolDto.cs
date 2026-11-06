namespace MultiTenantCSharpVargas.ViewModels
{
    public class RolDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}