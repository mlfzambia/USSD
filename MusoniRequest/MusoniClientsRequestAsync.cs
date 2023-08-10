using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusoniRequest
{
    public class MusoniClientsRequestAsync : IMusoniClientsRequest
    {
        MusoniClientsRequest CR_Client = new MusoniClientsRequest();

        public async Task<ModelView.SannyResponseMV.SearchDetailsResponse> IMusoniClientDetails(string Phonenumber)
        {
            var Mu_SearchResponse = await Task.Run(() => CR_Client.GetClientDetails(Phonenumber));
            return Mu_SearchResponse;
        }

        public async Task<ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse> IGetClientAccountBalance(string Phonenumber)
        {
            var Mu_SearchClientBalance = await Task.Run(() => CR_Client.GetClientAccountBalance(Phonenumber));
            return Mu_SearchClientBalance;
        }

        //Post current transaction 
        public async Task<ModelView.SannyResponseMV.loanPendingApprovalResponse> IPostAccountBalance(decimal PaymentAmount, string receiptNumber, string AccountNum,
            string ClientloanId)
        {
            var Mu_RepaymentDetailClient = await Task.Run(() => CR_Client.PostRepaymentMusoni(PaymentAmount, receiptNumber, AccountNum,
            ClientloanId));
            return Mu_RepaymentDetailClient;
        }


        //Approve Transaction
        public async Task<ModelView.GenralResponseMV.AllGenralResponse> IAuthorizaPostRepaymentMusoni(string CommandId)
        {
            var ApproveStatus = await Task.Run(() => CR_Client.AuthorizaPostRepaymentMusoni(CommandId));
            return ApproveStatus;
        }
    }
}
