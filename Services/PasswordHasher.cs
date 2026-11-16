using System.Security.Cryptography;
using System.Text;

namespace MultiTenantCSharpVargas.Services;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 en formato PHC string.
///
/// ¿Qué es el formato PHC?
/// Es una cadena autodescriptiva con esta forma:
///     algoritmo $ iteraciones $ salt (Base64) $ hash (Base64)
///     pbkdf2-sha256 $ 210000 $ GNnBg2xd... $ jOj6rSsx...
///
/// ¿Para qué sirve?
/// - El salt es aleatorio por usuario, así que para verificar hay que guardarlo.
/// - Guarda también el algoritmo y las iteraciones usadas, para poder recalcular
///   exactamente el mismo hash al iniciar sesión.
/// - Permite subir las iteraciones en el futuro sin romper los hashes viejos:
///   cada uno conserva sus propios parámetros.
/// - Nunca se guarda la contraseña en texto plano, solo este string.
/// </summary>
public static class PasswordHasher
{
    private const int Iteraciones = 210_000;
    private const int TamanioSalt = 16;
    private const int TamanioHash = 32;
    // Algoritmo declarado dentro del propio hash (parte 1 del formato PHC).
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

        // Se guarda el string PHC completo: algoritmo$iteraciones$salt$hash.
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string hash)
    {
        // Se parsea el string PHC para recuperar algoritmo, iteraciones, salt y hash esperado.
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
