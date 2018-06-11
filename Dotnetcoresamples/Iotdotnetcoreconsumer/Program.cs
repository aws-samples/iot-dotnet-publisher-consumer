using System;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;
using M2Mqtt.Messages;
using M2Mqtt.Net;
using M2Mqtt.Utility;
using M2Mqtt.Internal;

namespace Iotdotnetcoreconsumer
{
    class Program
    {
        static void Main(string[] args)
        {
          Console.WriteLine("AWS IOT dotnetcore message consumer starting");
            string IotEndPoint = "yourawsiotendpoint.amazonaws.com";
            int BrokerPort = 8883;
           string Topic = "Hello/World";
           
            var CaCert = X509Certificate.CreateFromCertFile("/home/swethaudit/dotnetdevice/root-CA.crt");
            var ClientCert = new X509Certificate2("/home/swethaudit/dotnetdevice/dotnet_devicecertificate.pfx", "password1");

          
            string ClientId = Guid.NewGuid().ToString();

            var IotClient = new MqttClient(IotEndPoint, BrokerPort, true, CaCert, ClientCert, MqttSslProtocols.TLSv1_2);

            IotClient.MqttMsgSubscribed += IotClient_MqttMsgSubscribed;
            IotClient.MqttMsgPublishReceived += IotClient_MqttMsgPublishReceived;

            IotClient.Connect(ClientId);

            Console.WriteLine("Connected to AWS IOT");
            IotClient.Subscribe(new string[] { Topic}, new byte[] {MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            while (true)
            {
                //Keeping the mainthread alive for the event receivers to get invoked

            }

        }

              private static void IotClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Message recived is " + System.Text.Encoding.UTF8.GetString(e.Message));
        }


       private static void IotClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine("Subscribed to the AWS IOT MQTT topic  ");
        }


    }
}
