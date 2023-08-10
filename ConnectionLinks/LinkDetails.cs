using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace ConnectionLinks
{
    public class LinkDetails
    {

        #region Database Connection Connection 

        public string localConnectionDatabase()
        {
            string DBConnection;
            var ConnectionStatus = LocalConnectionDB();
            DBConnection = ConnectionStatus.GetConnectionString("MLFZDB");
            return DBConnection;
        }

        private IConfigurationRoot LocalConnectionDB()
        {
            var ConnectionHolderBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return ConnectionHolderBuilder.Build();
        }






        #endregion



        #region Mosoni Authentication 

        public string MosoniAuthResponse()
        {
            var AuthResponse = AuthenticationDetails();
            string ResponseDetails = AuthResponse.GetConnectionString("MosoniAuth");
            return ResponseDetails;
        }

        private IConfiguration AuthenticationDetails()
        {
            var MosoniDetails = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return MosoniDetails.Build();


        }



        #endregion


    }
}