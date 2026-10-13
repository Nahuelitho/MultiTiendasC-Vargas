using Microsoft.AspNetCore.Mvc;

namespace MultiTenantCSharpVargas.Controllers;

public class CuentaController : Controller
{
    public IActionResult Crear()
    {
        return View();
    }
}
