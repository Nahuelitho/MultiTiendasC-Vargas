# Tu Tienda Cerca - MultiTenant C# Vargas

Proyecto Integrador Final - Sitio Web MVC - ULP 2026 - Lab 2

---

## 0. Credenciales y configuración

### Usuarios de prueba (se crean por migración)

| Rol | Correo | Contraseña | Acceso |
|---|---|---|---|
| SUPER_ADMIN | SuperAdmin@test.com | Super123. | /SuperAdmin - Aprueba empresas, gestiona roles y usuarios |
| COMERCIANTE | Comerciante1@test.com | Comerciante123. | /Comerciante - Crea empresa y gestiona productos |
| CLIENTE | Usuario1@test.com | Usuario123. | Catálogo público |

El comerciante arranca sin empresa. Primero tiene que ir a /Comerciante/CrearEmpresa y después el Super Admin la aprueba en /SuperAdmin/Empresas.

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

Base de datos: MySQL 8 / MariaDB 10.4 (XAMPP). Se crea automática con `dotnet ef database update`.

---

## 1. Descripción

Hice Tu Tienda Cerca como un marketplace para comercios de San Luis. Cada comerciante puede crear su empresa, subir productos con fotos y gestionar su catálogo. El catálogo público muestra solo las empresas que están activas. También agregué valoraciones, denuncias y el sello de Validada por la comunidad.

Lo hice para que los comercios locales tengan un lugar centralizado donde mostrar sus productos.

**Autor:** Vargas, Nahuel Darío - Potrero de los Funes, San Luis - nahuel11906@gmail.com

---

## 2. Matriz de Evidencias - Requerimientos Mínimos

| Requerimiento | Estado | Dónde está el código | Evidencia |
|---|---|---|---|
| 1. 6 Tablas y relaciones 1:N y N:M | Cumplido | Models/Empresa.cs, Models/Producto.cs, Models/ImagenProducto.cs, Models/Usuario.cs, Models/UsuarioRol.cs, Models/Rol.cs - Empresa 1:N Producto, Producto 1:N ImagenProducto, Usuario N:M Rol | docs/der.png |
| 2. Login, Roles, Avatar | Cumplido | Controllers/CuentaController.cs, Services/ServicioSesion.cs, Models/Usuario.cs (AvatarUrl), Views/Perfil/Index.cshtml, Program.cs autenticación por cookies | Captura de perfil con avatar |
| 3. Manejo de Archivos extra | Cumplido | Controllers/ComercianteController.cs (validación 6 imágenes, 2MB, JPG/PNG/WEBP/GIF) y Controllers/Api/ProductosApiController.cs endpoint con-imagen, wwwroot/imagenes/productos/ | GIF subiendo producto con imagen |
| 4. ABM Vue.js + AJAX (SPA) | Cumplido | Views/Comerciante/ProductosVue.cshtml (Vue 3 CDN), Controllers/Api/ProductosApiController.cs (Listar, Crear, Editar, Eliminar) | Video en /Comerciante/ProductosVue creando producto sin recargar |
| 5. Paginado por Servidor | Cumplido | Controllers/InicioController.cs TamanoPagina = 9 con Skip/Take y Controllers/Api/ProductosApiController.cs page/pageSize con CountAsync + Skip/Take, ViewModels/PaginacionVista.cs y Views/Shared/_Paginacion.cshtml | Captura Network con page=1&pageSize=10 y totalPages |
| 6. Búsqueda Asíncrona | Cumplido | Controllers/Api/ProductosApiController.cs BuscarAsync con Take(10) y ProductosVue.cshtml buscarAsyncProductos() | GIF buscando en input de ProductosVue |
| 7. Web API REST + JWT | Cumplido | Services/ServicioJwt.cs GenerarToken, Controllers/Api/AuthApiController.cs POST /api/auth/login, Program.cs AddJwtBearer, ProductosApiController con [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")] | docs/TuTiendaCerca.postman_collection.json con Bearer Token |

---

## 3. Dónde implementé cada parte

### Paginado por servidor
Lo hice en InicioController.cs y en ProductosApiController.cs. En vez de traer todos los productos, hago CountAsync() para saber el total y Skip/Take para traer solo la página que se pide.

```csharp
var total = await consulta.CountAsync();
var productos = await consulta.Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
```

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
├── Models/ - Empresa, Producto, ImagenProducto, Usuario, Rol, etc.
├── ViewModels/ - TarjetaProductoVista, PaginacionVista, etc.
├── Services/ - ServicioSesion, ServicioJwt
├── Views/
│   ├── Comerciante/ProductosVue.cshtml - SPA Vue
│   ├── Shared/_Paginacion.cshtml
├── wwwroot/css/componentes/ - catalogo.css, productos-vue.css, tarjetas.css
├── docs/
│   ├── der.png - Diagrama DER exportado de Workbench
│   └── TuTiendaCerca.postman_collection.json - Colección para probar la API
├── README.md
└── Program.cs - Auth cookies + JWT
```

---

## 5. Tecnologías

C# 12, .NET 8, ASP.NET Core MVC, Razor, Entity Framework Core 9 con Pomelo MySQL, Autenticación por cookies + JWT, Vue 3 (CDN), HTML, CSS, JavaScript.

Roles: SUPER_ADMIN (todo), COMERCIANTE (/Comerciante/*), CLIENTE (catálogo).

---

## 6. Colección Postman

La dejé en docs/TuTiendaCerca.postman_collection.json

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
# Luego /Comerciante/ProductosVue con usuario Comerciante1@test.com
```

---

## 8. DER

Diagrama exportado de MySQL Workbench 8.0 desde la base tu_tienda_cerca. Incluye 8 tablas principales.

Archivo: docs/der.png

---

## 9. Qué falta pulir

El ABM Vue ya funciona creando productos con y sin foto. Queda grabar un video corto mostrando el flujo sin recargar para la defensa y terminar de pulir los estilos del catálogo.

