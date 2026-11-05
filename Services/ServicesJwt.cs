using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Services;

public class ServicioJwt
{
    private readonly IConfiguration _config;
    public ServicioJwt(IConfiguration config) => _config = config;

    public string GenerarToken(Usuario usuario, List<string> roles)
    {
        var key = _config["Jwt:Key"] ?? "ClaveSuperSecretaDe32CaracteresMinimo123!";
        var issuer = _config["Jwt:Issuer"] ?? "TuTiendaCerca";
        var audience = _config["Jwt:Audience"] ?? "TuTiendaCercaApp";
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Email, usuario.Correo),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach(var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}