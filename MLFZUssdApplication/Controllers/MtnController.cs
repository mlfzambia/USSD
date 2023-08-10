using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MLFZUssdApplication.Controllers
{
    [Route("api/MTNPayments/")]
    [ApiController]
    public class MtnController : ControllerBase
    {
        MusoniRequest.MusoniClientsRequestAsync MC_client = new MusoniRequest.MusoniClientsRequestAsync();


        [HttpGet]
        [Route("tokenRequest")]
        [ActionName("tokenRequest")]
        public async Task<IActionResult> MtnTokenCaller([FromQuery] string phonenumber)
        {
            var TokenResponse = await Task.Run(() => MC_client.IGetClientAccountBalance(phonenumber));
            return Ok(TokenResponse);

        }

        //[HttpGet]
        //[Route("VerifyRequest")]
        //[ActionName("VerifyRequest")]
        //public async Task<IActionResult> VerifyRequest([FromQuery] string TraxRefId )
        //{
        //    var TokenResponse = await Task.Run(() => MtnPayments.Request_To_Pay_Transaction_Status(TraxRefId));
        //    return Ok(TokenResponse);

        //}


    }
}
