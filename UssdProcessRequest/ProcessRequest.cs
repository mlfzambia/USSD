using System.Data.SqlClient;
using System.Data;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace UssdProcessRequest
{

    public class ProcessRequest
    {
        enum LoanAccount
        {
            BusinessLoan = 1,
            AgricultureLoan = 2
        }


        ConnectionLinks.LinkDetails ConnectionInfor = new ConnectionLinks.LinkDetails();
        ModelView.GenralResponseMV.AllGenralResponse GenralResponse = new ModelView.GenralResponseMV.AllGenralResponse();

        UssdTransaction.UssdTransactionAsync Ussd_Trx_Client = new UssdTransaction.UssdTransactionAsync();



        internal async Task<string> UssdRequestDetails(int IsNewRequest, string MsisdnNumber, string SessionId, string ClientInput)
        {

            SqlConnection UR_App_Con = new SqlConnection(ConnectionInfor.localConnectionDatabase());
            string UssdStylesResponse = null;

            try
            {


                //New Client request 
                if (IsNewRequest == 1)
                {
                    //Register The Current Ussd Transaction

                    string UssdRegistration = "Insert Into UssdTransactionSession(ui_SessionId,ui_Msisdn,ui_page) Values (@SessionId,@Msisdn,@pageNo)";
                    SqlCommand UR_App_CM = new SqlCommand(UssdRegistration, UR_App_Con);
                    UR_App_CM.Parameters.Add("@SessionId", SqlDbType.NVarChar).Value = SessionId;
                    UR_App_CM.Parameters.Add("@Msisdn", SqlDbType.NVarChar).Value = MsisdnNumber;
                    UR_App_CM.Parameters.Add("@pageNo", SqlDbType.NVarChar).Value = 0;


                    //Check database Connection 
                    if (UR_App_Con.State == ConnectionState.Closed)
                    {
                        UR_App_Con.Open();// Open database connect
                    }

                    int URCount = UR_App_CM.ExecuteNonQuery();

                    if (URCount == 1)
                    {
                        UssdStylesResponse = "Welcome to MicroLoan.\n" +
                            "\n" +
                            "1. Repay Loan\n" +
                            "2. Registration\n";
                    }
                    else
                    {
                        UssdStylesResponse = "Connection problem or invalid MMI code";
                    }
                }
                //Client contiuning ussd process
                else if (IsNewRequest == 0)
                {
                    //Get Selected Page
                    int InitialSelectedPage;
                    //Selected Menu


                    //Get Client Ussd Process
                    string ClientStepsQuery = "Select ui_Msisdn,ui_LoginStatus,ui_Page,ui_Selected_Option,ui_Selected_Option1,ui_Selected_Option2,ui_Selected_Option3,ui_Selected_Option4 from [UssdTransactionSession](nolock) Where ui_SessionId = @sessionId";
                    SqlCommand CS_App_CM = new SqlCommand(ClientStepsQuery, UR_App_Con);
                    CS_App_CM.Parameters.Add("@sessionId", SqlDbType.NVarChar).Value = SessionId;

                    if (UR_App_Con.State == ConnectionState.Closed)
                    {
                        UR_App_Con.Open(); // Open Database Connection
                    }

                    SqlDataAdapter UR_App_DA = new SqlDataAdapter(CS_App_CM);
                    DataSet UR_App_DS = new DataSet();

                    UR_App_DA.Fill(UR_App_DS, "UR");
                    int URCount = UR_App_DS.Tables["UR"].Rows.Count;

                    //Current Main Page
                    int MainPageSelection = Convert.ToInt16(UR_App_DS.Tables["UR"].Rows[0]["ui_Page"]);


                    switch (MainPageSelection)
                    {
                        case 0:
                            //Check What Client Has Selected from the Main Menu

                            if (ClientInput == "1") //Selected Page One
                            {
                                UssdStylesResponse = "Please enter your account";
                                //Update Client Page Slection
                                //Show Page One Which is Enter Account
                                await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "1", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));

                            }
                            else if (ClientInput == "2") //Selected Page Two
                            {
                                UssdStylesResponse = "Registration coming soon.";

                            }
                            else
                            {
                                UssdStylesResponse = "Selected Input is invalied";
                            }

                            break;

                        case 1:

                            int OptionSelected = Convert.ToInt16(UR_App_DS.Tables["UR"].Rows[0]["ui_Selected_Option"]);

                            if (OptionSelected == 1)
                            {
                                var VerificationResponse = await Task.Run(() => RequestClientDetails(ClientInput));

                                UssdStylesResponse = "Hi, " + VerificationResponse.parentName;
                                
                                await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "2", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));




                                //Verify Client Account Number
                                //var VerificationResponse = await Task.Run(() => RequestClientDetails(ClientInput));
                                //string StatusCode = VerificationResponse.statusCode;
                                //string notification = VerificationResponse.notification;

                                //if (StatusCode == "OT001")
                                //{
                                //    var ClientResponseDetails = VerificationResponse.details;
                                //    int ClientLoansCount = ClientResponseDetails.Count;

                                //    StringBuilder NotificationHolder = new StringBuilder();
                                //    int OptCount = 0;

                                //    if (ClientLoansCount >= 1)
                                //    {
                                //        for (int i = 0; i <= ClientLoansCount - 1; i++)
                                //        {
                                //            OptCount = OptCount + 1;
                                //            NotificationHolder.Append(OptCount + "." + ClientResponseDetails[i].loanTypeName.ToString() + " (" + ClientResponseDetails[i].amount.ToString() + ")\n");
                                //        }

                                //        UssdStylesResponse = "Hi, " + ClientResponseDetails[0].accountName.ToString() + "\n" +
                                //            "" + NotificationHolder.ToString();

                                //        await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "2", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));

                                //    }
                                //}
                                //else
                                //{
                                //    UssdStylesResponse = "Wrong account.\n" +
                                //        "Please enter your account";
                                //    //Update Client Page Slection
                                //    await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "1", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));

                                //}


                            }
                            else if (OptionSelected == 2)
                            {
                                if (ClientInput == "1")
                                {
                                    UssdStylesResponse = "Enter your business loan Amount";

                                    //Update client stage to entering amount
                                    await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "3", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));

                                }
                                else if (ClientInput == "2")
                                {
                                    UssdStylesResponse = "Enter your Agriculture loan Amount";

                                    //Update client stage to entering amount
                                    await Task.Run(() => RecordInitialTransaction(SessionId, MsisdnNumber, 1, "3", DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString(), DBNull.Value.ToString()));

                                }



                            }
                            else if (OptionSelected == 3)
                            {
                                //Process completed
                                UssdStylesResponse = "end\n Shortly you will receice a prompt to enter your MoMo PIN";



                                // Ussd_Trx_Client.ITransactionRegistration()
                            }
                            else
                            {
                                UssdStylesResponse = "Something went wrong, try again later.";
                            }
                            break;

                        case 2:
                            UssdStylesResponse = "Customer registration coming soon";
                            break;

                    }

                }
            }
            catch (Exception ex)
            {
                UssdStylesResponse = "System error or connection problem or invalid MMI code";
            }

            return UssdStylesResponse;
        }

        #region Add Ussd Transaction details

        private ModelView.GenralResponseMV.AllGenralResponse RecordInitialTransaction(string _sessionId, string msisdn, int SelectedPage, string OptionPages, string Opt1, string Opt2, string Opt3, string Opt4)
        {
            SqlConnection RI_App_Con = new SqlConnection(ConnectionInfor.localConnectionDatabase());

            try
            {
                string RI_Initial_Transaction_Query = "Update  [UssdTransactionSession] set  ui_Msisdn=@Msisdn ,ui_Page=@Page ,ui_Selected_Option=@optionalPages,ui_Selected_Option1=@opt1,ui_Selected_Option2=@opt2,ui_Selected_Option3=@opt3,ui_Selected_Option4=@opt4 Where ui_SessionId=@SessionId";
                SqlCommand RI_App_CM = new SqlCommand(RI_Initial_Transaction_Query, RI_App_Con);
                RI_App_CM.Parameters.Add("@SessionId", SqlDbType.NVarChar).Value = _sessionId;
                RI_App_CM.Parameters.Add("@Msisdn", SqlDbType.NVarChar).Value = msisdn;
                RI_App_CM.Parameters.Add("@Page", SqlDbType.NVarChar).Value = SelectedPage;
                RI_App_CM.Parameters.Add("@optionalPages", SqlDbType.NVarChar).Value = OptionPages;

                RI_App_CM.Parameters.Add("@opt1", SqlDbType.NVarChar).Value = Opt1;
                RI_App_CM.Parameters.Add("@opt2", SqlDbType.NVarChar).Value = Opt2;
                RI_App_CM.Parameters.Add("@opt3", SqlDbType.NVarChar).Value = Opt3;
                RI_App_CM.Parameters.Add("@opt4", SqlDbType.NVarChar).Value = Opt4;


                if (RI_App_Con.State == ConnectionState.Closed)
                {
                    RI_App_Con.Open();//Open Connection
                }

                var RICount = RI_App_CM.ExecuteNonQuery();
                if (RICount >= 1)
                {
                    GenralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Completed"
                    };
                }
                else
                {
                    GenralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Failed"
                    };
                }
            }
            catch (Exception Ex)
            {
                GenralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "Failed"
                };
            }
            finally
            {
                RI_App_Con.Close();
            }
            return GenralResponse;
        }

        //Connecting to Sanny
        private async Task< ModelView.SannyResponseMV.DetailsHeld> RequestClientDetails(string PhoneNumber)
        {
            MusoniRequest.MusoniClientsRequestAsync ClientRequest = new MusoniRequest.MusoniClientsRequestAsync();

            ModelView.SannyResponseMV.DetailsHeld SearchDetails = null;


            var ResponseDetails = await Task.Run(() => ClientRequest.IMusoniClientDetails(PhoneNumber));

            string statusCode = ResponseDetails.statusCode;
            string notification = ResponseDetails.notification;

            if (statusCode == "OT001")
            {
                SearchDetails = new ModelView.SannyResponseMV.DetailsHeld()
                {
                    parentName = ResponseDetails.holder.parentName,
                    parentAccountNo = ResponseDetails.holder.parentAccountNo,
                    entityName = ResponseDetails.holder.entityName
                };
            }
         
            return SearchDetails;



        }

        #endregion
    }





}