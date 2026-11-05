using Microsoft.AspNetCore.Mvc;
using MultiTenantCSharpVargas.Services;
using MultiTenantCSharpVargas.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var query = _context.Roles.AsQueryable();

            var pagedResult = await query.GetPagedAsync<RolDto>(page, pageSize);

            return Ok(pagedResult);
        }
    }
}