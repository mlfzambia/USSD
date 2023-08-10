using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelView
{
    public class SecurityDetailsMV
    {

        //Security Details Response

        public class CreatedSecurityDetailsResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public CreatedDetails details { get; set; }
        }

        public class CreatedDetails
        {
            public string uuidKey { get; set; }
            public string clientSubscription { get; set; }
        }


    }
}
