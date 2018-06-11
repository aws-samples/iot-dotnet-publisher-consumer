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

namespace Iotdotnetcorepublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string IotEndPoint = "yourawsiotendpoint.amazonaws.com";
            Console.WriteLine("AWS IOT Dotnet core message publiser starting");
            int BrokerPort = 8883;
            string Topic = "Hello/World";

            var CaCert = X509Certificate.CreateFromCertFile("/home/swethaudit/dotnetdevice/root-CA.crt");
            var ClientCert = new X509Certificate2("/home/swethaudit/dotnetdevice/dotnet_devicecertificate.pfx", "password1");

            var Message = "Test message";
            string ClientId = Guid.NewGuid().ToString();

            var IotClient = new MqttClient(IotEndPoint, BrokerPort, true, CaCert, ClientCert, MqttSslProtocols.TLSv1_2);

            IotClient.Connect(ClientId);
            Console.WriteLine("Connected to AWS IOT");


            

            while (true)
            {
                IotClient.Publish(Topic, Encoding.UTF8.GetBytes(Message));
                Console.WriteLine("Message published");
                Thread.Sleep(5000);

            }

        }
    }
}
