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
        Task<ModelView.PaymentTransactionMV.PaymentResponse> ITransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider, decimal Amount, string LoanType, string ClientLoanId, string SessionId);

        //
        Task<ModelView.GenralResponseMV.AllGenralResponse> IAddSessionBalance(string SessionId, string ProductId, string ProductName, decimal Amount, int ClientOptionId, string ClientLoanId);

        //Update account 
        Task<ModelView.GenralResponseMV.AllGenralResponse> IUpdateAccountNumber(string Clientaccount, string SessionId, string AccountHolderName, string PayerPhonenumber);

        //Update transaction 
        Task<ModelView.GenralResponseMV.AllGenralResponse> ICompleteLoanPayment(string SessionId, decimal PaidLoanAmount);

        //Payment verification
        Task<ModelView.GenralResponseMV.AllGenralResponse> IConfirmPendingTransaction();
    }
}
