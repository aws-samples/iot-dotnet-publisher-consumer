using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Security;
using System.Security.Cryptography.X509Certificates;


namespace Iotconsumer
{
    class Program
    {
        static void Main(string[] args)
        {
            string IotEndPoint = "awsiotendpoint.amazonaws.com";

            int BrokerPort = 8883;
            string Topic = "Hello/World";

            var CaCert = X509Certificate.CreateFromCertFile(@"C:\Iotdevices\dotnetdevice\root-CA.crt");
            var clientCert = new X509Certificate2(@"C:\Iotdevices\dotnetdevice\dotnet_devicecertificate.pfx", "password1");


            string ClientID = Guid.NewGuid().ToString();

            var IotClient = new MqttClient(IotEndPoint, BrokerPort, true, CaCert, clientCert, MqttSslProtocols.TLSv1_2);
            IotClient.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
            IotClient.MqttMsgSubscribed += Client_MqttMsgSubscribed;

            IotClient.Connect(ClientID);
            Console.WriteLine("Connected");
            IotClient.Subscribe(new string[] { Topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            while (true)
            {
                //keeping the main thread alive for the event call backs
            }

        }

        private static void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Message subscribed");
        }

        private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Message Received is      " + System.Text.Encoding.UTF8.GetString(e.Message));
        }

    }
}
