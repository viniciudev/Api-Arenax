using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quadra.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeebHookController : ControllerBase
    {
        [HttpPost("identificador")]
        public async Task<IActionResult> Post([FromBody] string id)
        {
            return Ok("Conexão efetuada com sucesso");
        }
    }
}
