# Tu Tienda Cerca

Aplicacion web MVC para conectar personas con comercios locales, consultar sus
productos y contar con valoraciones y denuncias para brindar mayor confianza.

**Integrante:** Vargas, Nahuel Dario - nahuel11906@gmail.com

## Tecnologias

- C# y .NET 8.
- ASP.NET Core MVC y Razor.
- HTML y CSS.
- Entity Framework Core con Pomelo para MySQL.

## Modelo de datos

```mermaid
erDiagram
    USUARIO ||--o{ EMPRESA : administra
    USUARIO ||--o{ VALORACION : realiza
    EMPRESA ||--o{ VALORACION : recibe
    EMPRESA ||--o{ PRODUCTO : publica
    USUARIO ||--o{ DENUNCIA : crea
    EMPRESA ||--o{ DENUNCIA : recibe
    PRODUCTO o|--o{ DENUNCIA : puede_recibir

    USUARIO {
        int Id PK
        string Nombre
        string Correo UK
        string ContrasenaHash
        enum Rol
        datetime FechaAlta
    }

    EMPRESA {
        int Id PK
        int PropietarioId FK
        string Nombre
        string Direccion
        string Localidad
        decimal Latitud
        decimal Longitud
        boolean OfreceDelivery
        enum Estado
    }

    PRODUCTO {
        int Id PK
        int EmpresaId FK
        string Nombre
        decimal Precio
        int Stock
        boolean Activo
    }

    VALORACION {
        int Id PK
        int UsuarioId FK
        int EmpresaId FK
        int Puntuacion
        string Comentario
        datetime FechaCreacion
    }

    DENUNCIA {
        int Id PK
        int UsuarioId FK
        int EmpresaId FK
        int ProductoId FK
        enum Motivo
        string Descripcion
        enum Estado
        datetime FechaCreacion
    }
```

Cada usuario puede realizar una unica valoracion por comercio. Una denuncia
siempre corresponde a un comercio y, opcionalmente, a uno de sus productos.

## Descripcion del proyecto

Tu Tienda Cerca busca facilitar la visibilidad de comercios locales y permitir
que las personas encuentren productos, negocios cercanos, formas de pago y
opciones de delivery desde un unico lugar.

Los comerciantes podran registrar y administrar su negocio y sus productos.
Los usuarios podran consultar la informacion disponible, valorar comercios y
reportar publicaciones, productos o locales con informacion incorrecta. Las
valoraciones y denuncias aportaran un mecanismo de confianza y moderacion para
la comunidad.
