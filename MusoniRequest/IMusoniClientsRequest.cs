using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusoniRequest
{
    interface IMusoniClientsRequest
    {
        Task<ModelView.SannyResponseMV.SearchDetailsResponse> IMusoniClientDetails(string Phonenumber);


        Task<ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse> IGetClientAccountBalance(string Phonenumber);

        //Repayment detail 
        Task<ModelView.SannyResponseMV.loanPendingApprovalResponse> IPostAccountBalance(decimal PaymentAmount, string receiptNumber, string AccountNum,
            string ClientloanId);

        //Approval Details
        Task<ModelView.GenralResponseMV.AllGenralResponse> IAuthorizaPostRepaymentMusoni(string CommandId);

       
    }
}
