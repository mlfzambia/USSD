using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UssdTransaction
{
    public interface IUssdTransaction
    {
        //Interface
       Task< ModelView.PaymentTransactionMV.PaymentResponse> ITransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider,
            int TransactionStatus);

    }
}
