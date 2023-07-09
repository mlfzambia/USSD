using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace MtnPaymentProcessing
{
    public class MTNPayments
    {
        AllCredentialHolder.CreadentialHolder.MTN_Credentials MtnCredentials = new AllCredentialHolder.CreadentialHolder.MTN_Credentials();


        //Request for Token

        public async Task<ModelView.MTNModelview.TokenResponseDetails> MtnAccessToken()
        {
            ModelView.MTNModelview.TokenDetails _tdTokenHolder = new ModelView.MTNModelview.TokenDetails();
            ModelView.MTNModelview.TokenResponseDetails tr_Details = null;

            try
            {
                HttpClient ClientConnection = new HttpClient();

                string UuidKey = MtnCredentials.UserUuid;
                string Api_Key = MtnCredentials.APIKey;
                string SubcriptionKey = MtnCredentials.SubcriptionKey;


                //Create Security Mtn Token
                string Uuid_Api_Key_Combined = UuidKey + ":" + Api_Key;
                byte[] _keyBytes = Encoding.ASCII.GetBytes(Uuid_Api_Key_Combined);
                string _tokenSecurityKey = Convert.ToBase64String(_keyBytes);

                //Create Token Headers
                ClientConnection.DefaultRequestHeaders.Add("Authorization", "Basic " + _tokenSecurityKey);
                ClientConnection.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubcriptionKey);

                //Sercurity configuration
                //  System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                //| SecurityProtocolType.Tls11
                //    | SecurityProtocolType.Tls12
                //| SecurityProtocolType.Ssl3;

                // removes SSL3
                // ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
                // remove multiple protocols, SSL3 and TLS 1.0
                //ServicePointManager.SecurityProtocol &= ~(SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls);

                // add TLS 1.1
                //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11;
                // add multiple protocols, TLS 1.1 and TLS 1.2
                //ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 |SecurityProtocolType.Tls13;


                string MtnTokenRequest = MtnCredentials.MtnDomainUrl + "collection/token/";
                StringContent TokenContent = new StringContent("", Encoding.UTF8, "application/json");

                var TokenResponse = await Task.Run(() => ClientConnection.PostAsync(MtnTokenRequest, TokenContent));
                var TokentContent = TokenResponse.Content.ReadAsStringAsync().Result;
                var TokenHolder = JsonConvert.DeserializeObject<ModelView.MTNModelview.TokenDetails>(TokentContent);

                if (TokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    tr_Details = new ModelView.MTNModelview.TokenResponseDetails()
                    {
                        statusCode = "OT001",
                        notification = "Token found",
                        details = new ModelView.MTNModelview.TokenDetails()
                        {
                            access_token = TokenHolder.access_token,
                            expires_in = TokenHolder.expires_in,
                            token_type = TokenHolder.token_type
                        },
                    };
                }
                else
                {
                    tr_Details = new ModelView.MTNModelview.TokenResponseDetails()
                    {
                        statusCode = "OT002",
                        notification = "Token not generated."
                    };
                }
            }
            catch (Exception Ex)
            {
                tr_Details = new ModelView.MTNModelview.TokenResponseDetails()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return tr_Details;
        }


        //Get Names of the Client number

        public async Task<ModelView.MTNModelview.ClientMsisdn> MtnClientNameDetails(string ClientMsisdn)
        {
            ModelView.MTNModelview.ClientMsisdn CMMsisdnClient = null;


            try
            {
                HttpClient CND_App_Client = new HttpClient();
                string CND_Client_Details_Url = MtnCredentials.MtnDomainUrl + "collection/v1_0/accountholder/msisdn/" + ClientMsisdn + "/basicuserinfo";

                //Get Client Token
                var Auth_Token_Holder = MtnAccessToken();

                var MTNTokenDetails = await Task.Run(() => Auth_Token_Holder);

                string StatusCode = MTNTokenDetails.statusCode;
                string Notification = MTNTokenDetails.notification;

                if (StatusCode == "OT001")
                {
                    string Auth_Token = MTNTokenDetails.details.access_token;
                    string Sub_Key = MtnCredentials.SubcriptionKey;
                    string TargetEnv = MtnCredentials.Production;

                    CND_App_Client.DefaultRequestHeaders.Add("Authorization", "Basic " + Auth_Token);
                    CND_App_Client.DefaultRequestHeaders.Add("X-Target-Environment", TargetEnv);
                    CND_App_Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Sub_Key);

                    var ClientResponseDetails = await Task.Run(() => CND_App_Client.GetAsync(CND_Client_Details_Url));

                    if (ClientResponseDetails.StatusCode == HttpStatusCode.OK)
                    {
                        var ClientInformation = ClientResponseDetails.Content.ReadAsStringAsync().Result;
                        var ClientInformationDeser = JsonConvert.DeserializeObject<ModelView.MTNModelview.ClientMsisdnInformation>(ClientInformation);

                        CMMsisdnClient = new ModelView.MTNModelview.ClientMsisdn()
                        {
                            statusCode = "OT001",
                            notification = "Client information found",
                            information = new ModelView.MTNModelview.ClientMsisdnInformation()
                            {
                                birthdate = ClientInformationDeser.birthdate,
                                family_name = ClientInformationDeser.family_name,
                                gender = ClientInformationDeser.gender,
                                given_name = ClientInformationDeser.given_name,
                                locale = ClientInformationDeser.locale,
                                status = ClientInformationDeser.status
                            }
                        };
                    }
                    else
                    {
                        CMMsisdnClient = new ModelView.MTNModelview.ClientMsisdn()
                        {
                            statusCode = "OT002",
                            notification = "Client details not found"
                        };
                    }
                }
                else
                {
                    //Mtn auth token failed
                    CMMsisdnClient = new ModelView.MTNModelview.ClientMsisdn()
                    {
                        statusCode = "OT002",
                        notification = "Client details not found"
                    };
                }
            }
            catch (Exception Ex)
            {
                CMMsisdnClient = new ModelView.MTNModelview.ClientMsisdn()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return CMMsisdnClient;
        }



        //MTN Request to pay transaction
        public async Task<ModelView.GenralResponseMV.AnySingelAllGenralResponse> Request_To_Pay(decimal amount, string narration, string phonenumber,string TransactionUuid)
        {
            ModelView.GenralResponseMV.AnySingelAllGenralResponse General_Response_client = null;

            AllCredentialHolder.CreadentialHolder.MTN_Credentials _credentialsHolder = new AllCredentialHolder.CreadentialHolder.MTN_Credentials();
            ModelView.MTNModelview.RequestToPay RequestPaymentClient = null;
            AllCredentialHolder.QuickRefHolder Ref_Client = new AllCredentialHolder.QuickRefHolder();

            try
            {
                HttpClient HP_Client = new HttpClient();
                string PaymentRequestQuery = MtnCredentials.MtnDomainUrl + "collection/v1_0/requesttopay";

                //Get Mtn Token
                var MTNTonkenDetails = await Task.Run(() => MtnAccessToken());
                string StatusCode = MTNTonkenDetails.statusCode;
                string Notification = MTNTonkenDetails.notification;

                if (StatusCode == "OT001")
                {
                    string Aut_Keys_Holder = MTNTonkenDetails.details.access_token;

                    string Ref_Id = _credentialsHolder.UserUuid;
                    string Target_Env = _credentialsHolder.Production;
                    string Subcription = _credentialsHolder.SubcriptionKey;
                    string Currency = _credentialsHolder.Currency;

                    //Generate Uuid
                    //string TransactionUuid = await Task.Run(() => Ref_Client.UuidGenerented());

                    HP_Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Aut_Keys_Holder);
                    HP_Client.DefaultRequestHeaders.Add("X-Reference-Id", TransactionUuid);
                    HP_Client.DefaultRequestHeaders.Add("X-Target-Environment", Target_Env);
                    HP_Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Subcription);

                    ModelView.MTNModelview.RequestToPay PaymentRequest = new ModelView.MTNModelview.RequestToPay()
                    {
                        amount = amount.ToString(),
                        currency = Currency,
                        externalId = TransactionUuid,
                        payeeNote = narration,
                        payer = new ModelView.MTNModelview.Payerdetails()
                        {
                            partyId = phonenumber,
                            partyIdType = "MSISDN"
                        },
                        payerMessage = narration
                    };
                    var SerPaymentRequest = JsonConvert.SerializeObject(PaymentRequest);
                    StringContent _mtnRequesttoPayBody = new StringContent(SerPaymentRequest, Encoding.UTF8, "application/json");
                    var RequestPayloadResponse = await Task.Run(() => HP_Client.PostAsync(PaymentRequestQuery, _mtnRequesttoPayBody));

                    if (RequestPayloadResponse.StatusCode == HttpStatusCode.Accepted)
                    {
                        var RequestPayContent = RequestPayloadResponse.Content.ReadAsStringAsync().Result;
                        var PayloadDesrContent = JsonConvert.DeserializeObject<ModelView.GenralResponseMV.AnySingelAllGenralResponse>(RequestPayContent);

                        General_Response_client = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Transaction registered",
                              responseValue = TransactionUuid
                        };
                    }
                    else
                    {
                        General_Response_client = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Transaction registration failed"
                        };
                    }
                }
                else
                {
                    General_Response_client = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Token generation failed"
                    };
                }

            }
            catch (Exception Ex)
            {
                General_Response_client = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return General_Response_client;
        }


        //MTN Verify the transaction is comlpeted
        public async Task<ModelView.MTNModelview.MtnPaymentVerificationResponse> Request_To_Pay_Transaction_Status(string Trx_Ref_Id)
        {
            ModelView.MTNModelview.MtnPaymentVerificationResponse Mtn_Verification_Response = null;

            try
            {
                HttpClient RP_Client = new HttpClient();

                var Auth_token_Holder = await Task.Run(() => MtnAccessToken());
                string StatusCode = Auth_token_Holder.statusCode;
                string notification = Auth_token_Holder.notification;

                if (StatusCode == "OT001")
                {
                    string Auth_token = Auth_token_Holder.details.access_token;
                    string Target_env = MtnCredentials.Production;
                    string Subcription = MtnCredentials.SubcriptionKey;

                    string Transaction_Verification_Url = MtnCredentials.MtnDomainUrl + "collection/v1_0/requesttopay/" + Trx_Ref_Id;
                    RP_Client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Auth_token);
                    RP_Client.DefaultRequestHeaders.Add("X-Target-Environment", Target_env);
                    RP_Client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Subcription);

                    var VerificationResponse = await Task.Run(() => RP_Client.GetAsync(Transaction_Verification_Url));
                    var VerificationContent = VerificationResponse.Content.ReadAsStringAsync().Result;
                    var PaymentVerificationDetails = JsonConvert.DeserializeObject<ModelView.MTNModelview.Mtn_Payment_Verification>(VerificationContent);

                    if (VerificationResponse.StatusCode == HttpStatusCode.OK)
                    {
                        //Check payment status

                        Mtn_Verification_Response = new ModelView.MTNModelview.MtnPaymentVerificationResponse()
                        {
                            statusCode = "OT001",
                            notification = "Payment response",
                            verification = new ModelView.MTNModelview.Mtn_Payment_Verification()
                            {
                                amount = PaymentVerificationDetails.amount,
                                currency = PaymentVerificationDetails.currency,
                                financialTransactionId = PaymentVerificationDetails.financialTransactionId,
                                externalId = PaymentVerificationDetails.externalId,
                                payer = new ModelView.MTNModelview.clientNumberdetails()
                                {
                                    partyIdType = PaymentVerificationDetails.payer.partyIdType,
                                    partyId = PaymentVerificationDetails.payer.partyId
                                },
                                status = PaymentVerificationDetails.status,
                                //reason = new ModelView.MTNModelview.Reasondetails()
                                //{
                                //    code = PaymentVerificationDetails.reason.code,
                                //    message = PaymentVerificationDetails.reason.message
                                //}
                            }
                        };
                    }
                    else
                    {
                        Mtn_Verification_Response = new ModelView.MTNModelview.MtnPaymentVerificationResponse()
                        {
                            statusCode = "OT002",
                            notification = "Verification failed",
                            verification = new ModelView.MTNModelview.Mtn_Payment_Verification()
                            {
                                status = PaymentVerificationDetails.status
                            }
                        };
                    }
                }
                else
                {
                    Mtn_Verification_Response = new ModelView.MTNModelview.MtnPaymentVerificationResponse()
                    {
                        statusCode = "OT002",
                        notification = "Token verification failed",
                    };

                }
            }
            catch (Exception Ex)
            {
                Mtn_Verification_Response = new ModelView.MTNModelview.MtnPaymentVerificationResponse()
                {
                    statusCode = "OT099",
                    notification = "Token verification failed",
                };
            }

            return Mtn_Verification_Response;
        }



    }
}