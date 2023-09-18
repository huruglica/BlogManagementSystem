using FirebaseAdmin.Messaging;

namespace FireBase
{
    public class FireBaseNotification
    {
        public void PushNotificationFireBase(string title, string body)
        {
            //Getting the device token after app deployment,
            //device token here is only a default string sample not actual device token
            var deviceToken = "a4b7efpppt5z788kV";

            var message = new Message()
            {
                Notification = new Notification()
                {
                    Title = title,
                    Body = body,
                },
                Token = deviceToken,
            };

            FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
