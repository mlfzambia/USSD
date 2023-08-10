using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UssdProcessRequest
{
    interface IProcessRequest
    {
        //Ussd Processing Stage
        Task<string> UssdRequestDetails(int IsNewRequest, string SessionId, string MsisdnNumber, string ClientInput);

    }
}
