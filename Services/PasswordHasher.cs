using System.Security.Cryptography;
using System.Text;

namespace MultiTenantCSharpVargas.Services;

public static class PasswordHasher
{
    private static readonly byte[] Clave = Encoding.UTF8.GetBytes("tu-tienda-cerca-2026");

    public static string Hash(string password)
    {
        using var hmac = new HMACSHA256(Clave);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
    }

    public static bool Verify(string password, string hash) =>
        Hash(password) == hash;
}
