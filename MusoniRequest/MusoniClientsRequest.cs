using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Data.SqlClient;
using System.Data;


namespace MusoniRequest
{
    public class MusoniClientsRequest
    {
        string DomainNames = "https://api.demo.irl.musoniservices.com/v1/";
        AllCredentialHolder.QuickRefHolder Ref_Client = new AllCredentialHolder.QuickRefHolder();
        ConnectionLinks.LinkDetails LD_Connection = new ConnectionLinks.LinkDetails();
        MtnPaymentProcessing.MTNPayments MTNPayClient = new MtnPaymentProcessing.MTNPayments();

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

                CD_Client.DefaultRequestHeaders.Add("X-Api-Key", "ohZwDcukh07kQmmshh3b73aezkbvpHa03mmwKZnR");
                CD_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                CD_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);


                var ClientResponse = await Task.Run(() => CD_Client.GetAsync(ClientDetailsUrl));
                var ClientContent = ClientResponse.Content.ReadAsStringAsync().Result;

                var ResponseClientHolder = JsonConvert.DeserializeObject<List<ModelView.SannyResponseMV.SearchResponse>>(ClientContent);

                //Filter transaction By ClientIdentify
                int clientsCount = ResponseClientHolder.Count();

                for (int i = 0; i <= clientsCount - 1; i++)
                {
                    string entityStatus = ResponseClientHolder[i].entityStatus.value.ToString();
                    string clientIdentifier = ResponseClientHolder[i].entityType.ToString();

                    if (entityStatus.ToLower() == "active" && clientIdentifier == "CLIENTIDENTIFIER")
                    {
                        DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                        {
                            statusCode = "OT001",
                            notification = "Transaction found",
                            holder = new ModelView.SannyResponseMV.DetailsHeld()
                            {
                                entityId = ResponseClientHolder[i].entityId,
                                entityName = ResponseClientHolder[i].entityName,
                                parentAccountNo = ResponseClientHolder[i].parentAccountNo,
                                parentId = ResponseClientHolder[i].parentId,
                                parentName = ResponseClientHolder[i].parentName
                            }
                        };
                    }
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
       
        
        internal async Task<ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse> GetClientAccountBalance(string ParentId)
        {
            HttpClient CD_Client = new HttpClient();
            ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse detailResponse = null;

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

                //Query Musoni for client details
                var ClientBalanceResponse = await Task.Run(() => CD_Client.GetAsync(ClientDetailsUrl));
                var ClientBalanceContent = ClientBalanceResponse.Content.ReadAsStringAsync().Result;

                var ResponseClientBalanceHolder = JsonConvert.DeserializeObject<ModelView.SannyResponseMV.ClientBalanceloanAccounts>(ClientBalanceContent);

                if (ClientBalanceResponse.StatusCode == HttpStatusCode.OK)
                {
                    List<ModelView.SannyResponseMV.ClientLoanBalanceSummary> clientLoan = new List<ModelView.SannyResponseMV.ClientLoanBalanceSummary>();

                    int RecordCount = ResponseClientBalanceHolder.loanAccounts.Count;
                    for (int i = 0; i <= RecordCount - 1; i++)
                    {
                        //Get loan Product and only get first 3 values
                        string ProductNameHolder = ResponseClientBalanceHolder.loanAccounts[i].productName;
                        string ShortProductId = ProductNameHolder.Substring(0, 3);

                        //Check Client Status
                        bool Active_Status = ResponseClientBalanceHolder.loanAccounts[i].status.active;

                        if (Active_Status == true && ShortProductId.ToUpper() == "SDL" || ShortProductId.ToUpper() == "TIL")
                        {
                            clientLoan.Add(new ModelView.SannyResponseMV.ClientLoanBalanceSummary()
                            {
                                accountNo = ResponseClientBalanceHolder.loanAccounts[i].accountNo,
                                id = ResponseClientBalanceHolder.loanAccounts[i].id,
                                loanBalace = Convert.ToDecimal(ResponseClientBalanceHolder.loanAccounts[i].loanBalance),
                                productName = ShortProductId,
                                productId = ResponseClientBalanceHolder.loanAccounts[i].productId
                            });
                        }
                    }
                    detailResponse = new ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse()
                    {
                        statusCode = "OT001",
                        notification = "Loans found",
                        loanDetails = clientLoan
                    };
                }
                else
                {
                    detailResponse = new ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse()
                    {
                        statusCode = "OT002",
                        notification = "No loan(s) found",

                    };
                }


            }
            catch (Exception Ex)
            {
                detailResponse = new ModelView.SannyResponseMV.ClientLoanBalanceMainDetailResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return detailResponse;

        }

        //Complete Loan Payment
        //

     
    }
}