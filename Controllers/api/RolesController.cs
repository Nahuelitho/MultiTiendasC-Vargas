using Microsoft.AspNetCore.Mvc;
using MultiTenantCSharpVargas.Services;
using MultiTenantCSharpVargas.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MultiTenantCSharpVargas.Data;

namespace MultiTenantCSharpVargas.Controllers.api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly ContextoAplicacion _context;

        public RolesController(ContextoAplicacion context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            // Mapeamos las entidades 'Rol' de la BD directamente al DTO que espera recibir el paginador
            var queryDto = _context.Roles
                .Select(r => new RolDto
                {
                    Id = r.Id,
                    Nombre = r.Nombre,
                    Codigo = r.Codigo
                });

            var pagedResult = await queryDto.GetPagedAsync(page, pageSize);

            return Ok(pagedResult);
        }
    }
}
