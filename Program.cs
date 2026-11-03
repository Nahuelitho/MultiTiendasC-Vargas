using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;

var constructor = WebApplication.CreateBuilder(args);
constructor.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Agrega los servicios MVC y autenticación por cookies.
constructor.Services.AddControllersWithViews();
constructor.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.LogoutPath = "/Cuenta/Logout";
        opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    });

var cadenaConexion = constructor.Configuration.GetConnectionString("BaseDeDatos");
if (!string.IsNullOrWhiteSpace(cadenaConexion))
{
    constructor.Services.AddDbContext<ContextoAplicacion>(opciones =>
        opciones.UseMySql(cadenaConexion, ServerVersion.AutoDetect(cadenaConexion)));
}

var aplicacion = constructor.Build();

// Configura el flujo de peticiones.
if (!aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseExceptionHandler("/Inicio/Error");
    aplicacion.UseHsts();
}

aplicacion.UseHttpsRedirection();
aplicacion.UseStaticFiles();

aplicacion.UseRouting();

aplicacion.UseAuthentication();
aplicacion.UseAuthorization();

aplicacion.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Inicio}/{id?}");

aplicacion.Run();
