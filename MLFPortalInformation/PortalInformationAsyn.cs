using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLFPortalInformation
{
    public class PortalInformationAsyn : IPortalInformation
    {
        PortalInformation _piInformation = new PortalInformation();



        public async Task<ModelView.Portal_InforMV.TransactionResponse> ITranstionListing()
        {
            var Transaction_Response = await Task.Run(() => _piInformation.TranstionListing());
            string statusCode = Transaction_Response.statusCode;
            if (statusCode == "OT001")
            {
                return Transaction_Response;
            }
            else
            {
                return Transaction_Response;
            }

        }


     

    }
}