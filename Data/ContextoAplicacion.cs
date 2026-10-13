using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Data;

public class ContextoAplicacion(DbContextOptions<ContextoAplicacion> opciones) : DbContext(opciones)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    public DbSet<Denuncia> Denuncias => Set<Denuncia>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        base.OnModelCreating(modelo);

        modelo.Entity<Usuario>().HasIndex(usuario => usuario.Correo).IsUnique();

        modelo.Entity<Empresa>()
            .HasOne(empresa => empresa.Propietario)
            .WithMany(usuario => usuario.Empresas)
            .HasForeignKey(empresa => empresa.PropietarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Producto>()
            .HasOne(producto => producto.Empresa)
            .WithMany(empresa => empresa.Productos)
            .HasForeignKey(producto => producto.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelo.Entity<Valoracion>()
            .ToTable(tabla => tabla.HasCheckConstraint(
                "CK_Valoraciones_Puntuacion", "Puntuacion BETWEEN 1 AND 5"));

        modelo.Entity<Denuncia>()
            .HasOne(denuncia => denuncia.Usuario)
            .WithMany(usuario => usuario.Denuncias)
            .HasForeignKey(denuncia => denuncia.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Denuncia>()
            .HasOne(denuncia => denuncia.Empresa)
            .WithMany(empresa => empresa.Denuncias)
            .HasForeignKey(denuncia => denuncia.EmpresaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Denuncia>()
            .HasOne(denuncia => denuncia.Producto)
            .WithMany(producto => producto.Denuncias)
            .HasForeignKey(denuncia => denuncia.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
