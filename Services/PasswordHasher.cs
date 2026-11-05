using System.Security.Cryptography;
using System.Text;

namespace MultiTenantCSharpVargas.Services;

public static class PasswordHasher
{
    private const int Iteraciones = 210_000;
    private const int TamanioSalt = 16;
    private const int TamanioHash = 32;
    private const string Prefijo = "pbkdf2-sha256";

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanioSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iteraciones,
            HashAlgorithmName.SHA256,
            TamanioHash);

        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash) || !hash.StartsWith($"{Prefijo}$", StringComparison.Ordinal))
        {
            return false;
        }

        var partes = hash.Split('$');
        if (partes.Length != 4 || !int.TryParse(partes[1], out var iteraciones) || iteraciones <= 0)
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iteraciones,
                HashAlgorithmName.SHA256,
                esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
