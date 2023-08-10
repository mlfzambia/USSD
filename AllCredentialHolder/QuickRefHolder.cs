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

       

        public string BaseEncryption(string UserAuthDetails)
        {
            string ResponseDetails;
            byte[] _bytes = Encoding.ASCII.GetBytes(UserAuthDetails);
            ResponseDetails = Convert.ToBase64String(_bytes);
            return ResponseDetails;
        }


        //Number Verification Check

        public ModelView.GenralResponseMV.AnySingelAllGenralResponse ClientNumber(string PhoneNumber)
        {
            ModelView.GenralResponseMV.AnySingelAllGenralResponse GeneralResponse = null;

            try
            {
                //Get number lenth
                int NumberCount = PhoneNumber.Count();
                if (NumberCount == 12)
                {
                    //Check number status
                    string NumberExten = PhoneNumber.Substring(0, 5);

                    if (NumberExten == "26097")
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Network not activated",
                             responseValue="2"
                        };
                    }
                    else if (NumberExten == "26077")
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Network not activated",
                            responseValue = "2"
                        };
                    }
                    else if (NumberExten == "26096")
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Network activated",
                            responseValue = "1"
                        };
                    }
                    else if (NumberExten == "26076")
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Network activated",
                            responseValue = "1"
                        };
                    }
                    else if (NumberExten == "26095")
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Network not activated",
                            responseValue = "3"
                        };
                    }
                }
                else
                {
                    //Check number
                    GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                    {
                        statusCode = "OT003",
                        notification = "Check your number"
                    };
                }

            }
            catch (Exception Ex)
            {
                GeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return GeneralResponse;

        }


    }
}
