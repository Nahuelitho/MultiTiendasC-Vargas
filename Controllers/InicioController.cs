using Microsoft.AspNetCore.Mvc;
using MultiTenantCSharpVargas.Models;

namespace MultiTenantCSharpVargas.Controllers;

public class InicioController : Controller
{
    public IActionResult Inicio()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ModeloError { IdentificadorSolicitud = HttpContext.TraceIdentifier });
    }
}
