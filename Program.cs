using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;
using Microsoft.AspNetCore.HttpOverrides;

var constructor = WebApplication.CreateBuilder(args);
constructor.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

constructor.Services.AddControllersWithViews();
constructor.Services.AddScoped<ServicioSesion>();
constructor.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.LogoutPath = "/Cuenta/Logout";
        opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    });

// Para que Render no rompa con el https
constructor.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var cadenaConexion = constructor.Configuration.GetConnectionString("BaseDeDatos");
if (!string.IsNullOrWhiteSpace(cadenaConexion))
{
    constructor.Services.AddDbContext<ContextoAplicacion>(opciones =>
        opciones.UseMySql(cadenaConexion, ServerVersion.AutoDetect(cadenaConexion)));
}

var aplicacion = constructor.Build();

// Aplica migraciones automatico al arrancar - CLAVE PARA RENDER
using (var scope = aplicacion.Services.CreateScope())
{
    try {
        var db = scope.ServiceProvider.GetRequiredService<ContextoAplicacion>();
        db.Database.Migrate();
    } catch (Exception ex) {
        Console.WriteLine($"Error migrando DB: {ex.Message}");
    }
}

if (!aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseExceptionHandler("/Inicio/Error");
    aplicacion.UseHsts();
}

aplicacion.UseForwardedHeaders(); // <- agregado
// aplicacion.UseHttpsRedirection(); <- COMENTALO en Render, si no falla

aplicacion.UseStaticFiles();
aplicacion.UseRouting();
aplicacion.UseAuthentication();
aplicacion.UseAuthorization();

aplicacion.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Inicio}/{id?}");

aplicacion.Run();