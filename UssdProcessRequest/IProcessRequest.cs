using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModelView.GenralResponseMV;

namespace UssdProcessRequest
{
   public interface IProcessRequest
    {
        //Ussd Processing Stage
        Task<AnySingelAllGenralResponse> UssdRequestDetails(int IsNewRequest, string SessionId, string MsisdnNumber, string ClientInput);

    }
}
