using System.Data.SqlTypes;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Schema;

namespace MLFPortalInformation
{
    public class PortalInformation
    {
        ConnectionLinks.LinkDetails _connection = new ConnectionLinks.LinkDetails();

        public ModelView.Portal_InforMV.TransactionResponse TranstionListing()
        {
            SqlConnection _TL_App_Connect = new SqlConnection(_connection.localConnectionDatabase());

            ModelView.Portal_InforMV.TransactionResponse _transactionDetails = null;

            try
            {
                string TransactionDetailsQuery = "Select at.incrmid,  at.Clientaccount,at.PhoneNumber,at.ext_TransactionId,Trx_Date,at.Amount,np.npName,ts.tsName, at.LoanType,at.ClientLoanId,at.ReceiptNum,at.AccountHolderName,at.SessionId,at.PayerName, at.ErrorResponse  from [AccountTransaction] (nolock) as AT  inner join [dbo].[NetworkProvider](nolock) as NP On AT.serviceProvider = Np.npid inner join [TransactionStatus](nolock) TS On At.transactionStatus = Ts.tsId order by at.incrmid ";
                SqlCommand TDQ_App_CM = new SqlCommand(TransactionDetailsQuery, _TL_App_Connect);

                if (_TL_App_Connect.State == ConnectionState.Closed)
                {
                    _TL_App_Connect.Open(); // Open database
                }

                SqlDataAdapter _TL_App_DA = new SqlDataAdapter(TDQ_App_CM);
                DataSet _TL_App_DS = new DataSet();
                _TL_App_DA.Fill(_TL_App_DS, "TL");

                int _TLCount = _TL_App_DS.Tables["TL"].Rows.Count;

                if (_TLCount >= 1)
                {
                    List<ModelView.Portal_InforMV.TransactionDetails> _details = new List<ModelView.Portal_InforMV.TransactionDetails>();

                    _TLCount -= 1;

                    for (int i = 0; i <= _TLCount; i++)
                    {
                        //Remove the dash
                        string errorResponseChange = _TL_App_DS.Tables["TL"].Rows[i]["ErrorResponse"].ToString();
                        errorResponseChange= errorResponseChange.Replace('_', ' ');
                         
                        _details.Add(new ModelView.Portal_InforMV.TransactionDetails()
                        {
                            accountHolderName = _TL_App_DS.Tables["TL"].Rows[i]["AccountHolderName"].ToString(),
                            amount = Convert.ToDecimal(_TL_App_DS.Tables["TL"].Rows[i]["Amount"]),
                            clientaccount = _TL_App_DS.Tables["TL"].Rows[i]["Clientaccount"].ToString(),
                            clientLoanId = _TL_App_DS.Tables["TL"].Rows[i]["ClientLoanId"].ToString(),
                            extTransactionId = _TL_App_DS.Tables["TL"].Rows[i]["ext_TransactionId"].ToString(),
                            incrmid = _TL_App_DS.Tables["TL"].Rows[i]["incrmid"].ToString(),
                            LoanType = _TL_App_DS.Tables["TL"].Rows[i]["LoanType"].ToString(),

                            networkName = _TL_App_DS.Tables["TL"].Rows[i]["npName"].ToString(),
                            transactionStatus = _TL_App_DS.Tables["TL"].Rows[i]["tsName"].ToString(),

                            phoneNumber = _TL_App_DS.Tables["TL"].Rows[i]["PhoneNumber"].ToString(),
                            receiptNum = _TL_App_DS.Tables["TL"].Rows[i]["ReceiptNum"].ToString(),
                            sessionId = _TL_App_DS.Tables["TL"].Rows[i]["SessionId"].ToString(),
                            trxDate = _TL_App_DS.Tables["TL"].Rows[i]["Trx_Date"].ToString(),
                            errorResponse = errorResponseChange
                        });
                    }



                    _transactionDetails = new ModelView.Portal_InforMV.TransactionResponse()
                    {
                        statusCode = "OT001",
                        notification = "Clients found",
                        details = _details
                    };
                }
                else
                {
                    _transactionDetails = new ModelView.Portal_InforMV.TransactionResponse()
                    {
                        statusCode = "OT002",
                        notification = "Clients not found"                      
                    };
                } 
            }
            catch (Exception ex)
            {
                _transactionDetails = new ModelView.Portal_InforMV.TransactionResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };

            }

            return _transactionDetails;



        }



    }
}