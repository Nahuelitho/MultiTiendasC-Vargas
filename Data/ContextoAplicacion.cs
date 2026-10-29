using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Data;

public class ContextoAplicacion(DbContextOptions<ContextoAplicacion> opciones) : DbContext(opciones)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<ImagenProducto> ImagenesProducto => Set<ImagenProducto>();
    public DbSet<Valoracion> Valoraciones => Set<Valoracion>();
    public DbSet<Denuncia> Denuncias => Set<Denuncia>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItems => Set<PedidoItem>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoItem> CarritoItems => Set<CarritoItem>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

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

        modelo.Entity<ImagenProducto>()
            .HasOne(imagen => imagen.Producto)
            .WithMany(producto => producto.Imagenes)
            .HasForeignKey(imagen => imagen.ProductoId)
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

        // --- Roles y usuarios ---
        modelo.Entity<UsuarioRol>()
            .HasIndex(ur => new { ur.UsuarioId, ur.RolId, ur.EmpresaId }).IsUnique();

        modelo.Entity<UsuarioRol>()
            .HasOne(ur => ur.Usuario).WithMany(u => u.UsuarioRoles).HasForeignKey(ur => ur.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<UsuarioRol>()
            .HasOne(ur => ur.Rol).WithMany(r => r.UsuarioRoles).HasForeignKey(ur => ur.RolId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<UsuarioRol>()
            .HasOne(ur => ur.Empresa).WithMany().HasForeignKey(ur => ur.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        // --- Pedidos ---
        modelo.Entity<Pedido>()
            .HasCheckConstraint("CK_Pedidos_Estado", "Estado IN ('Pendiente','Confirmado','EnPreparacion','Listo','Entregado','Cancelado')");

        modelo.Entity<Pedido>()
            .HasOne(p => p.Usuario).WithMany(u => u.Pedidos).HasForeignKey(p => p.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Pedido>()
            .HasOne(p => p.Empresa).WithMany().HasForeignKey(p => p.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Pedido>()
            .HasOne(p => p.GrupoPedido).WithMany(p => p.PedidosGrupo).HasForeignKey(p => p.GrupoPedidoId).OnDelete(DeleteBehavior.Restrict);

        // PedidoItems: clave alternativa (Id, EmpresaId) para multi-tenant estricto
        modelo.Entity<Pedido>()
            .HasAlternateKey(p => new { p.Id, p.EmpresaId });

        modelo.Entity<PedidoItem>()
            .HasOne(i => i.Pedido).WithMany(p => p.Items).HasForeignKey(i => new { i.PedidoId, i.EmpresaId })
                .HasPrincipalKey(p => new { p.Id, p.EmpresaId }).OnDelete(DeleteBehavior.Cascade);

        modelo.Entity<PedidoItem>()
            .HasOne(i => i.Producto).WithMany().HasForeignKey(i => new { i.ProductoId, i.EmpresaId })
                .HasPrincipalKey(p => new { p.Id, p.EmpresaId }).OnDelete(DeleteBehavior.Restrict);

        // --- Carrito ---
        modelo.Entity<Carrito>()
            .HasOne(c => c.Usuario).WithMany(u => u.Carritos).HasForeignKey(c => c.UsuarioId).OnDelete(DeleteBehavior.Cascade);

        modelo.Entity<CarritoItem>()
            .HasOne(ci => ci.Carrito).WithMany(c => c.Items).HasForeignKey(ci => ci.CarritoId).OnDelete(DeleteBehavior.Cascade);

        modelo.Entity<CarritoItem>()
            .HasOne(ci => ci.Producto).WithMany().HasForeignKey(ci => ci.ProductoId).OnDelete(DeleteBehavior.Restrict);

        // --- Notificaciones ---
        modelo.Entity<Notificacion>()
            .HasOne(n => n.Usuario).WithMany(u => u.Notificaciones).HasForeignKey(n => n.UsuarioId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Notificacion>()
            .HasOne(n => n.Empresa).WithMany().HasForeignKey(n => n.EmpresaId).OnDelete(DeleteBehavior.Restrict);

        modelo.Entity<Notificacion>()
            .HasOne(n => n.Pedido).WithMany().HasForeignKey(n => n.PedidoId).OnDelete(DeleteBehavior.Restrict);

        // --- Seed de roles por defecto ---
        modelo.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Super Administrador", Codigo = "SUPER_ADMIN", Activo = true },
            new Rol { Id = 2, Nombre = "Administrador", Codigo = "ADMIN", Activo = true },
            new Rol { Id = 3, Nombre = "Comerciante", Codigo = "COMERCIANTE", Activo = true },
            new Rol { Id = 4, Nombre = "Cliente", Codigo = "CLIENTE", Activo = true }
        );
    }
}
