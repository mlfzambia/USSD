using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModelView.GenralResponseMV;

namespace UssdProcessRequest
{
    public class ProcessRequestAsync :IProcessRequest
    {
        ProcessRequest ClientRequest = new ProcessRequest();

        public async Task<AnySingelAllGenralResponse> UssdRequestDetails(int IsNewRequest, string SessionId, string MsisdnNumber, string ClientInput)
        { 
                var ResponseDetails = await Task.Run(() => ClientRequest.UssdRequestDetails(IsNewRequest, SessionId, MsisdnNumber, ClientInput));
                return ResponseDetails; 
        }

        
    }
}
