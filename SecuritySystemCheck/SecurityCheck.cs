using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SecuritySystemCheck
{
    public class SecurityCheck
    {
        ConnectionLinks.LinkDetails ConnectionHolder = new ConnectionLinks.LinkDetails();


        public async Task<ModelView.SecurityDetailsMV.CreatedSecurityDetailsResponse> SecurityGenerator()
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

                string AddNewCredentials = "Insert Into SecuriryX305 (sxUuidKey, sxPassword,sxSecretKey,sxSubscriptionKey,sxDecoNumber) Values (@UuidKey, @Password,@SecretKey,@SubscriptionKey,@DecRandnumber)";
                SqlCommand anc_App_CM = new SqlCommand(AddNewCredentials, SGAppCon);

                anc_App_CM.Parameters.Add("@UuidKey", SqlDbType.NVarChar).Value = _uuidClient;
                anc_App_CM.Parameters.Add("@Password", SqlDbType.NVarChar).Value = EncryptedSecurityPassword;
                anc_App_CM.Parameters.Add("@SecretKey", SqlDbType.NVarChar).Value = SecurityRequest;
                anc_App_CM.Parameters.Add("@SubscriptionKey", SqlDbType.NVarChar).Value = SubscriptionKey;
                anc_App_CM.Parameters.Add("@DecRandnumber", SqlDbType.NVarChar).Value = DecRandnumber;


                //Send The ClientThe Credential

                //Concatenate the UuidKey and SubKey
                string UuidKey_SubKey = _uuidClient + ":" + SubscriptionKey;
                byte[] _bytetUuidSub = Encoding.ASCII.GetBytes(UuidKey_SubKey);
                string UuidSubKey = Convert.ToBase64String(_bytetUuidSub);

                //Now add the Deco Number with - 

                string DecodGen = UuidSubKey + "-" + DecRandnumber;
                byte[] _byteDeco = Encoding.UTF8.GetBytes(DecodGen);
                string DecoBase64 = Convert.ToBase64String(_byteDeco);

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
                            uuidKeyEncrypted = DecodGen
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
    }
}