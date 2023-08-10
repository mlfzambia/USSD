using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelView
{
    public class MTNModelview
    {

        //Token Details

        public class TokenDetails
        {
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string expires_in { get; set; }
        }

        public class TokenResponseDetails
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public TokenDetails details { get; set; }
        }

        public class ClientMsisdn
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public ClientMsisdnInformation information { get; set; }
        }

        public class ClientMsisdnInformation
        {
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string birthdate { get; set; }
            public string locale { get; set; }
            public string gender { get; set; }
            public string status { get; set; }
        }


        //Request to pay
        public class RequestToPay
        {
            public string amount { get; set; }
            public string currency { get; set; }
            public string externalId { get; set; }
            public Payerdetails payer { get; set; }
            public string payerMessage { get; set; }
            public string payeeNote { get; set; }
        }

        public class Payerdetails
        {
            public string partyIdType { get; set; }
            public string partyId { get; set; }
        }



        //Mtn Payment verification

        public class MtnPaymentVerificationResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public Mtn_Payment_Verification verification { get; set; }
        }


        public class Mtn_Payment_Verification
        {
            public string amount { get; set; }
            public string currency { get; set; }
            public string financialTransactionId { get; set; }
            public string externalId { get; set; }
            public clientNumberdetails payer { get; set; }
            public string status { get; set; }
            public Reasondetails reason { get; set; }
        }

        public class clientNumberdetails
        {
            public string partyIdType { get; set; }
            public string partyId { get; set; }
        }

        public class Reasondetails
        {
            public string code { get; set; }
            public string message { get; set; }
        }

    }
}
