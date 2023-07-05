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

        public async Task<string> IGetClientAccountBalance(string Phonenumber)
        {
            var Mu_SearchClientBalance = await Task.Run(() => CR_Client.GetClientAccountBalance(Phonenumber));
            return Mu_SearchClientBalance;

        }



    }
}
