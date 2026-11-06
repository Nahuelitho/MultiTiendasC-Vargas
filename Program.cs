using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DB
var conn = builder.Configuration.GetConnectionString("BaseDeDatos") 
           ?? builder.Configuration["ConnectionStrings__BaseDeDatos"]
           ?? "Server=localhost;Port=3306;Database=tu_tienda_cerca;User ID=root;Password=root;";
builder.Services.AddDbContext<ContextoAplicacion>(opt => 
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn)));

// Servicios tuyos
builder.Services.AddScoped<ServicioSesion>();
builder.Services.AddScoped<ServicioJwt>();

// AUTH DUAL: Cookies para MVC + JWT para API.
// Un esquema "policy" elige el handler según el request:
// - Authorization: Bearer ... -> JwtBearer
// - caso contrario             -> Cookie (navegador / SPA mismo origen)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecretaDe32CaracteresMinimo123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TuTiendaCerca";

const string esquemaCookiesOJwt = "CookiesOrJwt";

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = esquemaCookiesOJwt;
        options.DefaultAuthenticateScheme = esquemaCookiesOJwt;
        options.DefaultChallengeScheme = esquemaCookiesOJwt;
        options.DefaultForbidScheme = esquemaCookiesOJwt;
    })
    .AddPolicyScheme(esquemaCookiesOJwt, "Cookies o JWT", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var autorizacion = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(autorizacion) &&
                autorizacion.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            return CookieAuthenticationDefaults.AuthenticationScheme;
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
    {
        opt.LoginPath = "/Cuenta/Login";
        opt.AccessDeniedPath = "/Cuenta/AccesoDenegado";

        // Para las llamadas AJAX de la SPA devolvemos 401/403 en JSON,
        // nunca un redirect a HTML (eso producía "Unexpected token '<'").
        opt.Events.OnRedirectToLogin = contexto =>
        {
            if (contexto.Request.Path.StartsWithSegments("/api"))
            {
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return contexto.Response.WriteAsJsonAsync(new { error = "No autenticado." });
            }

            contexto.Response.Redirect(contexto.RedirectUri);
            return Task.CompletedTask;
        };

        opt.Events.OnRedirectToAccessDenied = contexto =>
        {
            if (contexto.Request.Path.StartsWithSegments("/api"))
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                return contexto.Response.WriteAsJsonAsync(new { error = "No tenés permisos para esta acción." });
            }

            contexto.Response.Redirect(contexto.RedirectUri);
            return Task.CompletedTask;
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "TuTiendaCercaApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Migraciones auto + datos de demostración
using(var scope = app.Services.CreateScope()){
    try{
        var ctx = scope.ServiceProvider.GetRequiredService<ContextoAplicacion>();
        ctx.Database.Migrate();
        await SeedDatos.SembrarAsync(ctx, app.Logger);
    }catch(Exception ex){
        Console.WriteLine($"Migración/seed falló: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment()){
    app.UseExceptionHandler("/Inicio/Error");
    app.UseHsts();
}else{
    app.UseDeveloperExceptionPage();
}
app.UseWhen(
    contexto => !contexto.Request.Path.StartsWithSegments("/api"),
    rama => rama.UseStatusCodePagesWithReExecute("/Inicio/Error/{0}"));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Inicio}/{action=Inicio}/{id?}");
app.MapControllers(); // Para API
app.Run();