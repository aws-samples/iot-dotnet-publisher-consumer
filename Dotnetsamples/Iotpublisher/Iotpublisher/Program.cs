using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace Iotpublisher
{
    class Program
    {
        static void Main(string[] args)
        {

            string iotendpoint = "awsiotendpoint.amazonaws.com";
            int BrokerPort = 8883;
            string Topic = "Hello/World";

            var CaCert = X509Certificate.CreateFromCertFile(@"C:\Iotdevices\dotnetdevice\root-CA.crt");
            var ClientCert = new X509Certificate2(@"C:\Iotdevices\dotnetdevice\dotnet_devicecertificate.pfx", "password1");

            var Message = "Test message";
            string ClientId = Guid.NewGuid().ToString();

            var IotClient = new MqttClient(iotendpoint, BrokerPort, true, CaCert, ClientCert, MqttSslProtocols.TLSv1_2);

           
            IotClient.Connect(ClientId);
            Console.WriteLine("Connected");


            while (true)
            {
                IotClient.Publish(Topic, Encoding.UTF8.GetBytes(Message));
                Console.WriteLine("published" + Message);
                Thread.Sleep(5000);

            }
            //publish to the topic



        }

    }
}

