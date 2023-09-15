using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelView
{
    public class Portal_InforMV
    {

        //All Transaction Call

        public class TransactionResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public List<TransactionDetails> details { get; set; }

        }

        public class TransactionDetails
        {
            public string incrmid { get; set; }
            public string clientaccount { get; set; }
            public string phoneNumber { get; set; }
            public string extTransactionId { get; set; }
            public string trxDate { get; set; }
            public decimal amount { get; set; }
            public string networkName { get; set; }
            public string transactionStatus { get; set; }
            public string LoanType { get; set; }
            public string clientLoanId { get; set; }
            public string receiptNum { get; set; }
            public string accountHolderName { get; set; }
            public string sessionId { get; set; }
            public string payerName { get; set; }
            public string errorResponse { get; set; }
        }



    }
}
