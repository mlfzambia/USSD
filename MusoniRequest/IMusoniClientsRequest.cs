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

    }
}
