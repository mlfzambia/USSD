using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MLFZUssdApplication.Controllers
{


    //[Authorize]

    [Route("api/UssdRequest/")]
    [ApiController]
    public class UssdRequestController : ControllerBase
    {
        UssdProcessRequest.IProcessRequest PR_Client_Request = new UssdProcessRequest.ProcessRequestAsync();


        [HttpGet]
        [Route("UssdCaller")]
        public async Task<IActionResult> UssdCaller([FromQuery] int isnewrequest, string msisdn, string sessionid, string input)
        {
            var HeaderResponse = HttpContext.Response.Headers; //Response Headers

            var PR_Client_Details = await Task.Run(() => PR_Client_Request.UssdRequestDetails(isnewrequest, msisdn, sessionid, input));

            HeaderResponse.Add("Freeflow", PR_Client_Details.responseValue);
            return Ok(PR_Client_Details.notification);

        }


        //Testing
        [HttpGet]
        [Route("PaymentVerification")]
        [ActionName("PaymentVerification")]
        public async Task<IActionResult> PaymentVerification()
        {
            UssdTransaction.IUssdTransaction ussd = new UssdTransaction.UssdTransactionAsync();

            var TransactionVerificationResponse = await Task.Run(() => ussd.IConfirmPendingTransaction());
            string statusCode = TransactionVerificationResponse.statusCode;
            if (statusCode == "OT001")
            {

                return Ok(TransactionVerificationResponse);
            }
            else
            {
                return NotFound(TransactionVerificationResponse);

                // return NotFound("System down");asdasdads
            } 
        }





    }
}
