using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Services;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize]
public class PerfilController(ContextoAplicacion db, IWebHostEnvironment entorno) : Controller
{
    private const long TamanioMaximoFoto = 2 * 1024 * 1024;
    private static readonly string[] ExtensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var usuario = await db.Usuarios.FindAsync(UsuarioId);
        return usuario == null ? NotFound() : View(usuario);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Actualizar(string nombre, IFormFile? avatar)
    {
        var usuario = await db.Usuarios.FindAsync(UsuarioId);
        if (usuario == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["Error"] = "El nombre es obligatorio.";
            return RedirectToAction(nameof(Index));
        }

        if (avatar is { Length: > 0 })
        {
            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension) || avatar.Length > TamanioMaximoFoto)
            {
                TempData["Error"] = "La foto debe ser JPG, PNG, WEBP o GIF y pesar menos de 2 MB.";
                return RedirectToAction(nameof(Index));
            }

            EliminarArchivo(usuario.AvatarUrl);

            var carpeta = Path.Combine(entorno.WebRootPath, "imagenes", "avatares");
            Directory.CreateDirectory(carpeta);

            var archivo = $"avatar_{usuario.Id}{extension}";
            var ruta = Path.Combine(carpeta, archivo);

            await using (var stream = System.IO.File.Create(ruta))
            {
                await avatar.CopyToAsync(stream);
            }

            usuario.AvatarUrl = $"/imagenes/avatares/{archivo}";
        }

        usuario.Nombre = nombre.Trim();
        await db.SaveChangesAsync();

        TempData["Mensaje"] = "Perfil actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarAvatar()
    {
        var usuario = await db.Usuarios.FindAsync(UsuarioId);
        if (usuario == null)
        {
            return NotFound();
        }

        EliminarArchivo(usuario.AvatarUrl);
        usuario.AvatarUrl = null;
        await db.SaveChangesAsync();

        TempData["Mensaje"] = "Foto de perfil eliminada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(string actual, string nueva, string repetida)
    {
        var usuario = await db.Usuarios.FindAsync(UsuarioId);
        if (usuario == null)
        {
            return NotFound();
        }

        if (!PasswordHasher.Verify(actual, usuario.ContrasenaHash))
        {
            TempData["Error"] = "La contraseña actual no es correcta.";
        }
        else if (string.IsNullOrWhiteSpace(nueva) || nueva != repetida)
        {
            TempData["Error"] = "La nueva contraseña no coincide.";
        }
        else
        {
            usuario.ContrasenaHash = PasswordHasher.Hash(nueva);
            await db.SaveChangesAsync();
            TempData["Mensaje"] = "Contraseña actualizada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    private void EliminarArchivo(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        var ruta = Path.Combine(entorno.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(ruta))
        {
            System.IO.File.Delete(ruta);
        }
    }
}
