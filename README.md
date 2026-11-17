# Tu Tienda Cerca - MultiTenant C# Vargas

Proyecto Integrador Final - Sitio Web MVC - ULP 2026 - Lab 2

---

## Cómo iniciar el proyecto

Esto es **un solo proyecto ASP.NET Core MVC** (no hay frontend separado). El Vue se carga por CDN dentro de las vistas, así que **no hace falta `npm install` ni `npm run dev`**. La API (`/api/...`) corre en el mismo proceso que la web: alcanza con `dotnet run`.

### Requisitos

- .NET 8 SDK
- MySQL 8 / MariaDB (por ejemplo XAMPP) iniciado
- Herramienta de EF Core (solo la primera vez):

```powershell
dotnet tool install --global dotnet-ef --version 9.*
```

### 1. Configurar la conexión

Editá `appsettings.Development.json` y poné los datos de tu MySQL:

```json
"ConnectionStrings": {
  "BaseDeDatos": "Server=localhost;Port=3306;Database=tu_tienda_cerca;User ID=root;Password=TU_PASSWORD;"
}
```

> `appsettings.Development.json` se usa cuando corrés con `dotnet run` (entorno Development). Si tu usuario root no tiene contraseña, dejá `Password=;`.

### 2. Crear/actualizar la base de datos

Desde la carpeta del proyecto (donde está `MultiTenantCSharpVargas.csproj`):

```powershell
dotnet restore
dotnet ef database update
```

`dotnet ef database update` aplica las migraciones y crea la base `tu_tienda_cerca` si no existe. La app también aplica las migraciones y el seed automáticamente al iniciar (`Program.cs`), así que este paso es opcional si preferís que lo haga el arranque.

### 3. Ejecutar

```powershell
dotnet run
```

Abrí **http://localhost:5122** (perfil `http` de `Properties/launchSettings.json`). El seed de demostración se ejecuta solo al iniciar.

- Web MVC: `http://localhost:5122`
- API REST: `http://localhost:5122/api/...`
- ABM Vue: `http://localhost:5122/Comerciante/ProductosVue` (con un usuario COMERCIANTE)

---

## 0. Credenciales y configuración

### Usuarios de prueba (seed automático)

La migración crea un usuario por rol y `Data/SeedDatos.cs` crea los comerciantes 2 a 8 al iniciar.

| Rol | Correo | Contraseña | Acceso |
|---|---|---|---|
| SUPER_ADMIN | SuperAdmin@test.com | Super123. | /SuperAdmin - Aprueba empresas, gestiona roles y usuarios |
| ADMIN | Admin1@test.com | Admin123. | Panel de administración general |
| COMERCIANTE | Comerciante1@test.com | Comerciante1. | /Comerciante - Gestiona la empresa Mercado Central |
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

Dejé el dump completo en `docs/base_datos_demo.sql`: estructura de las 13 tablas + migraciones + **11 usuarios, 8 empresas, 53 productos y 8 valoraciones**. No incluye carritos, pedidos ni notificaciones (arranca limpio para la demo). Para importarlo:

```powershell
# Crear la base y luego importar el script
C:\xampp\mysql\bin\mysql.exe -u root -e "CREATE DATABASE IF NOT EXISTS tu_tienda_cerca CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
cmd /c "C:\xampp\mysql\bin\mysql.exe --default-character-set=utf8mb4 -u root tu_tienda_cerca < docs\base_datos_demo.sql"
```

> Usá `cmd /c "... < archivo"` en vez de `Get-Content | mysql`: PowerShell puede cambiar la codificación y romper los acentos (Panadería, Café, etc.).

También podés dejar que la app cree la base y siembre los datos sola: `Data/SeedDatos.cs` se ejecuta automáticamente al iniciar y es idempotente.

---

## 1. Descripción

Hice Tu Tienda Cerca como un marketplace para comercios de San Luis. Cada comerciante puede crear su empresa, subir productos con fotos y gestionar su catálogo. El catálogo público muestra solo las empresas que están activas. También agregué valoraciones, denuncias y el sello de Validada por la comunidad.

Lo hice para que los comercios locales tengan un lugar centralizado donde mostrar sus productos.

Además, un usuario con rol CLIENTE puede armar un carrito básico: agregar productos desde la ficha de cada producto, ver el total, cambiar cantidades y eliminar ítems. El carrito se guarda en la base (`Carritos` + `CarritoItems`), valida stock y se muestra un contador en el header. El checkout todavía no está implementado.

**Autor:** Vargas, Nahuel Darío - Potrero de los Funes, San Luis - nahuel11906@gmail.com

---

## 2. Matriz de Evidencias - Requerimientos Mínimos

| Requerimiento | Estado | Dónde está el código | Evidencia |
|---|---|---|---|
| 1. 6 Tablas y relaciones 1:N y N:M | Cumplido | 13 tablas en total. Models/Empresa.cs, Models/Producto.cs, Models/ImagenProducto.cs, Models/Usuario.cs, Models/UsuarioRol.cs, Models/Rol.cs - Empresa 1:N Producto, Producto 1:N ImagenProducto, Usuario N:M Rol | docs/DER.png |
| 2. Login, Roles, Avatar | Cumplido | Controllers/CuentaController.cs, Services/ServicioSesion.cs, Models/Usuario.cs (AvatarUrl), Views/Perfil/Index.cshtml, Program.cs autenticación por cookies | Captura de perfil con avatar |
| 3. Manejo de Archivos extra | Cumplido | Controllers/ComercianteController.cs (validación 6 imágenes, 2MB, JPG/PNG/WEBP/GIF) y Controllers/api/ProductosApiController.cs endpoint con-imagen, wwwroot/imagenes/productos/ | GIF subiendo producto con imagen |
| 4. ABM Vue.js + AJAX (SPA) | Cumplido | Views/Comerciante/ProductosVue.cshtml (Vue 3 CDN), Controllers/api/ProductosApiController.cs (Listar, Crear, Editar, Eliminar) | Video en /Comerciante/ProductosVue creando producto sin recargar |
| 5. Paginado por Servidor | Cumplido | Controllers/InicioController.cs (`TamanoPagina = 6`, CountAsync + Skip/Take), Controllers/ComercianteController.cs y Controllers/api/ProductosApiController.cs (`page`/`pageSize` + CountAsync + Skip/Take), ViewModels/PaginacionVista.cs, Views/Shared/_Paginacion.cshtml | Captura Network con page=1&pageSize=10 y totalPages |
| 6. Búsqueda Asíncrona | Cumplido | Controllers/SuperAdminController.cs `BuscarEmpresas` (Contains + `Take(10)`), wwwroot/js/buscador-empresas.js (debounce 250 ms + fetch), usado en Views/SuperAdmin/CrearUsuario.cshtml y Views/SuperAdmin/UsuarioRoles.cshtml. Extra: Controllers/api/ProductosApiController.cs `BuscarAsync` | GIF escribiendo en el campo Empresa y viendo sugerencias AJAX |
| 7. Web API REST + JWT | Cumplido | Services/ServicioJwt.cs GenerarToken, Controllers/api/AuthApiController.cs POST /api/auth/login, Program.cs AddJwtBearer, ProductosApiController con [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")] | docs/PostmanCollection.json con Bearer Token |

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
El caso principal está en el panel de SuperAdmin: al crear un usuario o asignar un rol, la empresa no se elige de un `<select>` con toda la tabla. Se escribe en un input y `wwwroot/js/buscador-empresas.js` consulta por AJAX a `SuperAdmin/BuscarEmpresas`, que filtra con `Contains`, ordena y devuelve como máximo 10 resultados (`Take(10)`). El JS espera 250 ms desde la última tecla (debounce) y guarda el id elegido en un input oculto, así el formulario envía `empresaId`.

```csharp
var empresas = await db.Empresas
    .Where(e => e.Nombre.Contains(q) || e.Localidad.Contains(q))
    .OrderBy(e => e.Nombre)
    .Take(limite)
    .Select(e => new { e.Id, e.Nombre, e.Localidad })
    .ToListAsync();
```

Como refuerzo, `ProductosApiController.BuscarAsync` hace lo mismo para productos (`Take(10)`) y lo usa el ABM Vue. En ningún caso se trae la tabla completa para llenar un combo.

### ABM Vue.js SPA
Lo hice en Views/Comerciante/ProductosVue.cshtml con Vue 3 por CDN. Todo lo hago con fetch a /api/productos: listar, crear, editar y eliminar sin hacer reload. La ruta es /Comerciante/ProductosVue y redirigí el Index del comerciante para que vaya directo ahí.

**Alcance de Vue (para la defensa):** Vue se usa **solo** en `ProductosVue`. La búsqueda pública de comercios y productos es MVC clásico con formulario GET y filtros en `InicioController` (`q` + `pagina`), sin Vue ni AJAX. La gestión de roles también es MVC (`SuperAdminController` + `Views/SuperAdmin/Roles.cshtml`): la vista `RolesVue` se eliminó porque estaba rota y duplicaba funcionalidad. La campanita de notificaciones es un `ViewComponent` renderizado en servidor, sin JavaScript.

### Carrito básico (CLIENTE)
`CarritoController` reutiliza las tablas `Carritos` y `CarritoItems` que ya existían en el modelo. Si el cliente agrega el mismo producto dos veces, no crea otra fila: suma la cantidad. Antes de agregar o modificar valida que el producto esté activo y que haya stock suficiente; si no, muestra un mensaje de error. El total se calcula en el servidor y la vista `Views/Carrito/Index.cshtml` permite actualizar cantidades y eliminar ítems. El contador del header es un `ViewComponent` (`CarritoContadorViewComponent`) para no repetir la consulta en cada vista.

### Web API + JWT
Usé ServicioJwt.cs para generar el token con los roles del usuario. En AuthApiController hice el login que devuelve el token y en Program.cs configuré el doble esquema: cookies para el MVC y Bearer para la API. Los endpoints de crear, editar y eliminar productos están protegidos con [Authorize(Roles="COMERCIANTE,SUPER_ADMIN")].

### Manejo de archivos
Para el avatar del perfil y para los productos. En el SPA agregué un endpoint POST /api/productos/con-imagen que recibe FormData con la imagen y la guarda en wwwroot/imagenes/productos/. Valido extensión y tamaño máximo 2MB.

---

## 4. Estructura del repositorio

```
MultiTenantCSharpVargas/
├── Controllers/
│   ├── api/ - ProductosApiController.cs, AuthApiController.cs
│   ├── InicioController.cs - Catálogo público con paginado
│   ├── ComercianteController.cs - Panel, redirige a ProductosVue
│   ├── CarritoController.cs - Carrito básico del CLIENTE
│   └── CuentaController.cs, SuperAdminController.cs, PerfilController.cs
├── ViewComponents/ - CarritoContadorViewComponent, NotificacionesViewComponent
├── Data/
│   ├── ContextoAplicacion.cs - DbContext, relaciones y seed de roles/usuarios
│   └── SeedDatos.cs - Seed idempotente de empresas y productos de demo
├── Models/ - Empresa, Producto, ImagenProducto, Usuario, Rol, Carrito, etc.
├── ViewModels/ - TarjetaProductoVista, PaginacionVista, CarritoVista, etc.
├── Services/ - ServicioSesion, ServicioJwt
├── Views/
│   ├── Carrito/Index.cshtml - Carrito del cliente
│   ├── Comerciante/ProductosVue.cshtml - SPA Vue
│   └── Shared/_Paginacion.cshtml
├── wwwroot/css/componentes/ - catalogo.css, productos-vue.css, tarjetas.css, carrito.css
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

La dejé en `docs/PostmanCollection.json`. La variable `baseUrl` ya apunta a `http://localhost:5122`, así que solo hay que tener la app corriendo.

Para probar:
1. Ejecutar **Login JWT** (`POST /api/auth/login` con `Comerciante2@test.com` / `Comerciante123.`). El script de test guarda el token automáticamente en la variable `jwt_token`.
2. Ejecutar **Usuario autenticado (me)**: `GET /api/auth/me` con `Bearer {{jwt_token}}`.
3. **Listar productos paginado**: `GET /api/productos?page=1&pageSize=5&search=pan`.
4. **Búsqueda asíncrona**: `GET /api/productos/buscar?q=pa`.
5. **Crear producto**: `POST /api/productos` (JSON). Copiar el `id` devuelto en la variable `productoId`.
6. **Obtener**, **Editar** (`PUT`) y **Eliminar** (`DELETE`) usando `{{productoId}}`.
7. **Crear producto con imagen**: `POST /api/productos/con-imagen` con FormData (elegir un archivo en el campo `Imagen`).

Si no se ejecuta el login primero, los endpoints protegidos responden **401**. El token dura 8 horas (`Jwt:ExpireHours`).

La app ya consume la API desde el Vue, pero dejé la colección igual porque el enunciado la pide cuando la web no consume la API de forma nativa; sirve como evidencia y para la defensa.

---

## 7. Cómo correr el proyecto

Los pasos completos están al inicio del README, en **“Cómo iniciar el proyecto”**. Resumen:

```powershell
dotnet restore
dotnet ef database update
dotnet run
# Abrir http://localhost:5122
```

La aplicación aplica las migraciones y el seed de demostración automáticamente al iniciar. Para volver a sembrar los datos, alcanza con borrar las empresas de demo (o la base) y reiniciar.

Alternativa con el dump ya cargado:

```powershell
C:\xampp\mysql\bin\mysql.exe -u root -e "CREATE DATABASE IF NOT EXISTS tu_tienda_cerca CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
cmd /c "C:\xampp\mysql\bin\mysql.exe --default-character-set=utf8mb4 -u root tu_tienda_cerca < docs\base_datos_demo.sql"
```

---

## 8. DER

Diagrama exportado de MySQL Workbench 8.0 desde la base tu_tienda_cerca. Incluye las tablas principales.

Archivo: docs/DER.png

---

## 9. Qué falta pulir

El ABM Vue ya funciona creando productos con y sin foto. Queda grabar el video corto mostrando el flujo sin recargar para la defensa y terminar de pulir los estilos del catálogo.

