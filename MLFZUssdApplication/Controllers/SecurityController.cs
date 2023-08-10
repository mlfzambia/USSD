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
        public async Task<IActionResult> CreateAPiKey([FromQuery] string ClientName)
        {
            var SecurityResponse = await Task.Run(() => _SecurityCheck.SecurityGenerator(ClientName));
            return Ok(SecurityResponse);
        }

        [HttpGet]
        [Route("UssdOnOffStatus")]
        public async Task<IActionResult> UssdOnOffStatus([FromQuery] int UssdStatusId)
        {
            var UssdSecurityResponse = await Task.Run(() => _SecurityCheck.ShutDownUssdOperation(UssdStatusId));
            return Ok(UssdSecurityResponse);
        }


    }
}
