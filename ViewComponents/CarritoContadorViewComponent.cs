using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;

namespace MultiTenantCSharpVargas.ViewComponents;

public class CarritoContadorViewComponent(ContextoAplicacion db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Solo los clientes compran: para otros roles no se muestra el acceso.
        if (UserClaimsPrincipal?.IsInRole("CLIENTE") != true)
        {
            return Content(string.Empty);
        }

        var idClaim = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var usuarioId))
        {
            return Content(string.Empty);
        }

        var total = await db.CarritoItems
            .AsNoTracking()
            .Where(item => item.Carrito.UsuarioId == usuarioId)
            .SumAsync(item => (int?)item.Cantidad) ?? 0;

        return View(total);
    }
}
