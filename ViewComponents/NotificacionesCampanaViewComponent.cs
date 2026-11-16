using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;

namespace MultiTenantCSharpVargas.ViewComponents;

public class NotificacionesCampanaViewComponent(ContextoAplicacion db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (UserClaimsPrincipal?.Identity?.IsAuthenticated != true)
        {
            return Content(string.Empty);
        }

        var idClaim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var usuarioId))
        {
            return Content(string.Empty);
        }

        var total = await db.Notificaciones
            .AsNoTracking()
            .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

        return View(total);
    }
}
