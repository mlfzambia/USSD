using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MLFZUssdApplication.Controllers
{
 
     
   [Authorize]

    [Route("api/UssdRequest/")]
    [ApiController]
    public class UssdRequestController : ControllerBase
    {
 

  UssdProcessRequest.ProcessRequestAsync PR_Client_Request = new UssdProcessRequest.ProcessRequestAsync();


        [HttpGet]
        [Route("UssdCaller")]
        public async Task<IActionResult> UssdCaller([FromQuery] int isnewrequest, string msisdn, string sessionid, string input)
        {
            var PR_Client_Details = await Task.Run(() => PR_Client_Request.UssdRequestDetails(isnewrequest, msisdn, sessionid, input));
            return Ok(PR_Client_Details);
        }
    }
}
