using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Models;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Data;

/// <summary>
/// Datos de demostración para poder probar el catálogo, el paginado y el ABM.
/// Es idempotente: si la empresa ya existe (o ya tiene productos) no vuelve a insertar.
/// </summary>
public static class SeedDatos
{
    public const string ClaveComerciantes = "Comerciante123.";

    private sealed record ProductoSeed(string Nombre, string Descripcion, decimal Precio, int Stock);

    private sealed record EmpresaSeed(
        string Nombre,
        string CorreoPropietario,
        string Descripcion,
        string Direccion,
        string Localidad,
        string Telefono,
        string MediosDePago,
        bool OfreceDelivery,
        int Puntuacion,
        ProductoSeed[] Productos);

    private static readonly EmpresaSeed[] Empresas =
    [
        new(
            "Mercado Central",
            "Comerciante1@test.com",
            "Almacén de barrio con productos de almacén, bebidas y limpieza.",
            "Av. Illia 1250",
            "San Luis",
            "2664-400101",
            "Efectivo, Mercado Pago, Tarjeta débito",
            true,
            5,
            [
                new("Yerba Mate Taragüí 1 kg", "Yerba mate despalillada suave.", 4500m, 25),
                new("Café Molido La Virginia 500 g", "Café tostado molido clásico.", 6200m, 18),
                new("Azúcar Ledesma 1 kg", "Azúcar refinada en paquete de 1 kg.", 1800m, 40),
                new("Aceite de Girasol Natura 1.5 L", "Aceite de girasol para cocina diaria.", 3200m, 22),
                new("Leche Entera La Serenísima 1 L", "Leche entera larga vida.", 1500m, 35),
                new("Fideos Don Vicente 500 g", "Fideos tallarín n° 5.", 1200m, 50),
                new("Arroz Gallo Oro 1 kg", "Arroz largo fino parboilizado.", 2100m, 30),
                new("Harina Cañuelas 000 1 kg", "Harina de trigo para panificados.", 1400m, 28),
                new("Galletitas Oreo 118 g", "Galletitas de chocolate rellenas.", 1100m, 45),
                new("Gaseosa Coca-Cola 2.25 L", "Bebida gaseosa sabor cola.", 3500m, 20),
                new("Detergente Ala 750 ml", "Detergente concentrado limón.", 2800m, 16),
                new("Papel Higiénico Elite 4 rollos", "Papel higiénico doble hoja.", 3600m, 24)
            ]),

        new(
            "Panadería La Espiga",
            "Comerciante2@test.com",
            "Panadería artesanal con pan casero, facturas y chipá recién horneados.",
            "San Martín 845",
            "Juana Koslay",
            "2664-400102",
            "Efectivo, Mercado Pago",
            true,
            5,
            [
                new("Pan Casero 1 kg", "Pan de campo recién horneado.", 2500m, 30),
                new("Medialunas (docena)", "Medialunas de manteca.", 4800m, 20),
                new("Facturas Surtidas (docena)", "Surtido de facturas de la casa.", 5200m, 18),
                new("Bizcochuelo Casero", "Bizcochuelo esponjoso para tortas.", 3500m, 10),
                new("Pan Integral 800 g", "Pan integral con semillas.", 3000m, 15),
                new("Chipá (docena)", "Chipá de queso al horno.", 4200m, 12)
            ]),

        new(
            "Farmacia San Luis",
            "Comerciante3@test.com",
            "Farmacia con medicamentos de venta libre, dermocosmética y primeros auxilios.",
            "Colón 560",
            "San Luis",
            "2664-400103",
            "Efectivo, Tarjeta débito, Tarjeta crédito",
            false,
            4,
            [
                new("Paracetamol 500 mg x 20", "Analgésico y antifebril.", 2500m, 40),
                new("Ibuprofeno 400 mg x 20", "Antiinflamatorio de venta libre.", 3200m, 35),
                new("Alcohol en Gel 250 ml", "Alcohol en gel con glicerina.", 1900m, 50),
                new("Protector Solar FPS 30", "Protección solar facial y corporal.", 8500m, 14),
                new("Vitamina C Efervescente", "Comprimidos efervescentes sabor naranja.", 2800m, 26),
                new("Curitas x 20", "Apósitos adhesivos de distintos tamaños.", 1200m, 60)
            ]),

        new(
            "Tienda del Cerro",
            "Comerciante4@test.com",
            "Regalos, decoración y artículos regionales para tu hogar.",
            "Ruta 20 km 4",
            "Potrero de los Funes",
            "2664-400104",
            "Efectivo, Mercado Pago, Transferencia",
            true,
            4,
            [
                new("Mate de Algarrobo", "Mate tallado en madera de algarrobo.", 9500m, 8),
                new("Set de Tazas Cerámicas", "Juego de 4 tazas artesanales.", 12000m, 6),
                new("Lámpara de Escritorio", "Lámpara LED con brazo articulado.", 18500m, 5),
                new("Cuadro Decorativo", "Cuadro con paisaje serrano en lienzo.", 7800m, 9),
                new("Billetera de Cuero", "Billetera artesanal de cuero vacuno.", 14200m, 7)
            ]),

        new(
            "Café de la Plaza",
            "Comerciante5@test.com",
            "Cafetería de especialidad con opciones dulces y saladas para llevar.",
            "Pringles 320",
            "La Punta",
            "2664-400105",
            "Efectivo, Mercado Pago, Tarjeta débito",
            true,
            5,
            [
                new("Café Americano", "Café de especialidad recién preparado.", 2200m, 30),
                new("Cappuccino", "Espresso con leche vaporizada y espuma.", 2800m, 25),
                new("Té Verde", "Té verde en hebras.", 1900m, 20),
                new("Cheesecake porción", "Cheesecake de frutos rojos.", 3200m, 12),
                new("Tostado de jamón y queso", "Pan de miga tostado.", 3500m, 15),
                new("Jugo de naranja", "Jugo natural exprimido al momento.", 2500m, 18)
            ]),

        new(
            "Verdulería El Huerto",
            "Comerciante6@test.com",
            "Frutas y verduras frescas de productores de la zona.",
            "Belgrano 1180",
            "Villa Mercedes",
            "2664-400106",
            "Efectivo, Transferencia",
            true,
            4,
            [
                new("Tomate Perita 1 kg", "Tomate perita fresco.", 1800m, 40),
                new("Papa Negra 1 kg", "Papa negra de calidad.", 1200m, 60),
                new("Manzana Red Delicious 1 kg", "Manzana dulce y crocante.", 2200m, 35),
                new("Zanahoria 1 kg", "Zanahoria fresca de huerta.", 1400m, 45),
                new("Lechuga Mantecosa", "Lechuga mantecosa de hoja tierna.", 900m, 25),
                new("Naranja 1 kg", "Naranja para jugo.", 1600m, 50)
            ]),

        new(
            "Librería El Saber",
            "Comerciante7@test.com",
            "Útiles escolares, librería y artículos de oficina.",
            "Ayacucho 670",
            "San Luis",
            "2664-400107",
            "Efectivo, Mercado Pago, Tarjeta débito",
            false,
            4,
            [
                new("Cuaderno Rivadavia 84 hojas", "Cuaderno universitario rayado.", 2400m, 50),
                new("Set de Lapiceras", "Set de 3 lapiceras de colores.", 3600m, 30),
                new("Mochila Escolar", "Mochila resistente con compartimentos.", 18500m, 10),
                new("Resma A4 500 hojas", "Papel blanco A4 75 g.", 6500m, 20),
                new("Calculadora Científica", "Calculadora con funciones científicas.", 9800m, 12),
                new("Agenda 2027", "Agenda anual tapa dura.", 7200m, 14)
            ]),

        new(
            "Boutique Aurora",
            "Comerciante8@test.com",
            "Indumentaria y accesorios para todos los estilos.",
            "Rivadavia 890",
            "Merlo",
            "2664-400108",
            "Efectivo, Mercado Pago, Tarjeta crédito",
            true,
            5,
            [
                new("Remera Básica de Algodón", "Remera unisex de algodón peinado.", 8900m, 24),
                new("Jean Mom Fit", "Jean de tiro alto y calce relaxed.", 24500m, 12),
                new("Vestido Floreado", "Vestido liviano con estampado floral.", 28900m, 8),
                new("Buzo Canguro", "Buzo frizado con capucha.", 19900m, 15),
                new("Zapatillas Urbanas", "Zapatillas livianas para uso diario.", 32900m, 9),
                new("Cartera Pequeña", "Cartera de mano con correa.", 15900m, 11)
            ])
    ];

    public static async Task SembrarAsync(ContextoAplicacion db, ILogger logger)
    {
        var rolComerciante = await db.Roles.FirstOrDefaultAsync(rol => rol.Codigo == "COMERCIANTE");
        if (rolComerciante is null)
        {
            logger.LogWarning("Seed demo omitido: no existe el rol COMERCIANTE.");
            return;
        }

        var cliente = await db.Usuarios.FirstOrDefaultAsync(usuario => usuario.Correo == "Usuario1@test.com");
        var hashComerciante = PasswordHasher.Hash(ClaveComerciantes);

        var empresasCreadas = 0;
        var productosCreados = 0;

        foreach (var semilla in Empresas)
        {
            var usuario = await db.Usuarios.FirstOrDefaultAsync(usuario => usuario.Correo == semilla.CorreoPropietario);

            // Si ya existe una empresa de ese usuario, no duplicamos: solo sembramos
            // productos cuando esa empresa todavía está vacía.
            Empresa? empresa = null;
            if (usuario is not null)
            {
                empresa = await db.Empresas
                    .OrderBy(e => e.Id)
                    .FirstOrDefaultAsync(e => e.PropietarioId == usuario.Id);
            }

            empresa ??= await db.Empresas.FirstOrDefaultAsync(e => e.Nombre == semilla.Nombre);

            if (usuario is null)
            {
                usuario = new Usuario
                {
                    Nombre = $"Comerciante {semilla.Nombre}",
                    Correo = semilla.CorreoPropietario,
                    ContrasenaHash = hashComerciante,
                    FechaAlta = DateTime.UtcNow
                };
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
            }

            if (empresa is null)
            {
                empresa = new Empresa
                {
                    PropietarioId = usuario.Id,
                    Nombre = semilla.Nombre,
                    Descripcion = semilla.Descripcion,
                    Direccion = semilla.Direccion,
                    Localidad = semilla.Localidad,
                    Telefono = semilla.Telefono,
                    MediosDePago = semilla.MediosDePago,
                    OfreceDelivery = semilla.OfreceDelivery,
                    Estado = EstadoEmpresa.Activa,
                    FechaAlta = DateTime.UtcNow
                };
                db.Empresas.Add(empresa);
                await db.SaveChangesAsync();
                empresasCreadas++;
            }

            var usuarioRol = await db.UsuarioRoles
                .FirstOrDefaultAsync(ur => ur.UsuarioId == usuario.Id && ur.RolId == rolComerciante.Id);

            if (usuarioRol is null)
            {
                db.UsuarioRoles.Add(new UsuarioRol
                {
                    UsuarioId = usuario.Id,
                    RolId = rolComerciante.Id,
                    EmpresaId = empresa.Id
                });
            }
            else if (usuarioRol.EmpresaId is null)
            {
                usuarioRol.EmpresaId = empresa.Id;
            }

            var nombresExistentes = await db.Productos
                .Where(producto => producto.EmpresaId == empresa.Id)
                .Select(producto => producto.Nombre)
                .ToListAsync();

            foreach (var producto in semilla.Productos)
            {
                // Idempotente: si el producto ya está cargado (mismo nombre) no se duplica.
                if (nombresExistentes.Contains(producto.Nombre, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                db.Productos.Add(new Producto
                {
                    EmpresaId = empresa.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Stock = producto.Stock,
                    Activo = true
                });
                productosCreados++;
            }

            if (cliente is not null)
            {
                var yaValoro = await db.Valoraciones
                    .AnyAsync(valoracion => valoracion.UsuarioId == cliente.Id && valoracion.EmpresaId == empresa.Id);

                if (!yaValoro)
                {
                    db.Valoraciones.Add(new Valoracion
                    {
                        UsuarioId = cliente.Id,
                        EmpresaId = empresa.Id,
                        Puntuacion = semilla.Puntuacion,
                        Comentario = "Excelente atención y buenos productos.",
                        FechaCreacion = DateTime.UtcNow
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        if (empresasCreadas > 0 || productosCreados > 0)
        {
            logger.LogInformation(
                "Seed demo aplicado: {Empresas} empresa(s) y {Productos} producto(s) nuevos.",
                empresasCreadas,
                productosCreados);
        }
    }
}
