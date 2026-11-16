using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "CLIENTE")]
public class ValoracionController(ContextoAplicacion db) : Controller
{
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int empresaId, int puntuacion, string? comentario)
    {
        var empresa = await db.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == empresaId);

        if (empresa is null)
        {
            return NotFound();
        }

        if (empresa.PropietarioId == UsuarioId)
        {
            TempData["Error"] = "No podés valorar tu propio comercio.";
            return RedirectToAction("Empresa", "Inicio", new { id = empresaId });
        }

        if (puntuacion < 1 || puntuacion > 5)
        {
            TempData["Error"] = "La puntuación debe ser entre 1 y 5 estrellas.";
            return RedirectToAction("Empresa", "Inicio", new { id = empresaId });
        }

        comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();
        if (comentario is { Length: > 500 })
        {
            comentario = comentario[..500];
        }

        // El índice único (UsuarioId, EmpresaId) garantiza una sola valoración por cliente.
        var valoracion = await db.Valoraciones
            .FirstOrDefaultAsync(v => v.UsuarioId == UsuarioId && v.EmpresaId == empresaId);

        var esNueva = valoracion is null;

        if (esNueva)
        {
            db.Valoraciones.Add(new Valoracion
            {
                UsuarioId = UsuarioId,
                EmpresaId = empresaId,
                Puntuacion = puntuacion,
                Comentario = comentario,
                FechaCreacion = DateTime.UtcNow
            });
        }
        else
        {
            valoracion!.Puntuacion = puntuacion;
            valoracion.Comentario = comentario;
            valoracion.FechaCreacion = DateTime.UtcNow;
        }

        // Avisa al dueño del comercio para que lo vea en la campanita.
        db.Notificaciones.Add(new Notificacion
        {
            UsuarioId = empresa.PropietarioId,
            EmpresaId = empresa.Id,
            Titulo = esNueva ? "Nueva valoración" : "Valoración actualizada",
            Mensaje = $"\"{empresa.Nombre}\" recibió {puntuacion} estrella{(puntuacion == 1 ? "" : "s")}.",
            Fecha = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        TempData["Mensaje"] = esNueva
            ? "¡Gracias por valorar este comercio!"
            : "Tu valoración se actualizó correctamente.";

        return RedirectToAction("Empresa", "Inicio", new { id = empresaId });
    }
}
