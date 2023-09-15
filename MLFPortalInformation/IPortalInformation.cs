using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLFPortalInformation
{
    public interface IPortalInformation
    {

        Task<ModelView.Portal_InforMV.TransactionResponse> ITranstionListing();

    }
}
