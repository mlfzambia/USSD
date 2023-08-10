namespace ModelView
{
    public class GenralResponseMV
    {

        public class AllGenralResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
        }

        public class AnySingelAllGenralResponse
        {
            public string statusCode { get; set; }
            public string notification { get; set; }
            public string responseValue { get; set; }
        }

        //Sms Notification General Response
        public class SMS_Final_Notification_Response
        {
            public string Status_Code { get; set; }
            public string Msg_Notification { get; set; }
        }

    }
}