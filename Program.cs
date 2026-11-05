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

// Aplica migraciones solo si hay DB configurada
using (var scope = aplicacion.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<ContextoAplicacion>();
    if (db != null)
    {
        try {
            db.Database.Migrate();
            Console.WriteLine("Migraciones aplicadas OK");
        } catch (Exception ex) {
            Console.WriteLine($"Error migrando DB: {ex.Message}");
            Console.WriteLine(ex.ToString());
        }
    }
    else
    {
        Console.WriteLine("ADVERTENCIA: ContextoAplicacion no registrado - ConnectionStrings__BaseDeDatos vacia o mal escrita");
    }
}

aplicacion.UseForwardedHeaders();

// En Render dejalo comentado para ver el error real
// if (!aplicacion.Environment.IsDevelopment())
// {
//     aplicacion.UseExceptionHandler("/Inicio/Error");
//     aplicacion.UseHsts();
// }

aplicacion.UseStaticFiles();
aplicacion.UseRouting();
aplicacion.UseAuthentication();
aplicacion.UseAuthorization();

aplicacion.MapControllers(); // <- agregalo por si tenes API
aplicacion.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Inicio}/{id?}");

aplicacion.Run();