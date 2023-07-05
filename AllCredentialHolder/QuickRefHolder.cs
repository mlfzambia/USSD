using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllCredentialHolder
{
    public class QuickRefHolder
    {

        public string UuidGenerented()
        {
            //Generate Uuid string
            Guid UG_Uuid_Holder = Guid.NewGuid();
            string UuidResponse = UG_Uuid_Holder.ToString();
            return UuidResponse;

        }


        //public string NetworkProvider(string CellNumber)
        //{



        //}



        public string BaseEncryption(string UserAuthDetails)
        {
            string ResponseDetails;
            byte[] _bytes = Encoding.ASCII.GetBytes(UserAuthDetails);
            ResponseDetails = Convert.ToBase64String(_bytes);
            return ResponseDetails;


        }

    }
}
