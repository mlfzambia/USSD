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

        public async Task<ModelView.PaymentTransactionMV.PaymentResponse> ITransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider, decimal Amount, string LoanType, string ClientLoanId)
        {
            var RegistrationResponse = await Task.Run(() => TrxClient.TransactionRegistration(Clientaccount, PhoneNumber, ServiceProvider, Amount, LoanType,  ClientLoanId));
            return RegistrationResponse;
        }


        public async Task<ModelView.GenralResponseMV.AllGenralResponse> IAddSessionBalance(string SessionId, string ProductId, string ProductName, decimal Amount, int ClientOptionId, string ClientLoanId)
        {
            var AddSessionAmount = await Task.Run(() => TrxClient.AddSessionBalance(SessionId, ProductId, ProductName, Amount, ClientOptionId, ClientLoanId));
            return AddSessionAmount;
        }

        //Update client account number
        public async Task<ModelView.GenralResponseMV.AllGenralResponse> IUpdateAccountNumber(string Clientaccount, string SessionId, string AccountHolderName)
        {
            var UpdateAccountnumberResponse = await Task.Run(() => TrxClient.UpdateAccountNumber(Clientaccount, SessionId, AccountHolderName));
            return UpdateAccountnumberResponse;
        }

        //Complete Payment transaction
        public async void ICompleteLoanPayment(string SessionId, decimal PaidLoanAmount)
        {
            await Task.Run(() => TrxClient.CompleteLoanPayment(SessionId, PaidLoanAmount));
            //  return UpdateAccountnumberResponse;
        }


        //
    }
}
