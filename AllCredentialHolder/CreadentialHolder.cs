namespace AllCredentialHolder
{
    public class CreadentialHolder
    {
        public class SmsDetails
        {
            public string SmsGatewayDomain = "http://102.68.138.7:8501/";

        }

        //public class MusoniLoginKeys
        //{
        //    public string APi_Key = "ohZwDcukh07kQmmshh3b73aezkbvpHa03mmwKZnR";
        //    public string Platform_Tenantid = "mlfzambia";
        //}

        public class MTN_Credentials
        {
            //#region Sanbox Credentials
            //public string UserUuid = "027563e2-949a-4a76-8486-258837a594c6";
            //public string SubcriptionKey = "61538b181ceb425fb7ce459537b5100d";
            //public string APIKey = "4d45db540c2247739ae9f1dd560f0aaa";
            //public string Production = "sandbox";
            //public string Currency = "EUR";
            //public string MtnDomainUrl = "https://sandbox.momodeveloper.mtn.com/";
            //#endregion



            #region Live MLF Credentials

            public string UserUuid = "25077d93-49fa-46a6-a3ce-765dd4dfc2da";
            public string SubcriptionKey = "2bcbdc425ebb442e9bf5086f8e486438";
            public string APIKey = "c1da800bb9524cab89cd4f91f82fe00e";
            public string Production = "mtnzambia";
            public string Currency = "ZMW";
            public string MtnDomainUrl = "https://proxy.momoapi.mtn.com/";

            #endregion

        }

        //public static string MTN_Api_User_Url = "https://proxy.momoapi.mtn.com/v1_0/apiuser";
        //public static string MTN_UUID_Number = "7844e558-446f-4255-9586-758bf0383a95";
        //public static string MTN_Ocp_Apim_Subscription_Key = "887d7f79ba434147941050a7009756fc";
        //public static string MTN_apiKey = "ea04f47feb94438fae3a12a879c3dc5f";
        //public static string MTN_Token_Url = "https://proxy.momoapi.mtn.com/collection/token/";
        //public static string MTN_Request_to_Pay = "https://proxy.momoapi.mtn.com/collection/v1_0/requesttopay";
        //public static string MTN_Request_to_Pay_Verification = "https://proxy.momoapi.mtn.com/collection/v1_0/requesttopay/";
        //public static string MTN_Production_Environment_Server = "mtnzambia";
        //public static string X_Currency = "ZMW";
        //public static string MSISDN_Client = "MSISDN";


    }
}