using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MLFZUssdApplication.Controllers
{
    [Route("api/Security/")]
    [ApiController]
    public class SecurityController : ControllerBase
    {

        private readonly SecuritySystemCheck.SecurityCheck _SecurityCheck;

        public SecurityController(SecuritySystemCheck.SecurityCheck _crSecurityCheck)
        {
            _SecurityCheck = _crSecurityCheck;
        }

        [HttpGet]
        [Route("CreateAPiKey")]
        public async Task<IActionResult> CreateAPiKey()
        {
            var SecurityResponse = await Task.Run(() => _SecurityCheck.SecurityGenerator());

            return Ok(SecurityResponse);


        }

    }
}
