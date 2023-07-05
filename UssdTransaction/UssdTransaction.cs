using System;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace UssdTransaction
{
    public class UssdTransaction
    {
        MtnPaymentProcessing.MTNPayments PaymentClient = new MtnPaymentProcessing.MTNPayments();
        ConnectionLinks.LinkDetails DatabaseConnection = new ConnectionLinks.LinkDetails();
        AllCredentialHolder.QuickRefHolder _quickDetailHolder = new AllCredentialHolder.QuickRefHolder();


        //Transaction Registration

        internal ModelView.PaymentTransactionMV.PaymentResponse TransactionRegistration(string Clientaccount, string PhoneNumber, int ServiceProvider, int TransactionStatus)
        {
            SqlConnection TR_App_Conn = new SqlConnection(DatabaseConnection.localConnectionDatabase());
            ModelView.PaymentTransactionMV.PaymentResponse PaymentClient = null;

            try
            {
                //Get Transaction Uuid
                string InternalTraxId = _quickDetailHolder.UuidGenerented();

                //System data
                DateTime _dtHolder = DateTime.Now;
                string CurrentDate = Convert.ToDateTime(_dtHolder).ToString("yyyy-MM-dd");

                //Register Transaction
                string AddPendingTransaction = "Insert Into AccountTransaction (InternalTransactionId,Clientaccount,PhoneNumber,Trx_Date,ServiceProvider,TransactionStatus) Values " +
                    "(@InternalTransactionId,@Clientaccount,@PhoneNumber,@Trx_Date,@ServiceProvider,@TransactionStatus)";
                SqlCommand TR_App_CM = new SqlCommand(AddPendingTransaction, TR_App_Conn);
                TR_App_CM.Parameters.Add("@InternalTransactionId", SqlDbType.NVarChar).Value = InternalTraxId;
                TR_App_CM.Parameters.Add("@Clientaccount", SqlDbType.NVarChar).Value = Clientaccount;
                TR_App_CM.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = PhoneNumber;
                TR_App_CM.Parameters.Add("@Trx_Date", SqlDbType.DateTime).Value = CurrentDate;
                TR_App_CM.Parameters.Add("@ServiceProvider", SqlDbType.NVarChar).Value = ServiceProvider;
                TR_App_CM.Parameters.Add("@TransactionStatus", SqlDbType.NVarChar).Value = "1";

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
    }
}