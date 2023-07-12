using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Transactions;

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
                        string ResponseReceipt = "ML" + LoanId + "-" + _years + "-" + NewReceiptNumber;

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
                        notification = "No. Receipt with that ID",

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

            return AllGeneralResponse;


        }

        //Transaction Registration

        internal async Task<ModelView.PaymentTransactionMV.PaymentResponse> TransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider, decimal Amount, string LoanType, string ClientLoanId)
        {
            SqlConnection TR_App_Conn = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.PaymentTransactionMV.PaymentResponse PaymentClient = null;

            try
            {
                //Get Transaction Uuid
                string InternalTraxId = await Task.Run(() => _quickDetailHolder.UuidGenerented());

                DateTime _dtHolder = DateTime.Now;
                string CurrentDate = Convert.ToDateTime(_dtHolder).ToString("yyyy-MM-dd");

                //Register Transaction
                string AddPendingTransaction = "Insert Into AccountTransaction (InternalTransactionId,Clientaccount,PhoneNumber,Trx_Date,Amount,ServiceProvider,TransactionStatus,LoanType,ClientLoanId) Values " +
                    "(@InternalTransactionId,@Clientaccount,@PhoneNumber,@Trx_Date,@Amount,@ServiceProvider,@TransactionStatus,@LoanType,@ClientLoanId)";
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

                if (TR_App_Conn.State == ConnectionState.Closed)
                {
                    TR_App_Conn.Open();//Open database connection
                }

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
            catch (Exception)
            {
                PaymentClient = new ModelView.PaymentTransactionMV.PaymentResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator",
                };
            }
            return PaymentClient;
        }

        //Updata  Account details
        internal ModelView.GenralResponseMV.AllGenralResponse UpdateAccountNumber(string Clientaccount, string SessionId, string AccountHolderName)
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
                TR_App_CM.Parameters.Add("@PaymentNumber", SqlDbType.NVarChar).Value = "26" + Clientaccount;
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

        //Updata transaction table with the status of the transaction
        internal ModelView.GenralResponseMV.AllGenralResponse UpdatePaymentTransaction(string InternalTrxId, string Ext_Tranx_Id, int TranStatus,string ReceiptNum)
        {
            ModelView.GenralResponseMV.AllGenralResponse GeneralResponse = null;

            SqlConnection UPT_App_Con = new SqlConnection(DatabaseConnection.localConnectionDatabase());

            try
            {
                string UPT_Payment_Trx_Query = "Update [AccountTransaction] set Ext_TransactionId = @ExTransId, TransactionStatus=@TranStatus,ReceiptNum=@ReceiptNum Where InternalTransactionId = @IntTrxId";
                SqlCommand UPT_App_CM = new SqlCommand(UPT_Payment_Trx_Query, UPT_App_Con);
                UPT_App_CM.Parameters.Add("@ExTransId", SqlDbType.NVarChar).Value = Ext_Tranx_Id;
                UPT_App_CM.Parameters.Add("@TranStatus", SqlDbType.NVarChar).Value = TranStatus;
                UPT_App_CM.Parameters.Add("@IntTrxId", SqlDbType.NVarChar).Value = InternalTrxId;
                UPT_App_CM.Parameters.Add("@ReceiptNum", SqlDbType.NVarChar).Value = ReceiptNum;



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


            return GeneralResponse;
        }


        //Complete Loan repayment
        internal async void CompleteLoanPayment(string SessionId, decimal PaidLoanAmount)
        {
            SqlConnection CLP_App_CN = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            // AllCredentialHolder.QuickRefHolder RefHolder = new AllCredentialHolder.QuickRefHolder();

            try
            {
                //Search for client details 
                string Client_details_Query = "Select uts.ui_ClientAccountNumber,uts.uiClientOptionSeleccted,uts.PaymentAmount,sbh.tbClientLoanId,uts.ClientPaymentNumber,sbh.tbProductId,uts.AccountHolderName,uts.uiClientOptionSeleccted,sbh.tbClientLoanId from " +
                    "[UssdTransactionSession](nolock) as UTS inner join [UssdSessionBalanceHolder](nolock) as SBH On  UTS.ui_SessionId= SBH.tbSessionId Where ui_SessionId = @sessionId";
                SqlCommand CLP_App_CM = new SqlCommand(Client_details_Query, CLP_App_CN);
                CLP_App_CM.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = SessionId;

                if (CLP_App_CN.State == ConnectionState.Closed)
                {
                    CLP_App_CN.Open();
                }

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
                    decimal Amount = Convert.ToDecimal(CLP_App_DS.Tables["CLP"].Rows[0]["PaymentAmount"].ToString());
                    string ClientLoanId = CLP_App_DS.Tables["CLP"].Rows[0]["tbClientLoanId"].ToString();
                    string ClientAccountNum = CLP_App_DS.Tables["CLP"].Rows[0]["ui_ClientAccountNumber"].ToString();
                    string ProductId = CLP_App_DS.Tables["CLP"].Rows[0]["tbProductId"].ToString();
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
                        var RegistrationTransactionResponse = await Task.Run(() => TransactionRegistration(ClientAccountNum, ToPayPhoneNumber, ServiceProviderId, PaidLoanAmount, ProductId, ClientLoanId));
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
                                decimal TotalLoanAmount = Convert.ToDecimal(_CLP_App_DS.Tables["_clp"].Rows[0]["tbAmount"]);
                                if (PaidLoanAmount <= TotalLoanAmount)
                                {
                                    string TransactionUuidRegatMtn = RegistrationTransactionResponse.details.transactionId; //TRansaction Id 

                                    var GeneratedLoanReceipt = await Task.Run(() => NewReceiptNumGenerator(ClientLoanId));

                                    string ReceiptNumber = GeneratedLoanReceipt.responseValue; //Current New Receipt Number

                                    var PaymentQuery = await Task.Run(() => PaymentClient.Request_To_Pay(PaidLoanAmount, ProductId + " - Loan repayment", ToPayPhoneNumber, TransactionUuidRegatMtn, ReceiptNumber));
                                    string tuStatusCode = PaymentQuery.statusCode;
                                    string Notification = PaymentQuery.notification;

                                    if (tuStatusCode == "OT001")
                                    {
                                        //Transaction verification
                                        await Task.Delay(13000); //Delay the payment verification by 10sec to allow the client to enter there pin

                                        //Create a loop that will run for 5times
                                        for (int i = 0; i <= 5; i++)
                                        {
                                            var VerificationResponse = await Task.Run(() => PaymentClient.Request_To_Pay_Transaction_Status(TransactionUuidRegatMtn));
                                            string _vrStatusCode = VerificationResponse.statusCode;
                                            string _vrNotification = VerificationResponse.notification;

                                            if (_vrStatusCode == "OT001")
                                            {
                                                StringBuilder SmsBodyHolder = new StringBuilder();//SMS holder

                                                string trxStatus = VerificationResponse.verification.status;
                                                string ExternalTranx = VerificationResponse.verification.financialTransactionId;

                                                //Transaction Receipt Id
                                                string receiptNumber = GeneratedLoanReceipt.responseValue;

                                                if (trxStatus.ToUpper() == "SUCCESSFUL")
                                                {
                                                    //Completeed
                                                    var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 4, receiptNumber)); //Transaction completed
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
                                                            string _abCommandId = _abAccountBalanceResponse.approvalStatus.commandId;

                                                            //Authorize Trnsaction in Musoni 
                                                            var AuthMusoniPost = await Task.Run(() => MosoniClient.IAuthorizaPostRepaymentMusoni(_abCommandId));

                                                            string AMStatusCode = AuthMusoniPost.statusCode;
                                                            string AMNotification = AuthMusoniPost.notification;

                                                            if (AMStatusCode == "OT001")
                                                            {

                                                                //Send Sms notification
                                                                SmsBodyHolder.Append("Hi, " + AccountHolderName + "\n");
                                                                SmsBodyHolder.Append("Loan repayment for K" + PaidLoanAmount + ", has been recieved\n");
                                                                SmsBodyHolder.Append("Trx No. " + receiptNumber + "\n");
                                                                SmsBodyHolder.Append("Physical reciept, visit any of our offices.");
                                                                await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));


                                                            }
                                                        }

                                                    }

                                                    break;
                                                }
                                                else if (trxStatus.ToUpper() == "FAILED")
                                                {
                                                    //Completeed
                                                    var UpdateStatus = await Task.Run(() => UpdatePaymentTransaction(TransactionUuidRegatMtn, ExternalTranx, 2, receiptNumber)); //Transaction completed
                                                    string _usStatusCode = UpdateStatus.statusCode;
                                                    string _notification = UpdateStatus.notification;
                                                    if (_usStatusCode == "OT001")
                                                    {
                                                        //Send Sms notification
                                                        //Send Sms notification
                                                        SmsBodyHolder.Append("Hi, " + AccountHolderName);
                                                        SmsBodyHolder.Append("Loan repayment for K" + PaidLoanAmount + ", has failed.\n");
                                                        SmsBodyHolder.Append("Trx No. " + receiptNumber + "\n");
                                                        SmsBodyHolder.Append("Physical reciept, visit any of our offices.");
                                                        await Task.Run(() => Sms_Notification_Client.SendSmsNotification(ToPayPhoneNumber, SmsBodyHolder.ToString()));


                                                    }
                                                    break;
                                                }

                                            }
                                        }
                                    }                                    

                                }                               

                            }
                        }
                      
                    }
                 
                }
                
            }
            catch (Exception Ex)
            {
 
            }
            finally
            {
                CLP_App_CN.Close();
            }
        }


    }
}