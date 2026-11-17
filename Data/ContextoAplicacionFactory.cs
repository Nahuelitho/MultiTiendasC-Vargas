using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultiTenantCSharpVargas.Data;

// Permite crear migraciones con las herramientas de EF Core sin necesidad
// de que MySQL esté encendido ni de abrir una conexión al iniciar.
public class ContextoAplicacionFactory : IDesignTimeDbContextFactory<ContextoAplicacion>
{
    public ContextoAplicacion CreateDbContext(string[] args)
    {
        var opciones = new DbContextOptionsBuilder<ContextoAplicacion>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=tu_tienda_cerca;User ID=root;Password=;",
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        return new ContextoAplicacion(opciones);
    }
}
