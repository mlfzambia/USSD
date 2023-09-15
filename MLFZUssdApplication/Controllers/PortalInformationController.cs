using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MLFZUssdApplication.Controllers
{
    [Route("api/PortalInformation/")]
    [ApiController]
    public class PortalInformationController : ControllerBase
    {
        MLFPortalInformation.IPortalInformation _PIInformation = new MLFPortalInformation.PortalInformationAsyn();


        [HttpGet]
        [Route("AllTransactionInformation")]
        public async Task<IActionResult> AllTransactionInformation()
        {
         var PaymentResponses = await Task.Run(()=>   _PIInformation.ITranstionListing());
            string StatusCode = PaymentResponses.statusCode;

            if(StatusCode == "OT001")
            {
                return Ok(PaymentResponses);

            }
            else
            {
                return NotFound(PaymentResponses);

            }

        }


    }
}
