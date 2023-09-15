using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using System.Net.Sockets;

namespace MusoniRequest
{
    public class MusoniClientsRequest
    {
       // string DomainNames = "https://api.live.irl.musoniservices.com/v1/";
        AllCredentialHolder.QuickRefHolder Ref_Client = new AllCredentialHolder.QuickRefHolder();
        ConnectionLinks.LinkDetails LD_Connection = new ConnectionLinks.LinkDetails();
        MtnPaymentProcessing.MTNPayments MTNPayClient = new MtnPaymentProcessing.MTNPayments();


        MusoniCredentials MusoniClients = new MusoniCredentials();






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

                string ClientDetailsUrl = MusoniClients.MusoniUrl + "search?query=" + Phonenumber + "&exactMatch=true";

                CD_Client.DefaultRequestHeaders.Add("X-Api-Key", MusoniClients.MusoniAPIKey);
                CD_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                CD_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);


                var ClientResponse = await Task.Run(() => CD_Client.GetAsync(ClientDetailsUrl));

                if (ClientResponse.StatusCode == HttpStatusCode.OK)
                {
                    var ClientContent = ClientResponse.Content.ReadAsStringAsync().Result;

                    var ResponseClientHolder = JsonConvert.DeserializeObject<List<ModelView.SannyResponseMV.SearchResponse>>(ClientContent);

                    //Filter transaction By ClientIdentify
                    int clientsCount = ResponseClientHolder.Count();

                    if (clientsCount <= 0)
                    {
                        DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                        {
                            statusCode = "OT002",
                            notification = "No client found",
                        };
                    }
                    else
                    {
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



                }
                else
                {
                    DetailsResponse = new ModelView.SannyResponseMV.SearchDetailsResponse()
                    {
                        statusCode = "OT002",
                        notification = "No transaction found",
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

                string ClientDetailsUrl = MusoniClients.MusoniUrl + "clients/" + ParentId + "/accounts?fields=loanAccounts";

                CD_Client.DefaultRequestHeaders.Add("x-api-key", MusoniClients.MusoniAPIKey);
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

                        //Only add Business Account
                        bool Active_Status = ResponseClientBalanceHolder.loanAccounts[i].status.active;

                        if (Active_Status == true && ShortProductId.ToUpper() == "SDL" )
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

                        //Only Add Tilime Accounts
                        if (Active_Status == true &&  ShortProductId.ToUpper() == "TIL")
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

        //Loan Posting 

        internal async Task<ModelView.SannyResponseMV.loanPendingApprovalResponse> PostRepaymentMusoni(decimal PaymentAmount, string receiptNumber, string AccountNum,
            string ClientloanId)
        {
            HttpClient PR_Client = new HttpClient();
            ModelView.SannyResponseMV.loanPendingApprovalResponse PendingApprovalClient = null;

            try
            {
                DateTime _tranxDate = DateTime.Now;
                string TransactDate = Convert.ToDateTime(_tranxDate).ToString("dd MMMM yyyy");

                ModelView.SannyResponseMV.LoanRepaymentDetails PaymentDetails = null;

                PaymentDetails = new ModelView.SannyResponseMV.LoanRepaymentDetails()
                {
                    accountNumber = AccountNum,
                    bankNumber = "",
                    checkNumber = "",
                    dateFormat = "dd MMMM yyyy",
                    locale = "en",
                    note = "Check payment",
                    paymentTypeId = MusoniClients.MusonipaymentTypeId,
                    receiptNumber = receiptNumber,
                    routingCode = "",
                    transactionAmount = PaymentAmount,
                    transactionDate = TransactDate
                };

                //Musoni Authentication Details
                string AuthDetailsHolder = await Task.Run(() => LD_Connection.MosoniAuthResponse());
                //Get basic auth 
                string BasicAuthResponse = await Task.Run(() => Ref_Client.BaseEncryption(AuthDetailsHolder));


                PR_Client.DefaultRequestHeaders.Add("X-Api-Key", MusoniClients.MusoniAPIKey);
                PR_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                PR_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);

                string RepaymentDetailsURL = MusoniClients.MusoniUrl + "loans/" + ClientloanId + "/transactions?command=repayment";
                var _repaymentHolder = JsonConvert.SerializeObject(PaymentDetails);
                StringContent _BuilderContent = new StringContent(_repaymentHolder, Encoding.ASCII, "application/json");

                var RepaymentResponseDetails = await Task.Run(() => PR_Client.PostAsync(RepaymentDetailsURL, _BuilderContent));

                if (RepaymentResponseDetails.StatusCode == HttpStatusCode.OK)
                {
                    var RepaymentResponseContents = RepaymentResponseDetails.Content.ReadAsStringAsync().Result;
                    var PendingApprovalDeser = JsonConvert.DeserializeObject<ModelView.SannyResponseMV.LoanPendingApproval>(RepaymentResponseContents);

                    PendingApprovalClient = new ModelView.SannyResponseMV.loanPendingApprovalResponse()
                    {
                        statusCode = "OT001",
                        notification = "Post completed, Pending authorization",
                        approvalStatus = new ModelView.SannyResponseMV.LoanPendingApproval()
                        {
                            commandId = PendingApprovalDeser.commandId,
                            resourceId = PendingApprovalDeser.resourceId,
                            rollbackTransaction = PendingApprovalDeser.rollbackTransaction
                        }
                    };
                }
                else
                {
                    PendingApprovalClient = new ModelView.SannyResponseMV.loanPendingApprovalResponse()
                    {
                        statusCode = "OT002",
                        notification = "Update failed",
                    };
                }
            }
            catch (Exception Ex)
            {
                PendingApprovalClient = new ModelView.SannyResponseMV.loanPendingApprovalResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator",
                };
            }
            return PendingApprovalClient;
        }


        //Payment Authorization From Musoni

        internal async Task<ModelView.GenralResponseMV.AllGenralResponse> AuthorizaPostRepaymentMusoni(string CommandId)
        {

            HttpClient PR_Client = new HttpClient();
            ModelView.GenralResponseMV.AllGenralResponse PendingApprovalClient = null;

            try
            {
                ModelView.SannyResponseMV.LoanRepaymentDetails PaymentDetails = null;

                //Musoni Authentication Details
                string AuthDetailsHolder = await Task.Run(() => LD_Connection.MosoniAuthResponse());
                //Get basic auth 
                string BasicAuthResponse = await Task.Run(() => Ref_Client.BaseEncryption(AuthDetailsHolder));

                PR_Client.DefaultRequestHeaders.Add("X-Api-Key", MusoniClients.MusoniAPIKey);
                PR_Client.DefaultRequestHeaders.Add("X-Fineract-Platform-Tenantid", "mlfzambia");
                PR_Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuthResponse);

                string AuthRepaymentDetailsURL = MusoniClients.MusoniUrl + "makercheckers/" + CommandId + "?command=approve";
                var _repaymentHolder = JsonConvert.SerializeObject(PaymentDetails);
                StringContent _BuilderContent = new StringContent(_repaymentHolder, Encoding.ASCII, "application/json");

                var RepaymentResponseDetails = await Task.Run(() => PR_Client.PostAsync(AuthRepaymentDetailsURL, _BuilderContent));

                if (RepaymentResponseDetails.StatusCode == HttpStatusCode.OK)
                {
                var ResponseDetails=    RepaymentResponseDetails.Content.ReadAsStringAsync().Result;

                    PendingApprovalClient = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Approval Completed"
                    };
                }
                else
                {
                    PendingApprovalClient = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Approval failed"
                    };
                }
            }
            catch (Exception Ex)
            {
                PendingApprovalClient = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "Approval failed"
                };
            }
            return PendingApprovalClient;


        }

    }
}