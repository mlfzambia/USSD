using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Transactions;
using static AllCredentialHolder.CreadentialHolder;

namespace UssdTransaction
{
    public class UssdTransaction
    {
        MtnPaymentProcessing.MTNPayments PaymentClient = new MtnPaymentProcessing.MTNPayments();
        ConnectionLinks.LinkDetails DatabaseConnection = new ConnectionLinks.LinkDetails();
        AllCredentialHolder.QuickRefHolder _quickDetailHolder = new AllCredentialHolder.QuickRefHolder();
        SmsNotification.Notification Sms_Notification_Client = new SmsNotification.Notification();
        MusoniRequest.MusoniClientsRequestAsync MosoniClient = new MusoniRequest.MusoniClientsRequestAsync();
        // ModelView.GenralResponseMV.AnySingelAllGenralResponse AllGeneralResponse = null;





        //Generate Receipt Number
        public ModelView.GenralResponseMV.AnySingelAllGenralResponse NewReceiptNumGenerator(string LoanId)
        {
            SqlConnection RNG_App_Con = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.GenralResponseMV.AnySingelAllGenralResponse AllGeneralResponse = null;

            try
            {
                //Get current Number 
                string CurrentReceiptNum = "Select receiptNumber from ReceiptNumberHolder(nolock)";
                SqlCommand CR_App_CM = new SqlCommand(CurrentReceiptNum, RNG_App_Con);
                if (RNG_App_Con.State == ConnectionState.Closed)
                {
                    RNG_App_Con.Open(); //Open Database
                }

                SqlDataAdapter CR_App_DA = new SqlDataAdapter(CR_App_CM);
                DataSet CR_App_DS = new DataSet();
                CR_App_DA.Fill(CR_App_DS, "CR");

                int _crCount = CR_App_DS.Tables["CR"].Rows.Count;

                if (_crCount >= 1)
                {
                    string CurrentReceiptNumber = CR_App_DS.Tables["CR"].Rows[0]["receiptNumber"].ToString();
                    int NewReceiptNumber = Convert.ToInt32(CurrentReceiptNumber) + 1;

                    //Update the Receipt Table
                    string UpdateCurrentReceiptNum = "Update ReceiptNumberHolder  set receiptNumber =@ReceiptNum";
                    CR_App_CM = new SqlCommand(UpdateCurrentReceiptNum, RNG_App_Con);
                    CR_App_CM.Parameters.Add("@ReceiptNum", SqlDbType.NVarChar).Value = NewReceiptNumber;

                    int _updateCount = CR_App_CM.ExecuteNonQuery();

                    if (_updateCount == 1)
                    {
                        DateTime _dtHolder = DateTime.Now;
                        int _years = _dtHolder.Year;
                        int _month = _dtHolder.Month;
                        string ResponseReceipt = "ML" + LoanId + "-" + _month + _years + "-" + NewReceiptNumber;

                        AllGeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Receipt Updated",
                            responseValue = ResponseReceipt
                        };
                    }
                    else
                    {
                        AllGeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Receipt UpdatedFailed",

                        };
                    }
                }
                else
                {
                    AllGeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "No. Receipt with that ID"
                    };
                }
            }
            catch (Exception Ex)
            {
                AllGeneralResponse = new ModelView.GenralResponseMV.AnySingelAllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator",
                };
            }
            finally
            {
                RNG_App_Con.Close();
            }
            return AllGeneralResponse;


        }

        //Transaction Registration
        internal async Task<ModelView.PaymentTransactionMV.PaymentResponse> TransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider, decimal Amount, string LoanType, string ClientLoanId, string sessionId)
        {
            SqlConnection TR_App_Conn = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.PaymentTransactionMV.PaymentResponse PaymentClient = null;

            try
            {
                //Get Client User Names
                string ClientUserNamesQuery = "Select AccountHolderName from [dbo].[UssdTransactionSession](nolock) Where ui_SessionId = @sessionId";
                SqlCommand _cnAppCm = new SqlCommand(ClientUserNamesQuery, TR_App_Conn);
                _cnAppCm.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = sessionId;

                if (TR_App_Conn.State == ConnectionState.Closed)
                {
                    TR_App_Conn.Open();//Open database connection
                }

                SqlDataAdapter _cnAppDA = new SqlDataAdapter(_cnAppCm);
                DataSet _cnAppDS = new DataSet();

                _cnAppDA.Fill(_cnAppDS, "US");

                int _cnCount = _cnAppDS.Tables["US"].Rows.Count;

                if (_cnCount == 1)
                {
                    //Client names
                    string AccountHolderName = _cnAppDS.Tables["US"].Rows[0]["AccountHolderName"].ToString();

                    //Get Transaction Uuid
                    string InternalTraxId = await Task.Run(() => _quickDetailHolder.UuidGenerented());

                    DateTime _dtHolder = DateTime.Now;
                    string CurrentDate = Convert.ToDateTime(_dtHolder).ToString("yyyy-MM-dd hh:mm:ss");

                    //Register Transaction
                    string AddPendingTransaction = "Insert Into AccountTransaction (InternalTransactionId,Clientaccount,PhoneNumber,Trx_Date,Amount,ServiceProvider,TransactionStatus,LoanType,ClientLoanId,AccountHolderName,sessionId) Values " +
                        "(@InternalTransactionId,@Clientaccount,@PhoneNumber,@Trx_Date,@Amount,@ServiceProvider,@TransactionStatus,@LoanType,@ClientLoanId,@AccountHolderName,@ui_sessionId)";
                    SqlCommand TR_App_CM = new SqlCommand(AddPendingTransaction, TR_App_Conn);
                    TR_App_CM.Parameters.Add("@InternalTransactionId", SqlDbType.NVarChar).Value = InternalTraxId;
                    TR_App_CM.Parameters.Add("@Clientaccount", SqlDbType.NVarChar).Value = Clientaccount;
                    TR_App_CM.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = PhoneNumber;
                    TR_App_CM.Parameters.Add("@Trx_Date", SqlDbType.DateTime).Value = CurrentDate;
                    TR_App_CM.Parameters.Add("@ServiceProvider", SqlDbType.NVarChar).Value = ServiceProvider;
                    TR_App_CM.Parameters.Add("@TransactionStatus", SqlDbType.Int).Value = "1";
                    TR_App_CM.Parameters.Add("@Amount", SqlDbType.Decimal).Value = Amount;
                    TR_App_CM.Parameters.Add("@LoanType", SqlDbType.NVarChar).Value = LoanType;
                    TR_App_CM.Parameters.Add("@ClientLoanId", SqlDbType.NVarChar).Value = ClientLoanId;
                    TR_App_CM.Parameters.Add("@AccountHolderName", SqlDbType.NVarChar).Value = AccountHolderName.ToUpper();
                    TR_App_CM.Parameters.Add("@ui_sessionId", SqlDbType.NVarChar).Value = sessionId;


                    int TRCount = TR_App_CM.ExecuteNonQuery();

                    if (TRCount == 1)
                    {
                        PaymentClient = new ModelView.PaymentTransactionMV.PaymentResponse()
                        {
                            statusCode = "OT001",
                            notification = "Transaction registered",
                            details = new ModelView.PaymentTransactionMV.PaymentDetails()
                            {
                                transactionId = InternalTraxId
                            },
                        };
                    }
                    else
                    {
                        PaymentClient = new ModelView.PaymentTransactionMV.PaymentResponse()
                        {
                            statusCode = "OT002",
                            notification = "Transaction registered",
                        };
                    }



                }
                else
                {




                }


            }
            catch (Exception ex)
            {
                PaymentClient = new ModelView.PaymentTransactionMV.PaymentResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator",
                };
            }
            finally
            {
                TR_App_Conn.Close();

            }


            return PaymentClient;
        }

        //Updata  Account details
        internal ModelView.GenralResponseMV.AllGenralResponse UpdateAccountNumber(string Clientaccount, string SessionId, string AccountHolderName, string PayerPhonenumber)
        {
            SqlConnection TR_App_Conn = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.GenralResponseMV.AllGenralResponse PaymentClient = null;

            try
            {
                //Register Transaction
                string AddPendingTransaction = "Update UssdTransactionSession set ui_ClientAccountNumber =@ClientAccountNumber, ClientPaymentNumber=@PaymentNumber,AccountHolderName=@accHolderName Where ui_SessionId =@sessionId";
                SqlCommand TR_App_CM = new SqlCommand(AddPendingTransaction, TR_App_Conn);
                TR_App_CM.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = SessionId;
                TR_App_CM.Parameters.Add("@ClientAccountNumber", SqlDbType.NVarChar).Value = Clientaccount;
                TR_App_CM.Parameters.Add("@PaymentNumber", SqlDbType.NVarChar).Value = PayerPhonenumber;
                TR_App_CM.Parameters.Add("@accHolderName", SqlDbType.NVarChar).Value = AccountHolderName;

                if (TR_App_Conn.State == ConnectionState.Closed)
                {
                    TR_App_Conn.Open();//Open database connection
                }

                int TRCount = TR_App_CM.ExecuteNonQuery();

                if (TRCount == 1)
                {
                    PaymentClient = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Account update completed"
                    };
                }
                else
                {
                    PaymentClient = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Transaction registered",
                    };
                }
            }
            catch (Exception)
            {
                PaymentClient = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator",
                };
            }
            finally
            {
                TR_App_Conn.Close();
            }
            return PaymentClient;
        }

        //Update the client payments 
        internal ModelView.GenralResponseMV.AllGenralResponse AddSessionBalance(string SessionId, string ProductId, string ProductName, decimal Amount, int ClientOptionId, string ClientLoanId)
        {
            ModelView.GenralResponseMV.AllGenralResponse grGeneralResponse = null;

            SqlConnection AB_App_Con = new SqlConnection(DatabaseConnection.localConnectionDatabase());

            try
            {
                string ab_AddedSessionBalance = "Insert UssdSessionBalanceHolder(tbSessionId,tbLoanName,tbAmount,tbProductId,tbClientOptionID,tbClientLoanId) Values (@SessionId,@LoanName,@Amount,@ProductId,@ClientOptionId,@ClientLoanId)";
                SqlCommand ab_App_CM = new SqlCommand(ab_AddedSessionBalance, AB_App_Con);
                ab_App_CM.Parameters.Add("@SessionId", SqlDbType.NVarChar).Value = SessionId;
                ab_App_CM.Parameters.Add("@LoanName", SqlDbType.NVarChar).Value = ProductName;
                ab_App_CM.Parameters.Add("@Amount", SqlDbType.NVarChar).Value = Amount;
                ab_App_CM.Parameters.Add("@ProductId", SqlDbType.NVarChar).Value = ProductId;
                ab_App_CM.Parameters.Add("@ClientOptionId", SqlDbType.Int).Value = ClientOptionId;
                ab_App_CM.Parameters.Add("@ClientLoanId", SqlDbType.Int).Value = ClientLoanId;

                if (AB_App_Con.State == ConnectionState.Closed)
                {
                    AB_App_Con.Open(); // Open Database connection
                }

                int abCount = ab_App_CM.ExecuteNonQuery(); //Excute update

                if (abCount >= 1)
                {
                    grGeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Session details added"
                    };
                }
                else
                {
                    grGeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Session details failed"
                    };
                }
            }
            catch (Exception Ex)
            {
                grGeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error conect system administrator"
                };
            }
            finally
            {
                AB_App_Con.Close();
            }

            return grGeneralResponse;
        }

        //Update transaction table with the status of the transaction
        internal ModelView.GenralResponseMV.AllGenralResponse UpdatePaymentTransaction(string InternalTrxId, string Ext_Tranx_Id, int TranStatus, string ReceiptNum, string ResponseReason, string MusoniResponses)
        {
            ModelView.GenralResponseMV.AllGenralResponse GeneralResponse = null;
            SqlConnection UPT_App_Con = new SqlConnection(DatabaseConnection.localConnectionDatabase());

            try
            {
                string UPT_Payment_Trx_Query = "Update [AccountTransaction] set Ext_TransactionId = @ExTransId, TransactionStatus=@TranStatus,ReceiptNum=@ReceiptNum, ErrorResponse=@ErrorResponse, MosoniResponseReason=@MosoniResponseReason Where InternalTransactionId = @IntTrxId";
                SqlCommand UPT_App_CM = new SqlCommand(UPT_Payment_Trx_Query, UPT_App_Con);
                UPT_App_CM.Parameters.Add("@ExTransId", SqlDbType.NVarChar).Value = Ext_Tranx_Id;
                UPT_App_CM.Parameters.Add("@TranStatus", SqlDbType.NVarChar).Value = TranStatus;
                UPT_App_CM.Parameters.Add("@IntTrxId", SqlDbType.NVarChar).Value = InternalTrxId;
                UPT_App_CM.Parameters.Add("@ReceiptNum", SqlDbType.NVarChar).Value = ReceiptNum;
                UPT_App_CM.Parameters.Add("@ErrorResponse", SqlDbType.NVarChar).Value = ResponseReason;
                UPT_App_CM.Parameters.Add("@MosoniResponseReason", SqlDbType.NVarChar).Value = MusoniResponses;


                if (UPT_App_Con.State == ConnectionState.Closed)
                {
                    UPT_App_Con.Open(); //Open database
                }

                int _uptCount = UPT_App_CM.ExecuteNonQuery(); //Update client
                                                              //
                if (_uptCount == 1)
                {
                    GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Update completed"
                    };
                }
                else
                {
                    GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Update failed"
                    };
                }
            }
            catch (Exception Ex)
            {
                GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }
            finally { UPT_App_Con.Close(); }

            return GeneralResponse;
        }

        //Complete Loan repayment
        internal async Task<ModelView.GenralResponseMV.AllGenralResponse> CompleteLoanPayment(string SessionId, decimal PaidLoanAmount)
        {
            SqlConnection CLP_App_CN = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            // AllCredentialHolder.QuickRefHolder RefHolder = new AllCredentialHolder.QuickRefHolder();

            ModelView.GenralResponseMV.AllGenralResponse _allResponse = null;

            try
            {
                //Get current selected option
                string GetCurrentDetailsQuery = "Select uiClientOptionSeleccted from [UssdTransactionSession] Where ui_sessionId = @sessionId";
                SqlCommand gcd_App_CM = new SqlCommand(GetCurrentDetailsQuery, CLP_App_CN);
                gcd_App_CM.Parameters.Add("@sessionid", SqlDbType.NVarChar).Value = SessionId;


                if (CLP_App_CN.State == ConnectionState.Closed)
                {
                    CLP_App_CN.Open();
                }

                SqlDataAdapter gcd_App_DA = new SqlDataAdapter(gcd_App_CM);
                DataSet gcd_APp_DS = new DataSet();
                gcd_App_DA.Fill(gcd_APp_DS, "gcd");

                int gcdCount = gcd_APp_DS.Tables["gcd"].Rows.Count;

                if (gcdCount == 1)
                {
                    //Selected Option
                    string SelectionLoanOption = gcd_APp_DS.Tables["gcd"].Rows[0]["uiClientOptionSeleccted"].ToString();

                    //Get the Loan Type Id

                    string LoanTypeIdQuery = "Select tbClientLoanId,tbProductid,tbAmount from [dbo].[UssdSessionBalanceHolder]  where tbsessionId = @sessionID and tbClientOptionId = @selectedOption";
                    SqlCommand _ltq_App_CM = new SqlCommand(LoanTypeIdQuery, CLP_App_CN);
                    _ltq_App_CM.Parameters.Add("@sessionID", SqlDbType.NVarChar).Value = SessionId;
                    _ltq_App_CM.Parameters.Add("@selectedOption", SqlDbType.NVarChar).Value = SelectionLoanOption;

                    SqlDataAdapter _ltq_App_DA = new SqlDataAdapter(_ltq_App_CM);
                    DataSet _itq_App_DS = new DataSet();
                    _ltq_App_DA.Fill(_itq_App_DS, "LTQ");

                    //Details Holder
                    string ClientLoanIdHolder = _itq_App_DS.Tables["LTQ"].Rows[0]["tbClientLoanId"].ToString();
                    string ProductLoanId = _itq_App_DS.Tables["LTQ"].Rows[0]["tbProductid"].ToString();
                    decimal ClientTotalLoan = Convert.ToDecimal(_itq_App_DS.Tables["LTQ"].Rows[0]["tbAmount"]);


                    //Search for client details 
                    string Client_details_Query = "Select top(1) uts.ui_ClientAccountNumber,uts.uiClientOptionSeleccted,uts.ClientPaymentNumber,uts.AccountHolderName,uts.uiClientOptionSeleccted  from " +
                        "[UssdTransactionSession](nolock) as UTS inner join [UssdSessionBalanceHolder](nolock) as SBH On   UTS.uiClientOptionSeleccted = SBH.tbclientOptionId Where ui_SessionId = @sessionId";
                    SqlCommand CLP_App_CM = new SqlCommand(Client_details_Query, CLP_App_CN);
                    CLP_App_CM.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = SessionId;

                    SqlDataAdapter CLP_App_DA = new SqlDataAdapter(CLP_App_CM);
                    DataSet CLP_App_DS = new DataSet();
                    CLP_App_DA.Fill(CLP_App_DS, "CLP");

                    int CLPCount = CLP_App_DS.Tables["CLP"].Rows.Count;

                    if (CLPCount >= 1)
                    {
                        //Complete Payment
                        string ToPayPhoneNumber = CLP_App_DS.Tables["CLP"].Rows[0]["ClientPaymentNumber"].ToString();
                        string ClientOptionSeleccted = CLP_App_DS.Tables["CLP"].Rows[0]["uiClientOptionSeleccted"].ToString();
                        //string LoanName = CLP_App_DS.Tables["CLP"].Rows[0]["tbLoanName"].ToString();
                        // decimal Amount = Convert.ToDecimal(CLP_App_DS.Tables["CLP"].Rows[0]["PaymentAmount"].ToString());
                        string ClientLoanId = ClientLoanIdHolder;
                        string ClientAccountNum = CLP_App_DS.Tables["CLP"].Rows[0]["ui_ClientAccountNumber"].ToString();
                        string ProductId = ProductLoanId;
                        string AccountHolderName = CLP_App_DS.Tables["CLP"].Rows[0]["AccountHolderName"].ToString();
                        string optionSelected = CLP_App_DS.Tables["CLP"].Rows[0]["uiClientOptionSeleccted"].ToString();

                        //Verifing the client number   

                        var ClientNumberVerification = await Task.Run(() => _quickDetailHolder.ClientNumber(ToPayPhoneNumber));
                        string numStatusCode = ClientNumberVerification.statusCode;
                        string numNotification = ClientNumberVerification.notification;

                        if (numStatusCode == "OT001")
                        {
                            //Get Service provider number
                            int ServiceProviderId = Convert.ToInt16(ClientNumberVerification.responseValue);

                            //Register the current tansaction before sending for payment 
                            var RegistrationTransactionResponse = await Task.Run(() => TransactionRegistration(ClientAccountNum, ToPayPhoneNumber, ServiceProviderId, PaidLoanAmount, ProductId, ClientLoanId, SessionId));
                            string rtStatusCode = RegistrationTransactionResponse.statusCode;
                            string rtNotification = RegistrationTransactionResponse.notification;

                            if (rtStatusCode == "OT001")
                            {
                                string MusoniAmountHolder = "Select usb.tbAmount From UssdTransactionSession(nolock) as UTS inner join [UssdSessionBalanceHolder] as USB On  UTS.ui_SessionId = usb.tbSessionId  Where USB.tbSessionId = @sessionId and USB.tbClientOptionID = @ClientOption";
                                SqlCommand ms_App_CM = new SqlCommand(MusoniAmountHolder, CLP_App_CN);
                                ms_App_CM.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = SessionId;
                                ms_App_CM.Parameters.Add("@ClientOption", SqlDbType.Int).Value = optionSelected;


                                if (CLP_App_CN.State == ConnectionState.Closed)
                                {
                                    CLP_App_CN.Open();//Open database connection
                                }

                                SqlDataAdapter _CLP_App_DA = new SqlDataAdapter(ms_App_CM);
                                DataSet _CLP_App_DS = new DataSet();
                                _CLP_App_DA.Fill(_CLP_App_DS, "_clp");
                                int _count = _CLP_App_DS.Tables["_clp"].Rows.Count;

                                if (_count == 1)
                                {
                                    //Generate new Re
                                    var GeneratedLoanReceipt = await Task.Run(() => NewReceiptNumGenerator(ClientLoanId));
                                    string ReceiptNumber = GeneratedLoanReceipt.responseValue; //Current New Receipt Number

                                    decimal TotalLoanAmount = Convert.ToDecimal(_CLP_App_DS.Tables["_clp"].Rows[0]["tbAmount"]);

                                    if (PaidLoanAmount <= ClientTotalLoan)
                                    {
                                        string TransactionUuidRegatMtn = RegistrationTransactionResponse.details.transactionId; //TRansaction Id 

                                        var PaymentQuery = await Task.Run(() => PaymentClient.Request_To_Pay(PaidLoanAmount, ProductId + " - Loan repayment", ToPayPhoneNumber, TransactionUuidRegatMtn, ReceiptNumber));
                                        string tuStatusCode = PaymentQuery.statusCode;
                                        string Notification = PaymentQuery.notification;

                                        if (tuStatusCode == "OT001")
                                        {
                                            //Transaction verification

                                            //Create a loop that will run for 5times
                                            for (int i = 0; i <= 10; i++)
                                            {
                                                await Task.Delay(5000); //Delay the payment verification by 15sec to allow the client to enter there pin

                                                var VerificationResponse = await Task.Run(() => PaymentClient.Request_To_Pay_Transaction_Status(TransactionUuidRegatMtn));
                                                string _vrStatusCode = VerificationResponse.statusCode;
                                                string _vrNotification = VerificationResponse.notification;

                                                if (_vrStatusCode == "OT001")
                                                {
                                                    StringBuilder SmsBodyHolder = new StringBuilder();//SMS holder

                                                    string trxStatus = VerificationResponse.verification.status;
                                                    string ExternalTranx = VerificationResponse.verification.financialTransactionId;

                                                    //Transaction Receipt Id
                                                    //   string receiptNumber = GeneratedLoanReceipt.responseValue;

                                                    //Mtn Message Responses
                                                    string ErrorResponse = VerificationResponse.verification.reason;

                                                    if (trxStatus.ToUpper() == "SUCCESSFUL")
                                                    {
                                                        //Completeed
                                                        var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 6, ReceiptNumber, "Completed Payment", "Pending Posting to Musoni")); //Transaction completed
                                                        string _usStatusCode = UpdateStatus.statusCode;
                                                        string _notification = UpdateStatus.notification;

                                                        if (_usStatusCode == "OT001")
                                                        {
                                                            //Post tranaction to Musoni
                                                            var _abAccountBalanceResponse = await Task.Run(() => MosoniClient.IPostAccountBalance(PaidLoanAmount, ReceiptNumber, ClientAccountNum, ClientLoanId));
                                                            string _abStatusCode = _abAccountBalanceResponse.statusCode;
                                                            string _abNotification = _abAccountBalanceResponse.notification;

                                                            if (_abStatusCode == "OT001")
                                                            {
                                                                //Adding Transaction to musoni

                                                                var MusoniUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 7, ReceiptNumber, "Completed Payment", "Transaction Added to musoni")); //Transaction completed
                                                                string mus_Status = MusoniUpdateStatus.statusCode;

                                                                if (mus_Status == "OT001")
                                                                {
                                                                    string _abCommandId = _abAccountBalanceResponse.approvalStatus.commandId;

                                                                    //Authorize Trnsaction in Musoni 
                                                                    var AuthMusoniPost = await Task.Run(() => MosoniClient.IAuthorizaPostRepaymentMusoni(_abCommandId));

                                                                    string AMStatusCode = AuthMusoniPost.statusCode;
                                                                    string AMNotification = AuthMusoniPost.notification;

                                                                    if (AMStatusCode == "OT001")
                                                                    {
                                                                        var MusoniAuthUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 4, ReceiptNumber, "Completed Payment", "Musoni approval completed")); //Transaction completed
                                                                        string musa_Status = MusoniUpdateStatus.statusCode;

                                                                        if (musa_Status == "OT001")
                                                                        {
                                                                            //New loan Balance calculation
                                                                            decimal NewBalance = ClientTotalLoan - PaidLoanAmount;

                                                                            //Send Sms notification
                                                                            SmsBodyHolder.Append("Hi, " + AccountHolderName + "\n");
                                                                            SmsBodyHolder.Append("Loan repayment of K" + PaidLoanAmount + ", has been recieved\n");
                                                                            //Add new loan balance
                                                                            SmsBodyHolder.Append("Trx No. " + ReceiptNumber + "\n");
                                                                            SmsBodyHolder.Append("Your new Loan balance now is K"+ NewBalance + "\n");
                                                                            SmsBodyHolder.Append("Thank you for the payment");
                                                                            await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));
                                                                        }

                                                                    }
                                                                    else
                                                                    {
                                                                        var MusoniAuthUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 7, ReceiptNumber, "Completed Payment", "Transaction not authorized in musoni")); //Transaction completed

                                                                    }

                                                                }
                                                            }
                                                            else
                                                            {
                                                                var MusoniAuthUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 6, ReceiptNumber, "Completed Payment", "Transaction not posted in musoni")); //Transaction completed

                                                            }
                                                        }

                                                        break;
                                                    }
                                                    else if (trxStatus.ToUpper() == "FAILED")
                                                    {
                                                        //Completeed
                                                        var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 2, ReceiptNumber, ErrorResponse, "")); //Transaction completed
                                                        string _usStatusCode = UpdateStatus.statusCode;
                                                        string _notification = UpdateStatus.notification;
                                                        if (_usStatusCode == "OT001")
                                                        {
                                                            //Send Sms notification
                                                            //Send Sms notification
                                                            SmsBodyHolder.Append("Hi, " + AccountHolderName+"\n");
                                                            SmsBodyHolder.Append("Loan repayment of K" + PaidLoanAmount + ", has failed.\n");
                                                            //SmsBodyHolder.Append("Trx No. " + ReceiptNumber + "\n");
                                                            //SmsBodyHolder.Append("Physical reciept, visit any of our offices.");
                                                            await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));
                                                        }
                                                        break;
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Get Current Client details

                                        //Subcribe to the Sms Service
                                        Sms_Notification_Client.EventNotification += Sms_Notification_Client_EventNotification;

                                        string ClientDetailsQuery = "Select ClientAccount,PhoneNumber,AccountHolderName,Amount from [dbo].[AccountTransaction] Where SESSIONID = @SessionId";
                                        SqlCommand _cdq_App_CM = new SqlCommand(ClientDetailsQuery, CLP_App_CN);
                                        _cdq_App_CM.Parameters.Add("@SessionId", SqlDbType.NVarChar).Value = SessionId;

                                        SqlDataAdapter _cdq_App_DA = new SqlDataAdapter(_cdq_App_CM);
                                        DataSet _cdq_App_DS = new DataSet();

                                        _cdq_App_DA.Fill(_cdq_App_DS, "_cdq");
                                        int _cdqCount = _cdq_App_DS.Tables["_cdq"].Rows.Count;

                                        if (_cdqCount >= 1)
                                        {
                                            string TransactionUuidRegatMtn = RegistrationTransactionResponse.details.transactionId; //TRansaction Id 

                                            string ClientsName = _cdq_App_DS.Tables["_cdq"].Rows[0]["AccountHolderName"].ToString();
                                            string Amounts = _cdq_App_DS.Tables["_cdq"].Rows[0]["Amount"].ToString();
                                            string PhoneNumber = _cdq_App_DS.Tables["_cdq"].Rows[0]["PhoneNumber"].ToString();

                                            StringBuilder SmsBodyHolder = new StringBuilder();
                                            SmsBodyHolder.Append("Hi, " + ClientsName + "\n");
                                            SmsBodyHolder.Append("You are trying to pay more then your current loan, your loan balance is K" + ClientTotalLoan + " and you tried to pay K" + Amounts + "");

                                            Sms_Notification_Client.OnSmsNotification(PhoneNumber, SmsBodyHolder.ToString());

                                            //Cancel the transaction
                                            var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, "", 2, ReceiptNumber, "Exceeding amount to be paid", "")); //Transaction completed

                                            _allResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                                            {
                                                statusCode = "OT002",
                                                notification = "Amount being is more then your loan amount"
                                            };
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    _allResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "System error."
                    };
                }
            }
            catch (Exception Ex)
            {

            }
            finally
            {
                CLP_App_CN.Close();
            }

            return _allResponse;
        }

        private async void Sms_Notification_Client_EventNotification(string PhoneNumber, string SmsBody)
        {
            await Task.Run(() => Sms_Notification_Client.SendSmsNotification(PhoneNumber, SmsBody));
        }

        //Verify if payment on any pending transaction

        public async Task<ModelView.GenralResponseMV.AllGenralResponse> ConfirmPendingTransaction()
        {

            SqlConnection CPT_App_Conn = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.GenralResponseMV.AllGenralResponse All_Responses = new ModelView.GenralResponseMV.AllGenralResponse();


            try
            {
                StringBuilder SmsBodyHolder = new StringBuilder();

                //Get all pending Transaction
                string PendingTransaction = "Select InternalTransactionId,Clientaccount,SessionId,ClientLoanId,Amount,AccountHolderName,PhoneNumber,ReceiptNum,trx_date  from [AccountTransaction](nolock) Where TransactionStatus = @transactionStatus and sessionId is not null Order by incrmid";
                SqlCommand _pt_App_CM = new SqlCommand(PendingTransaction, CPT_App_Conn);

                if (CPT_App_Conn.State == ConnectionState.Closed)
                {
                    CPT_App_Conn.Open();// Open database
                }
                _pt_App_CM.Parameters.Add("@transactionStatus", SqlDbType.Int).Value = 1; // get only pending transaction

                SqlDataAdapter _pt_App_DA = new SqlDataAdapter(_pt_App_CM);
                DataSet _pt_App_DS = new DataSet();

                _pt_App_DA.Fill(_pt_App_DS, "_PT");
                int _ptCount = _pt_App_DS.Tables["_PT"].Rows.Count;

                if (_ptCount >= 1)
                {
                    string InternalTransactionId;
                    _ptCount -= 1;

                    for (int i = 0; i <= _ptCount; i++)
                    {
                        //Client Id reclls
                        string ClientLoanId = _pt_App_DS.Tables["_PT"].Rows[i]["ClientLoanId"].ToString();

                        InternalTransactionId = _pt_App_DS.Tables["_PT"].Rows[i]["InternalTransactionId"].ToString();
                        string AccountHolderName = _pt_App_DS.Tables["_PT"].Rows[i]["AccountHolderName"].ToString();
                        string ToPayPhoneNumber = _pt_App_DS.Tables["_PT"].Rows[i]["PhoneNumber"].ToString();

                        DateTime TrxDateTime = Convert.ToDateTime(_pt_App_DS.Tables["_PT"].Rows[i]["trx_date"]);

                        //Current Date time
                        DateTime _datetimeHolder = DateTime.Now;
                        TimeSpan _CurrentTime = new TimeSpan(_datetimeHolder.Hour, _datetimeHolder.Minute, _datetimeHolder.Second);

                        //Transaction Date time
                        TimeSpan _TransactTime = new TimeSpan(TrxDateTime.Hour, TrxDateTime.Minute, TrxDateTime.Second);
                        TimeSpan CheckingTimeDiff = _CurrentTime.Subtract(_TransactTime);

                        if (CheckingTimeDiff.Minutes > 10)
                        {

                            var PaymentResponseDetails = await Task.Run(() => PaymentClient.Request_To_Pay_Transaction_Status(InternalTransactionId));

                            //Complet the posting 
                            string PaymentStatus = PaymentResponseDetails.statusCode;
                            decimal PaidLoanAmount = Convert.ToDecimal(_pt_App_DS.Tables["_PT"].Rows[i]["Amount"]);
                            string ClientAccountNum = _pt_App_DS.Tables["_PT"].Rows[i]["Clientaccount"].ToString();

                            string MtnResponseStatus = PaymentResponseDetails.verification.status;

                            //MTN Response Details
                            string MtnResponseReason = PaymentResponseDetails.verification.reason;

                            if (PaymentStatus == "OT001")
                            {
                                if (MtnResponseStatus.ToUpper() == "SUCCESSFUL")
                                {
                                    //MTN Payment details
                                    string ExternalTranx = PaymentResponseDetails.verification.financialTransactionId;
                                    string receiptNumber = PaymentResponseDetails.verification.externalId;

                                    //Completeed
                                    var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(InternalTransactionId, ExternalTranx, 6, receiptNumber, "Completed Payment", "Pending Posting to Musoni")); //Transaction completed
                                    string _usStatusCode = UpdateStatus.statusCode;
                                    string _notification = UpdateStatus.notification;

                                    if (_usStatusCode == "OT001")
                                    {
                                        //Post tranaction to Musoni
                                        var _abAccountBalanceResponse = await Task.Run(() => MosoniClient.IPostAccountBalance(PaidLoanAmount, receiptNumber, ClientAccountNum, ClientLoanId));
                                        string _abStatusCode = _abAccountBalanceResponse.statusCode;
                                        string _abNotification = _abAccountBalanceResponse.notification;

                                        if (_abStatusCode == "OT001")
                                        {

                                            //Update Transaction
                                            var MusoniUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(InternalTransactionId, ExternalTranx, 7, receiptNumber, "Completed Payment", "Transaction Added to musoni")); //Transaction completed
                                            string musStatus = MusoniUpdateStatus.statusCode;

                                            if (musStatus == "OT001")
                                            {
                                                string _abCommandId = _abAccountBalanceResponse.approvalStatus.commandId;

                                                //Authorize Trnsaction in Musoni 
                                                var AuthMusoniPost = await Task.Run(() => MosoniClient.IAuthorizaPostRepaymentMusoni(_abCommandId));

                                                string AMStatusCode = AuthMusoniPost.statusCode;
                                                string AMNotification = AuthMusoniPost.notification;

                                                if (AMStatusCode == "OT001")
                                                {
                                                    var MusoniAUpdateStatus = await Task.Run(() => UpdatePaymentTransaction(InternalTransactionId, ExternalTranx, 4, receiptNumber, "Completed Payment", "Musoni approval completed")); //Transaction completed

                                                    //Send Sms notification
                                                    SmsBodyHolder.Append("Hi, " + AccountHolderName + "\n");
                                                    SmsBodyHolder.Append("Loan repayment for K" + PaidLoanAmount + ", has been recieved\n");
                                                    //Add new loan balance
                                                    SmsBodyHolder.Append("Trx No. " + receiptNumber + "\n");
                                                    SmsBodyHolder.Append("Physical reciept, visit any of our offices.");
                                                    await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));

                                                    All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                                    {
                                                        statusCode = "OT001",
                                                        notification = "Payment verification completed"

                                                    };

                                                }
                                                else
                                                {
                                                    //Update transaction table that Musoni Auth failed to post

                                                    All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                                    {
                                                        statusCode = "OT002",
                                                        notification = "Account " + ClientAccountNum + "  was not authorized in musoni, Amount of " + PaidLoanAmount + " and receipt No. " + receiptNumber + ", contact system administrator"
                                                    };
                                                    break;

                                                }

                                            }
                                            else
                                            {
                                                All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                                {
                                                    statusCode = "OT002",
                                                    notification = "Account " + ClientAccountNum + "  was not updated, contact system administrator"
                                                };
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            //Update transaction table that Musoni failed to post
                                            //Update transaction table that Musoni Auth failed to post

                                            All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                            {
                                                statusCode = "OT002",
                                                notification = "Account " + ClientAccountNum + "  was not posted in musoni, Amount of " + PaidLoanAmount + " and receipt No. " + receiptNumber + ", contact system administrator"
                                            };
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                        {
                                            statusCode = "OT002",
                                            notification = "Account " + ClientAccountNum + " was not updated, Amount of " + PaidLoanAmount + " and receipt No. " + receiptNumber + ", contact system administrator"
                                        };
                                        break;
                                    }
                                }
                                else if (MtnResponseStatus.ToUpper() == "FAILED")
                                {
                                    //var GeneratedLoanReceipt = await Task.Run(() => NewReceiptNumGenerator(ClientLoanId));
                                    //string ReceiptNumber = GeneratedLoanReceipt.responseValue; //Current New Receipt Number

                                    //Update transaction
                                    //Completeed
                                    var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(InternalTransactionId, "", 2, "", MtnResponseReason, "")); //Transaction completed
                                    string _usStatusCode = UpdateStatus.statusCode;
                                    string _notification = UpdateStatus.notification;
                                    if (_usStatusCode == "OT001")
                                    {

                                        //Send Sms notification
                                        SmsBodyHolder.Append("Hi, " + AccountHolderName);
                                        SmsBodyHolder.Append("Loan repayment for K" + PaidLoanAmount + ", has failed.\n");

                                        SmsBodyHolder.Append("Physical reciept, visit any of our offices.");
                                        await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));

                                        All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                        {
                                            statusCode = "OT001",
                                            notification = "Payment verification completed"

                                        };
                                    }
                                    else
                                    {
                                        All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                        {
                                            statusCode = "OT002",
                                            notification = "Transaction updated failed, contact system administrator"
                                        };
                                    }
                                    //  break;
                                }
                            }
                            else
                            {
                                //  var GeneratedLoanReceipt = await Task.Run(() => NewReceiptNumGenerator(ClientLoanId));
                                // string ReceiptNumber = GeneratedLoanReceipt.responseValue; //Current New Receipt Number

                                //Update transaction
                                //Completeed
                                var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(InternalTransactionId, "", 2, "", MtnResponseReason + ".Error", "")); //Transaction completed
                                string _usStatusCode = UpdateStatus.statusCode;
                                string _notification = UpdateStatus.notification;
                                if (_usStatusCode == "OT001")
                                {
                                    //Send Sms notification
                                    //Send Sms notification
                                    SmsBodyHolder.Append("Hi, " + AccountHolderName);
                                    SmsBodyHolder.Append("Loan repayment for K" + PaidLoanAmount + ", has failed.\n");
                                    //SmsBodyHolder.Append("Trx No. " + ReceiptNumber + "\n");
                                    SmsBodyHolder.Append("Physical reciept, visit any of our offices.");

                                    await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));
                                    All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                    {
                                        statusCode = "OT001",
                                        notification = "Payment verification completed"
                                    };
                                }
                                else
                                {
                                    All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                                    {
                                        statusCode = "OT002",
                                        notification = "Current transaction " + InternalTransactionId + " for MTN request to pay failed to complete"
                                    };
                                }
                                //break;


                            }

                        }
                        else
                        {
                            //Skip the check


                        }

                        All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Payment Verification Completed"
                        };
                    }
                }
                else
                {
                    //Not transaction found
                    All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "No pending transaction found"
                    };
                }
            }
            catch (Exception Ex)
            {
                //Not transaction found
                All_Responses = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT002",
                    notification = "System error, contact system administraotor " + Ex.Message
                };
            }

            finally
            {
                CPT_App_Conn.Close(); //CLose DB connection
            }

            return All_Responses;
        }

        //Post the Transaction to musoni

        private void RepostingTransactionToMusoni()
        {
            SqlConnection _mpl_app_Con = new SqlConnection(DatabaseConnection.localConnectionDatabase());

            //decimal PaymentAmount, string receiptNumber, string AccountNum, string ClientloanId

            try
            {
                //Get all pending Loans for Musoni
                string MusoniPendingLoan = "Select Amount,ReceiptNum,Clientaccount,ClientLoanId from [dbo].[AccountTransaction] Where TransactionStatus = @tranStatusNum order by incrmid desc";
                SqlCommand _mpl_App_CM = new SqlCommand(MusoniPendingLoan, _mpl_app_Con);

                if (_mpl_app_Con.State == ConnectionState.Closed)
                {
                    _mpl_app_Con.Open();// Open Database
                }

                SqlDataAdapter _mpl_App_DA = new SqlDataAdapter(_mpl_App_CM);
                DataSet _mpl_App_DS = new DataSet();

                // _mpl_App_DA


            }
            catch (Exception)
            {

                throw;
            }






        }


    }
}