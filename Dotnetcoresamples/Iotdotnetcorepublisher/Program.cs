using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;

namespace Iotdotnetcorepublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotEndpoint = "a2p1hwvv77f23d-ats.iot.us-east-1.amazonaws.com";
            Console.WriteLine("AWS IoT Dotnet core message publiser starting");
            int brokerPort = 8883;

            string message = "Test message";
            string topic = "Hello/World";

            var caCert = X509Certificate.CreateFromCertFile(Path.Join(AppContext.BaseDirectory, "AmazonRootCA1.crt"));
            var clientCert = new X509Certificate2(Path.Join(AppContext.BaseDirectory, "certificate.cert.pfx"), "MyPassword1");

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
