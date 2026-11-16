using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.ViewModels;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize]
public class NotificacionesController(ContextoAplicacion db) : Controller
{
    private const int TamanoPagina = 10;

    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Al entrar a la página el usuario ya las vio: se marcan todas como leídas.
    [HttpGet]
    public async Task<IActionResult> Index(int pagina = 1)
    {
        await db.Notificaciones
            .Where(n => n.UsuarioId == UsuarioId && !n.Leida)
            .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.Leida, true));

        var consulta = db.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == UsuarioId);

        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var notificaciones = await consulta
            .OrderByDescending(n => n.Fecha)
            .Skip((pagina - 1) * TamanoPagina)
            .Take(TamanoPagina)
            .ToListAsync();

        ViewBag.Paginacion = new PaginacionVista
        {
            Pagina = pagina,
            TotalPaginas = totalPaginas,
            PageSize = TamanoPagina,
            TotalItems = total,
            Controlador = "Notificaciones",
            Accion = nameof(Index)
        };

        return View(notificaciones);
    }
}
