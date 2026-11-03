using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantCSharpVargas.Data;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Controllers;

[Authorize(Roles = "COMERCIANTE")]
public class ComercianteController(ContextoAplicacion db) : Controller
{
    private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var empresa = await db.Empresas.FirstOrDefaultAsync(e => e.PropietarioId == UsuarioId);
        return View(empresa);
    }

    public async Task<IActionResult> CrearEmpresa()
    {
        if (await db.Empresas.AnyAsync(e => e.PropietarioId == UsuarioId))
        {
            return RedirectToAction(nameof(Index));
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CrearEmpresa(Empresa empresa)
    {
        if (await db.Empresas.AnyAsync(e => e.PropietarioId == UsuarioId))
        {
            return RedirectToAction(nameof(Index));
        }

        empresa.PropietarioId = UsuarioId;
        empresa.Estado = EstadoEmpresa.Pendiente;
        empresa.FechaAlta = DateTime.UtcNow;

        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();

        // Vincula el rol COMERCIANTE del usuario a la empresa creada.
        var usuarioRol = await db.UsuarioRoles
            .Include(ur => ur.Rol)
            .FirstOrDefaultAsync(ur => ur.UsuarioId == UsuarioId && ur.Rol.Codigo == "COMERCIANTE");
        if (usuarioRol != null)
        {
            usuarioRol.EmpresaId = empresa.Id;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
