using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using System.IO;

namespace Iotpublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotEndpoint = "<<your-iot-endpoint>>";
            Console.WriteLine("AWS IoT Dotnet message publisher starting..");

            int brokerPort = 8883;
            string topic = "Hello/World";
            string message = "Test message";

            var caCert = X509Certificate.CreateFromCertFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AmazonRootCA1.crt"));
            var clientCert = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.cert.pfx"), "MyPassword1");

            var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client id: {clientId}.");

            int i = 0;
            while (true)
            {
                client.Publish(topic, Encoding.UTF8.GetBytes($"{message} {i}"));
                Console.WriteLine($"Published: {message} {i}");
                i++;
                Thread.Sleep(5000);
            }
        }
    }
}

