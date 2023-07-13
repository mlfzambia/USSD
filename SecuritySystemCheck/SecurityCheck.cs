using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace SecuritySystemCheck
{
    public class SecurityCheck
    {
        ConnectionLinks.LinkDetails ConnectionHolder = new ConnectionLinks.LinkDetails();
        SmsNotification.Notification Sms_client = new SmsNotification.Notification();



        #region Encryption Creation

        public async Task<ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse> SecurityGenerator(string ClientName)
        {
            SqlConnection SGAppCon = new SqlConnection(ConnectionHolder.localConnectionDatabase());
            ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse SecurityDetails = null;

            try
            {
                //Create Uuid Key
                Guid _uuidHoler = Guid.NewGuid();
                string _uuidClient = _uuidHoler.ToString("N");

                var SecurityRequest = await Task.Run(() => SecurityKeyGenerator());
                var SubscriptionKey = await Task.Run(() => SubscriptionKeyGenerator(_uuidClient));

                var EncryptedSecurityPassword = await Task.Run(() => SecurityPasswordEncrytion(_uuidClient, SecurityRequest, SubscriptionKey));

                //Randomg Key
                string DecRandnumber = await Task.Run(() => CreateDecoNumber());
                //Add Security 

                string AddNewCredentials = "Insert Into SecuriryX305 (sxUuidKey, sxPassword,sxSecretKey,sxSubscriptionKey,sxDecoNumber,ClientName) Values (@UuidKey, @Password,@SecretKey,@SubscriptionKey,@DecRandnumber,@ClientName)";
                SqlCommand anc_App_CM = new SqlCommand(AddNewCredentials, SGAppCon);

                anc_App_CM.Parameters.Add("@UuidKey", SqlDbType.NVarChar).Value = _uuidClient;
                anc_App_CM.Parameters.Add("@Password", SqlDbType.NVarChar).Value = EncryptedSecurityPassword;
                anc_App_CM.Parameters.Add("@SecretKey", SqlDbType.NVarChar).Value = SecurityRequest;
                anc_App_CM.Parameters.Add("@SubscriptionKey", SqlDbType.NVarChar).Value = SubscriptionKey;
                anc_App_CM.Parameters.Add("@DecRandnumber", SqlDbType.NVarChar).Value = DecRandnumber;
                anc_App_CM.Parameters.Add("@ClientName", SqlDbType.NVarChar).Value = ClientName;

                if (SGAppCon.State == ConnectionState.Closed)
                {
                    SGAppCon.Open();
                }

                int _ancCount = anc_App_CM.ExecuteNonQuery();

                if (_ancCount == 1)
                {
                    SecurityDetails = new ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse()
                    {
                        statusCode = "OT001",
                        notification = "User Key created",
                        details = new ModelView.SecurityDetailsMV.CreatedDetails()
                        {
                            uuidKey = _uuidClient,
                            clientSubscription = SubscriptionKey
                        }
                    };
                }
                else
                {
                    SecurityDetails = new ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse()
                    {
                        statusCode = "OT002",
                        notification = "User Key could not be creates"
                    };
                }
            }
            catch (Exception Ex)
            {
                SecurityDetails = new ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }
            return SecurityDetails;
        }


        public string SecurityKeyGenerator()
        {
            string generalResponse = null;

            //Get current system date
            DateTime dateTime = DateTime.Now;
            string newDate = Convert.ToDateTime(dateTime).ToString("dd-MM-yyyy");

            //Generator Uuid
            Guid _gcode = Guid.NewGuid();
            string UuidCode = _gcode.ToString();
            string _securityDetails = newDate + UuidCode;

            //Convert it to Uuid
            byte[] _byteHolder = Encoding.ASCII.GetBytes(_securityDetails);
            string ConvertBase64 = Convert.ToBase64String(_byteHolder);

            //Sha security
            SHA256 _shaHold = SHA256.Create();
            byte[] _shaBytes = _shaHold.ComputeHash(Encoding.ASCII.GetBytes(ConvertBase64));

            StringBuilder _sbbuilder = new StringBuilder();

            for (int i = 0; i < _shaBytes.Length; i++)
            {
                _sbbuilder.Append(_shaBytes[i].ToString("x3"));
            }

            generalResponse = _sbbuilder.ToString();
            return generalResponse;
        }

        public string SubscriptionKeyGenerator(string clientUUid)
        {
            string CompanyName = "MLFZambia";
            string _conkatinate = clientUUid + CompanyName;
            byte[] _subBytes = Encoding.ASCII.GetBytes(_conkatinate);
            string SubKeyHolder = Convert.ToBase64String(_subBytes);
            return SubKeyHolder;
        }

        public string CreateDecoNumber()
        {
            Random cdn_Rnd = new Random();
            int NewDecNumber = cdn_Rnd.Next(100, 999);
            return NewDecNumber.ToString();
        }

        public string SecurityPasswordEncrytion(string clientUUid, string secrityKey, string SubcriptionKey)
        {
            string _conkatinateKeys = clientUUid + secrityKey + SubcriptionKey;
            byte[] _byteHold = Encoding.ASCII.GetBytes(_conkatinateKeys);
            string _converBytes = Convert.ToBase64String(_byteHold);

            //Hash the values
            SHA256 _sha256Holder = SHA256.Create();
            byte[] _hasBytes = _sha256Holder.ComputeHash(Encoding.ASCII.GetBytes(_converBytes));

            StringBuilder _sbBuilder = new StringBuilder();

            for (int i = 0; i < _hasBytes.Length; i++)
            {
                _sbBuilder.Append(_hasBytes[i].ToString("x2"));
            }

            return _sbBuilder.ToString();
        }


        #endregion



        public async Task<ModelView.GenralResponseMV.AllGenralResponse> SecurityCheckPoint(string ClientUuid, string SubcriptionKey)
        {
            ModelView.GenralResponseMV.AllGenralResponse GeneralResponse = null;

            SqlConnection SCP_App_Con = new SqlConnection(ConnectionHolder.localConnectionDatabase());

            try
            {
                //Verify that The client and sub key are correct
                string VerificationQuery = "Select sxPassword,sxSecretKey,sxSubscriptionKey,UssdemergencyStopStatus from [SecuriryX305](nolock) Where sxUuidKey =@UuidKey";
                SqlCommand SCP_App_CM = new SqlCommand(VerificationQuery, SCP_App_Con);

                SCP_App_CM.Parameters.Add("@UuidKey", SqlDbType.NVarChar).Value = ClientUuid;

                if (SCP_App_Con.State == ConnectionState.Closed)
                {
                    SCP_App_Con.Open(); // Open database connection 
                }

                SqlDataAdapter SCP_App_DA = new SqlDataAdapter(SCP_App_CM);
                DataSet SCP_App_DS = new DataSet();
                SCP_App_DA.Fill(SCP_App_DS, "SCP");

                int _scoCount = SCP_App_DS.Tables["SCP"].Rows.Count;

                if (_scoCount == 1)
                {
                    string dbPassword = SCP_App_DS.Tables["SCP"].Rows[0]["sxPassword"].ToString();
                    string dbSecretKey = SCP_App_DS.Tables["SCP"].Rows[0]["sxSecretKey"].ToString();
                    string dbSubcriptionKey = SCP_App_DS.Tables["SCP"].Rows[0]["sxSubscriptionKey"].ToString();

                    if (SubcriptionKey.Equals(dbSubcriptionKey))
                    {
                        //Verify that the passowrd Match
                        string EncryptedPassword = await Task.Run(() => SecurityPasswordEncrytion(ClientUuid, dbSecretKey, SubcriptionKey));

                        if (EncryptedPassword.Equals(dbPassword))
                        {
                            //Check if the System is allowed to run
                            int UssdemergencyStopStatus = Convert.ToInt16(SCP_App_DS.Tables["SCP"].Rows[0]["UssdemergencyStopStatus"]);
                            if (UssdemergencyStopStatus == 0)
                            {
                                //Ussd running
                                GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                                {
                                    statusCode = "OT001",
                                    notification = "Successfull"
                                };
                            }
                            else
                            {
                                GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                                {
                                    statusCode = "OT002",
                                    notification = "failed"
                                };
                            }
                        }
                        else
                        {
                            GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                            {
                                statusCode = "OT002",
                                notification = "Failed"
                            };
                        }
                    }
                    else
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                        {
                            statusCode = "OT002",
                            notification = "Subscription does not Match"
                        };

                    }
                }
                else
                {
                    GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Uuid Key does not Match"
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


        //Shut Down Ussd Process

        public async Task<ModelView.GenralResponseMV.AllGenralResponse> ShutDownUssdOperation(int ShutDownId)
        {
            SqlConnection SD_App_Con = new SqlConnection(ConnectionHolder.localConnectionDatabase());
            ModelView.GenralResponseMV.AllGenralResponse GeneralResponse = null;

            string SMSBodyNotification = null;

            try
            {
                string GetUssdStatus = "Select ClientName,UssdemergencyStopStatus from [SecuriryX305](nolock)";
                SqlCommand GUS_App_CM = new SqlCommand(GetUssdStatus, SD_App_Con);

                if (SD_App_Con.State == ConnectionState.Closed)
                {
                    SD_App_Con.Open();
                }

                SqlDataAdapter SD_App_DA = new SqlDataAdapter(GUS_App_CM);
                DataSet SD_App_DS = new DataSet();
                SD_App_DA.Fill(SD_App_DS, "SD");

                int UssdStatus = Convert.ToInt16(SD_App_DS.Tables["SD"].Rows[0]["UssdemergencyStopStatus"]);


                if (ShutDownId == 0)
                {
                    //Check if the Ussd Is Up and running

                    if (UssdStatus.Equals(ShutDownId))
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Ussd is current Up and Running"
                        };
                    }
                    else
                    {
                        //Bring Ussd Up
                        var UssdResponseStatus = await Task.Run(() => UssdStatusChange(ShutDownId));
                        string _statusCode = UssdResponseStatus.statusCode;
                        string _notification = UssdResponseStatus.notification;

                        if (_statusCode == "OT001")
                        {
                            SMSBodyNotification = "Ussd has been turned On";
                            await Task.Run(() => SmsNotification(SMSBodyNotification));
                             
                            GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                            {
                                statusCode = "OT001",
                                notification = "Ussd Is Now On"
                            };
                        }
                        else
                        {
                            GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                            {
                                statusCode = "OT001",
                                notification = "Operation failed to complete"
                            };

                        }

                    }

                }
                else if (ShutDownId == 1)
                {
                    //Check if the Ussd Is down 

                    //Check if the Ussd Is Up and running

                    if (UssdStatus.Equals(ShutDownId))
                    {
                        GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                        {
                            statusCode = "OT001",
                            notification = "Ussd is current Down"
                        };
                    }
                    else
                    {
                        //Bring Ussd Up
                        var UssdResponseStatus = await Task.Run(() => UssdStatusChange(ShutDownId));
                        string _statusCode = UssdResponseStatus.statusCode;
                        string _notification = UssdResponseStatus.notification;

                        if (_statusCode == "OT001")
                        {
                            SMSBodyNotification = "Ussd is has been turned down";
                            await Task.Run(() => SmsNotification(SMSBodyNotification));


                            GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                            {
                                statusCode = "OT001",
                                notification = "Ussd Is Now Down"
                            };
                        }
                        else
                        {
                            GeneralResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                            {
                                statusCode = "OT001",
                                notification = "Operation failed to complete"
                            };

                        }

                    }

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


        internal ModelView.GenralResponseMV.AllGenralResponse UssdStatusChange(int StatusId)
        {
            SqlConnection _usCon = new SqlConnection(ConnectionHolder.localConnectionDatabase());
            ModelView.GenralResponseMV.AllGenralResponse _generalResponse = null;


            try
            {
                string usStatusChangeQuery = "update [SecuriryX305] set UssdemergencyStopStatus = @statusChange";
                SqlCommand _us_App_CM = new SqlCommand(usStatusChangeQuery, _usCon);
                _us_App_CM.Parameters.Add("@statusChange", SqlDbType.Int).Value = StatusId;

                if (_usCon.State == ConnectionState.Closed)
                {
                    _usCon.Open(); //Open DB Connection
                }

                int _usCount = _us_App_CM.ExecuteNonQuery();

                if (_usCount >= 1)
                {
                    _generalResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Status Change completed"
                    };

                }
                else
                {
                    _generalResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Status Change failed"
                    };
                }
            }
            catch (Exception)
            {
                _generalResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }

            return _generalResponse;

        }


        //Sms notification
        internal async Task<ModelView.GenralResponseMV.AllGenralResponse> SmsNotification(string SmsBody)
        {
            SqlConnection _smsCon = new SqlConnection(ConnectionHolder.localConnectionDatabase());
            ModelView.GenralResponseMV.AllGenralResponse GeneraResponse = null;

            try
            {
                string GetSmsNotifyDetailsQuery = "Select sdcContactName,scContactNumber from [UssdShutDownContacts](nolock)";
                SqlCommand _gsd_App_Cm = new SqlCommand(GetSmsNotifyDetailsQuery, _smsCon);
                if (_smsCon.State == ConnectionState.Closed)
                {
                    _smsCon.Open(); // Open database
                }

                SqlDataAdapter _sms_App_DA = new SqlDataAdapter(_gsd_App_Cm);
                DataSet _sms_App_DS = new DataSet();
                _sms_App_DA.Fill(_sms_App_DS, "SMS");

                int _smsCount = _sms_App_DS.Tables["SMS"].Rows.Count;

                if (_smsCount >= 1)
                {
                    _smsCount -= 1;

                    for (int i = 0; i <= _smsCount; i++)
                    {
                        string ContactNumber = _sms_App_DS.Tables["SMS"].Rows[i]["scContactNumber"].ToString();
                        string ContactName = _sms_App_DS.Tables["SMS"].Rows[i]["sdcContactName"].ToString();

                        StringBuilder Sms_Body_Details = new StringBuilder();
                        Sms_Body_Details.Append("Hi, " + ContactName+"\n");
                        Sms_Body_Details.Append( SmsBody);
                        await Task.Run(() => Sms_client.SendSmsNotification(ContactNumber, Sms_Body_Details.ToString())); 
                    }
                    GeneraResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT001",
                        notification = "Sms notification Sent"
                    };


                }
                else
                {
                    GeneraResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                    {
                        statusCode = "OT002",
                        notification = "Sms notification was not sent"
                    };
                }

            }
            catch (Exception Ex)
            {
                GeneraResponse = new ModelView.GenralResponseMV.AllGenralResponse()
                {
                    statusCode = "OT099",
                    notification = "System error, contact system administrator"
                };
            }
            return GeneraResponse;

        }

    }
}