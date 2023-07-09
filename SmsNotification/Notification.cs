using System.Net.Http;
using Newtonsoft.Json;


namespace SmsNotification
{
    public class Notification
    {
        AllCredentialHolder.CreadentialHolder.SmsDetails Sms_Client = new AllCredentialHolder.CreadentialHolder.SmsDetails();

        public async Task<ModelView.GenralResponseMV.SMS_Final_Notification_Response> SendSmsNotification(string ClientNumber, string SmsBody)
        {
            ModelView.GenralResponseMV.SMS_Final_Notification_Response NotificationResponse = null;

            HttpClient HC_Client = new HttpClient();

            try
            {
                string SmsNotificationUrl = Sms_Client.SmsGatewayDomain + "sms-api/sms-request/SMS_Notification/Send_Sms?Company_ID=MLoanZ&Phone_Number=" + ClientNumber + "&Client_Message=" + SmsBody;
                var SmsNotificationResponse = await Task.Run(() => HC_Client.GetAsync(SmsNotificationUrl));

                if (SmsNotificationResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var SmsContentDetails = SmsNotificationResponse.Content.ReadAsStringAsync().Result;
                    var SmsContentResponse = JsonConvert.DeserializeObject<ModelView.GenralResponseMV.SMS_Final_Notification_Response>(SmsContentDetails);
                    string _smsStatusCode = SmsContentResponse.Status_Code;
                    string _notification = SmsContentResponse.Msg_Notification;

                    if (_smsStatusCode == "OT001")
                    {
                        NotificationResponse = new ModelView.GenralResponseMV.SMS_Final_Notification_Response()
                        {
                            Status_Code = "OT001",
                            Msg_Notification = "Sms sent"
                        };
                    }
                    else
                    {

                        NotificationResponse = new ModelView.GenralResponseMV.SMS_Final_Notification_Response()
                        {
                            Status_Code = "OT002",
                            Msg_Notification = "Sms not sent"
                        };
                    }
                }
                else
                {
                    NotificationResponse = new ModelView.GenralResponseMV.SMS_Final_Notification_Response()
                    {
                        Status_Code = "OT002",
                        Msg_Notification = "Sms not sent"
                    };
                }
            }
            catch (Exception Ex)
            {
                NotificationResponse = new ModelView.GenralResponseMV.SMS_Final_Notification_Response()
                {
                    Status_Code = "OT099",
                    Msg_Notification = "Sms not sent"
                };
            }

            return NotificationResponse;
        }


    }
}