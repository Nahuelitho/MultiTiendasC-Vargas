using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;
using Microsoft.AspNetCore.HttpOverrides;

// Evita ambigüedades al interpretar decimales enviados por formularios HTML
// (los inputs numéricos siempre usan punto como separador decimal).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var constructor = WebApplication.CreateBuilder(args);
constructor.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

constructor.Services.AddControllersWithViews(opciones =>
{
    // Las propiedades de navegación de EF no llegan en los formularios:
    // no deben validarse como campos obligatorios.
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
constructor.Services.AddScoped<ServicioSesion>();
constructor.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Cuenta/Login";
        opciones.LogoutPath = "/Cuenta/Logout";
        opciones.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        opciones.Cookie.HttpOnly = true;
        opciones.Cookie.SameSite = SameSiteMode.Lax;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
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
    var registro = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetService<ContextoAplicacion>();
    if (db != null)
    {
        try
        {
            db.Database.Migrate();
            registro.LogInformation("Migraciones aplicadas correctamente.");
        }
        catch (Exception ex)
        {
            // No se corta el arranque: la aplicación muestra un error amigable
            // en las páginas que necesitan la base de datos.
            registro.LogError(ex, "No se pudieron aplicar las migraciones de la base de datos.");
        }
    }
    else
    {
        registro.LogWarning("ContextoAplicacion no registrado: falta ConnectionStrings:BaseDeDatos.");
    }
}

aplicacion.UseForwardedHeaders();

if (aplicacion.Environment.IsDevelopment())
{
    aplicacion.UseDeveloperExceptionPage();
}
else
{
    // Errores no controlados -> vista amigable sin exponer detalles internos.
    aplicacion.UseExceptionHandler("/Inicio/Error");
    aplicacion.UseHsts();
}

// 404, 400, 403, etc. -> misma vista de error con el código correspondiente.
aplicacion.UseStatusCodePagesWithReExecute("/Inicio/Error/{0}");

aplicacion.UseStaticFiles();
aplicacion.UseRouting();
aplicacion.UseAuthentication();
aplicacion.UseAuthorization();

aplicacion.MapControllers(); // <- agregalo por si tenes API
aplicacion.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Inicio}/{id?}");

aplicacion.Run();