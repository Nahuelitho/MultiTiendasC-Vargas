# Tu Tienda Cerca - MultiTenant C# Vargas

Proyecto Integrador Final - Sitio Web MVC - ULP 2026 - Lab 2

---

## 0. Credenciales y configuración

### Usuarios de prueba (se crean por migración)

| Rol | Correo | Contraseña | Acceso |
|---|---|---|---|
| SUPER_ADMIN | SuperAdmin@test.com | Super123. | /SuperAdmin - Aprueba empresas, gestiona roles y usuarios |
| COMERCIANTE | Comerciante1@test.com | Comerciante123. | /Comerciante - Gestiona la empresa Mercado Central |
| COMERCIANTE | Comerciante2@test.com | Comerciante123. | /Comerciante - Panadería La Espiga |
| COMERCIANTE | Comerciante3@test.com | Comerciante123. | /Comerciante - Farmacia San Luis |
| COMERCIANTE | Comerciante4@test.com | Comerciante123. | /Comerciante - Tienda del Cerro |
| COMERCIANTE | Comerciante5@test.com | Comerciante123. | /Comerciante - Café de la Plaza |
| COMERCIANTE | Comerciante6@test.com | Comerciante123. | /Comerciante - Verdulería El Huerto |
| COMERCIANTE | Comerciante7@test.com | Comerciante123. | /Comerciante - Librería El Saber |
| COMERCIANTE | Comerciante8@test.com | Comerciante123. | /Comerciante - Boutique Aurora |
| CLIENTE | Usuario1@test.com | Usuario123. | Catálogo público |

El seed de demostración se ejecuta automáticamente al iniciar la aplicación (`Data/SeedDatos.cs`) y es idempotente: crea 8 empresas activas, más de 50 productos y usuarios comerciantes de prueba. Si una empresa ya existe y ya tiene productos, no se vuelve a sembrar.

> Si ya tenías una empresa creada con Comerciante1@test.com, el seed agrega los productos de demostración a esa empresa (solo si todavía no tiene productos).

### Configuración JWT y Base de datos (appsettings.json)

```json
{
  "Jwt": {
    "Key": "ClaveSuperSecretaDe32CaracteresMinimo123!ParaJWT2026",
    "Issuer": "TuTiendaCerca",
    "Audience": "TuTiendaCercaApp",
    "ExpireHours": "8"
  },
  "ConnectionStrings": {
    "BaseDeDatos": "Server=localhost;Port=3306;Database=tu_tienda_cerca;User ID=root;Password=;"
  }
}
```

Base de datos: MySQL 8 / MariaDB 10.4 (XAMPP). Se crea automática con `dotnet ef database update` o al iniciar la aplicación.

### Script SQL con datos de prueba

Dejé el dump completo en `docs/base_datos_demo.sql` (estructura + usuarios + 8 empresas + 53 productos + valoraciones). Para importarlo:

```powershell
# Crear la base y luego importar el script
C:\xampp\mysql\bin\mysql.exe -u root -e "CREATE DATABASE IF NOT EXISTS tu_tienda_cerca CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
Get-Content docs\base_datos_demo.sql -Raw | C:\xampp\mysql\bin\mysql.exe -u root tu_tienda_cerca
```

También podés dejar que la app cree la base y siembre los datos sola: `Data/SeedDatos.cs` se ejecuta automáticamente al iniciar y es idempotente.

---

## 1. Descripción

Hice Tu Tienda Cerca como un marketplace para comercios de San Luis. Cada comerciante puede crear su empresa, subir productos con fotos y gestionar su catálogo. El catálogo público muestra solo las empresas que están activas. También agregué valoraciones, denuncias y el sello de Validada por la comunidad.

Lo hice para que los comercios locales tengan un lugar centralizado donde mostrar sus productos.

**Autor:** Vargas, Nahuel Darío - Potrero de los Funes, San Luis - nahuel11906@gmail.com

---

## 2. Matriz de Evidencias - Requerimientos Mínimos

| Requerimiento | Estado | Dónde está el código | Evidencia |
|---|---|---|---|
| 1. 6 Tablas y relaciones 1:N y N:M | Cumplido | Models/Empresa.cs, Models/Producto.cs, Models/ImagenProducto.cs, Models/Usuario.cs, Models/UsuarioRol.cs, Models/Rol.cs - Empresa 1:N Producto, Producto 1:N ImagenProducto, Usuario N:M Rol | docs/DER.png |
| 2. Login, Roles, Avatar | Cumplido | Controllers/CuentaController.cs, Services/ServicioSesion.cs, Models/Usuario.cs (AvatarUrl), Views/Perfil/Index.cshtml, Program.cs autenticación por cookies | Captura de perfil con avatar |
| 3. Manejo de Archivos extra | Cumplido | Controllers/ComercianteController.cs (validación 6 imágenes, 2MB, JPG/PNG/WEBP/GIF) y Controllers/Api/ProductosApiController.cs endpoint con-imagen, wwwroot/imagenes/productos/ | GIF subiendo producto con imagen |
| 4. ABM Vue.js + AJAX (SPA) | Cumplido | Views/Comerciante/ProductosVue.cshtml (Vue 3 CDN), Controllers/Api/ProductosApiController.cs (Listar, Crear, Editar, Eliminar) | Video en /Comerciante/ProductosVue creando producto sin recargar |
| 5. Paginado por Servidor | Cumplido | Controllers/InicioController.cs (`TamanoPagina = 6`, CountAsync + Skip/Take), Controllers/ComercianteController.cs y Controllers/Api/ProductosApiController.cs (`page`/`pageSize` + CountAsync + Skip/Take), ViewModels/PaginacionVista.cs, Views/Shared/_Paginacion.cshtml | Captura Network con page=1&pageSize=10 y totalPages |
| 6. Búsqueda Asíncrona | Cumplido | Controllers/Api/ProductosApiController.cs BuscarAsync con Take(10) y ProductosVue.cshtml buscarAsyncProductos() | GIF buscando en input de ProductosVue |
| 7. Web API REST + JWT | Cumplido | Services/ServicioJwt.cs GenerarToken, Controllers/Api/AuthApiController.cs POST /api/auth/login, Program.cs AddJwtBearer, ProductosApiController con [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")] | docs/PostmanCollection.json con Bearer Token |

---

## 3. Dónde implementé cada parte

### Paginado por servidor
Lo hice en InicioController.cs (catálogo público, 6 registros por página), ComercianteController.cs (9 por página) y ProductosApiController.cs (10 por defecto). En vez de traer todos los productos, hago CountAsync() para saber el total y Skip/Take para traer solo la página que se pide.

```csharp
var total = await consulta.CountAsync();
var productos = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
```

El seed crea suficientes empresas y productos para que el paginado se vea funcionando: en el catálogo público `/` hay varias páginas tanto en la vista "Por empresa" como en "Por producto". Los enlaces usan `pagina` y no recargan todos los registros desde la base.

### Búsqueda asíncrona
En ProductosApiController.cs tengo el endpoint buscar que solo trae 10 resultados si escribís 2 letras o más. No traigo toda la tabla.

```csharp
var res = await _ctx.Productos.Where(p => p.Nombre.Contains(q)).Take(10).ToListAsync();
```

### ABM Vue.js SPA
Lo hice en Views/Comerciante/ProductosVue.cshtml con Vue 3 por CDN. Todo lo hago con fetch a /api/productos: listar, crear, editar y eliminar sin hacer reload. La ruta es /Comerciante/ProductosVue y redirigí el Index del comerciante para que vaya directo ahí.

### Web API + JWT
Usé ServicioJwt.cs para generar el token con los roles del usuario. En AuthApiController hice el login que devuelve el token y en Program.cs configuré el doble esquema: cookies para el MVC y Bearer para la API. Los endpoints de crear, editar y eliminar productos están protegidos con [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")].

### Manejo de archivos
Para el avatar del perfil y para los productos. En el SPA agregué un endpoint POST /api/productos/con-imagen que recibe FormData con la imagen y la guarda en wwwroot/imagenes/productos/. Valido extensión y tamaño máximo 2MB.

---

## 4. Estructura del repositorio

```
MultiTenantCSharpVargas/
├── Controllers/
│   ├── Api/ - ProductosApiController.cs, AuthApiController.cs
│   ├── InicioController.cs - Catálogo público con paginado
│   ├── ComercianteController.cs - Panel, redirige a ProductosVue
│   └── CuentaController.cs, SuperAdminController.cs
├── Data/
│   ├── ContextoAplicacion.cs - DbContext, relaciones y seed de roles/usuarios
│   └── SeedDatos.cs - Seed idempotente de empresas y productos de demo
├── Models/ - Empresa, Producto, ImagenProducto, Usuario, Rol, etc.
├── ViewModels/ - TarjetaProductoVista, PaginacionVista, etc.
├── Services/ - ServicioSesion, ServicioJwt
├── Views/
│   ├── Comerciante/ProductosVue.cshtml - SPA Vue
│   ├── Shared/_Paginacion.cshtml
├── wwwroot/css/componentes/ - catalogo.css, productos-vue.css, tarjetas.css
├── docs/
│   ├── DER.png - Diagrama DER exportado de Workbench
│   ├── base_datos_demo.sql - Dump con estructura y datos de prueba
│   └── PostmanCollection.json - Colección para probar la API
├── README.md
└── Program.cs - Auth cookies + JWT + migraciones/seed automáticos
```

---

## 5. Tecnologías

C# 12, .NET 8, ASP.NET Core MVC, Razor, Entity Framework Core 9 con Pomelo MySQL, Autenticación por cookies + JWT, Vue 3 (CDN), HTML, CSS, JavaScript.

Roles: SUPER_ADMIN (todo), COMERCIANTE (/Comerciante/*), CLIENTE (catálogo).

---

## 6. Colección Postman

La dejé en docs/PostmanCollection.json

Para probar:
1. POST /api/auth/login con Comerciante1@test.com / Comerciante123. -> devuelve token
2. GET /api/productos?page=1&pageSize=10
3. GET /api/productos/buscar?q=Ag
4. POST /api/productos con Bearer token
5. POST /api/productos/con-imagen con FormData y foto

La app ya consume la API desde el Vue, pero dejé la colección para la defensa.

---

## 7. Cómo correr el proyecto

```powershell
dotnet restore
dotnet ef database update
dotnet run
# Abrir http://localhost:5122
# Login con Comerciante1@test.com / Comerciante123.
# Ir a /Comerciante/ProductosVue para ver el ABM Vue con paginado.
```

La aplicación aplica las migraciones y el seed de demostración automáticamente al iniciar. Para volver a sembrar los datos, alcanza con borrar las empresas de demo (o la base) y reiniciar.

Alternativa con el dump ya cargado:

```powershell
C:\xampp\mysql\bin\mysql.exe -u root -e "CREATE DATABASE IF NOT EXISTS tu_tienda_cerca CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
Get-Content docs\base_datos_demo.sql -Raw | C:\xampp\mysql\bin\mysql.exe -u root tu_tienda_cerca
```

---

## 8. DER

Diagrama exportado de MySQL Workbench 8.0 desde la base tu_tienda_cerca. Incluye las tablas principales.

Archivo: docs/DER.png

---

## 9. Qué falta pulir

El ABM Vue ya funciona creando productos con y sin foto. Queda grabar el video corto mostrando el flujo sin recargar para la defensa y terminar de pulir los estilos del catálogo.

