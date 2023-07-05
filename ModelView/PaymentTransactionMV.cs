using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelView
{
    public class PaymentTransactionMV
    {

        public class PaymentResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public PaymentDetails details { get; set; }
        }

        public class PaymentDetails
        {
            public string transactionId { get; set; }

        }



    }
}
