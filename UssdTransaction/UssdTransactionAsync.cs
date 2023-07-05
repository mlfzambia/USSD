using ModelView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UssdTransaction
{
    public class UssdTransactionAsync : IUssdTransaction
    {
        UssdTransaction TrxClient = new UssdTransaction();

        //Register transaction

        public async Task<ModelView.PaymentTransactionMV.PaymentResponse> ITransactionRegistration(string Clientaccount, string PhoneNumber,  int ServiceProvider,
            int TransactionStatus)
        {
            var RegistrationResponse = await Task.Run(() => TrxClient.TransactionRegistration(Clientaccount, PhoneNumber,  ServiceProvider, TransactionStatus));
            return RegistrationResponse;
        }

    }
}
