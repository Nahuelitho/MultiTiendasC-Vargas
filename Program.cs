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

// AUTH DUAL: Cookies para MVC + JWT para API
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ClaveSuperSecretaDe32CaracteresMinimo123!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TuTiendaCerca";

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt => {
        opt.LoginPath = "/Cuenta/Login";
        opt.AccessDeniedPath = "/Cuenta/AccesoDenegado";
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
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

// Migraciones auto
using(var scope = app.Services.CreateScope()){
    try{
        var ctx = scope.ServiceProvider.GetRequiredService<ContextoAplicacion>();
        ctx.Database.Migrate();
    }catch(Exception ex){
        Console.WriteLine($"Migración falló: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment()){
    app.UseExceptionHandler("/Inicio/Error");
    app.UseHsts();
}else{
    app.UseDeveloperExceptionPage();
}
app.UseStatusCodePagesWithReExecute("/Inicio/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Inicio}/{action=Inicio}/{id?}");
app.MapControllers(); // Para API
app.Run();