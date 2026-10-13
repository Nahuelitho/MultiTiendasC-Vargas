namespace MultiTenantCSharpVargas.Models;

public enum RolUsuario
{
    Cliente,
    Comerciante,
    Administrador
}

public enum EstadoEmpresa
{
    Pendiente,
    Activa,
    EnRevision,
    Suspendida
}

public enum EstadoDenuncia
{
    Pendiente,
    EnRevision,
    Resuelta,
    Rechazada
}

public enum MotivoDenuncia
{
    InformacionIncorrecta,
    LocalInexistente,
    ContenidoInadecuado,
    Otro
}
