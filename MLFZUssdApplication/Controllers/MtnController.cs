using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Cryptography.Xml;

namespace MLFZUssdApplication.Controllers
{
    [Route("api/MTNPayments/")]
    [ApiController]
    public class MtnController : ControllerBase
    {
        MusoniRequest.MusoniClientsRequestAsync MC_client = new MusoniRequest.MusoniClientsRequestAsync();

        MtnPaymentProcessing.MTNPayments MtnPay = new MtnPaymentProcessing.MTNPayments();

        [HttpGet]
        [Route("tokenRequest")]
        [ActionName("tokenRequest")]
        public async Task<IActionResult> MtnTokenCaller([FromQuery] string phonenumber)
        {
            var TokenResponse = await Task.Run(() => MC_client.IGetClientAccountBalance(phonenumber));
            return Ok(TokenResponse);

        }

        [HttpGet]
        [Route("VerifyRequested")]
        [ActionName("VerifyRequest")]
        public async Task<IActionResult> VerifyRequest([FromQuery] string ClientNumber)
        {
            var TokenResponse = await Task.Run(() => MtnPay.MtnClientNameDetails(ClientNumber));
            return Ok(TokenResponse);

        }

       
    }
}
