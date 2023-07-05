using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;



namespace MusoniRequest
{


    public class MusoniClientsRequest
    {
        string DomainNames = "https://api.demo.irl.musoniservices.com/v1/";
        AllCredentialHolder.QuickRefHolder Ref_Client = new AllCredentialHolder.QuickRefHolder();
        ConnectionLinks.LinkDetails LD_Connection = new ConnectionLinks.LinkDetails();



        internal async Task<ModelView.SannyResponseMV.SearchDetailsResponse> GetClientDetails(string Phonenumber)
        {

            ModelView.SannyResponseMV.SearchDetailsResponse MusoniClient = new ModelView.SannyResponseMV.SearchDetailsResponse();
            ModelView.SannyResponseMV.SearchDetailsResponse DetailsResponse = null;
            HttpClient CD_Client = new HttpClient();

            try
            {
                //Musoni Authentication Details
                string AuthDetailsHolder = await Task.Run(() => LD_Connection.MosoniAuthResponse());

                //Get basic auth 
                string BasicAuthResponse = await Task.Run(() => Ref_Client.BaseEncryption(AuthDetailsHolder));

                string ClientDetailsUrl = DomainNames + "search?query=" + Phonenumber;

                CD_Client.DefaultRequestHeaders.Add("x-api-key", "ohZwDcukh07kQmmshh3b73aezkbvpHa03mmwKZnR");
                CD_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                CD_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);


                var ClientResponse = await Task.Run(() => CD_Client.GetAsync(ClientDetailsUrl));
                var ClientContent = ClientResponse.Content.ReadAsStringAsync().Result;

                var ResponseClientHolder = JsonConvert.DeserializeObject<ModelView.SannyResponseMV.SearchResponse>(ClientContent);

                //Filter transaction By ClientIdentify

                string entity_Type = ResponseClientHolder.entityStatus.ToString();

                if (entity_Type == "CLIENTIDENTIFIER")
                {
                    DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                    {
                        statusCode = "OT001",
                        notification = "Transaction found",
                        holder = new ModelView.SannyResponseMV.DetailsHeld()
                        {
                            entityId = ResponseClientHolder.entityId,
                            entityName = ResponseClientHolder.entityName,
                            parentAccountNo = ResponseClientHolder.parentAccountNo,
                            parentId = ResponseClientHolder.parentId,
                            parentName = ResponseClientHolder.parentName
                        }
                    };
                }
                else
                {
                    DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                    {
                        statusCode = "OT002",
                        notification = "Transaction not found"
                    };
                }
            }
            catch (Exception Ex)
            {
                DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return DetailsResponse;



        }


        internal async Task<string> GetClientAccountBalance(string ParentId)
        {
            HttpClient CD_Client = new HttpClient();

            try
            {
                //Musoni Authentication Details
                string AuthDetailsHolder = await Task.Run(() => LD_Connection.MosoniAuthResponse());

                //Get basic auth 
                string BasicAuthResponse = await Task.Run(() => Ref_Client.BaseEncryption(AuthDetailsHolder));

                string ClientDetailsUrl = DomainNames + "clients/" + ParentId + "/accounts?fields=loanAccounts";

                CD_Client.DefaultRequestHeaders.Add("x-api-key", "ohZwDcukh07kQmmshh3b73aezkbvpHa03mmwKZnR");
                CD_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                CD_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);

                var ClientBalanceResponse = await Task.Run(() => CD_Client.GetAsync(ClientDetailsUrl));
                var ClientBalanceContent = ClientBalanceResponse.Content.ReadAsStringAsync().Result;

                var ResponseClientBalanceHolder = JsonConvert.DeserializeObject<ModelView.SannyResponseMV.ClientBalanceloanAccounts>(ClientBalanceContent);

                if (ClientBalanceResponse.StatusCode == HttpStatusCode.OK)
                {
                    int RecordCount = ResponseClientBalanceHolder.loanAccounts.Count;
                    for (int i = 0; i <= RecordCount - 1; i++)
                    {
                        //Check Client Status
                        bool Active_Status = ResponseClientBalanceHolder.loanAccounts[i].status.active;
                        if(Active_Status == true)
                        {

                        }




                    }




                }
                else
                {



                }


                //Filter transaction By ClientIdentify

                //string entity_Type = ResponseClientHolder.entityStatus.ToString();

                //if (entity_Type == "CLIENTIDENTIFIER")
                //{
                //    DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                //    {
                //        statusCode = "OT001",
                //        notification = "Transaction found",
                //        holder = new ModelView.SannyResponseMV.DetailsHeld()
                //        {
                //            entityId = ResponseClientHolder.entityId,
                //            entityName = ResponseClientHolder.entityName,
                //            parentAccountNo = ResponseClientHolder.parentAccountNo,
                //            parentId = ResponseClientHolder.parentId,
                //            parentName = ResponseClientHolder.parentName
                //        }
                //    };
                //}
                //else
                //{
                //    DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                //    {
                //        statusCode = "OT002",
                //        notification = "Transaction not found"
                //    };
                //}
            }
            catch (Exception Ex)
            {
                //DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                //{
                //    statusCode = "OT099",
                //    notification = "System error, contact system administrator"
                //};
            }

            return "";

        }
    }
}