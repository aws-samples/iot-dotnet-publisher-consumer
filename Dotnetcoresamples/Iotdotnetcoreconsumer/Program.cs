using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;
using M2Mqtt.Messages;

namespace Iotdotnetcoreconsumer
{
    class Program
    {
        private static ManualResetEvent manualResetEvent;

        static void Main(string[] args)
        {
            string iotEndpoint = "yourendpointhere.iot.us-east-1.amazonaws.com";
            int brokerPort = 8883;
           
            Console.WriteLine("AWS IoT dotnetcore message consumer starting..");
            var caCert = X509Certificate.CreateFromCertFile(Path.Join(AppContext.BaseDirectory, "AmazonRootCA1.crt"));
            var clientCert = new X509Certificate2(Path.Join(AppContext.BaseDirectory, "certificate.cert.pfx"), "MyPassword1");

            var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);

            client.MqttMsgSubscribed += IotClient_MqttMsgSubscribed;
            client.MqttMsgPublishReceived += IotClient_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            Console.WriteLine($"Connected to AWS IoT with client ID: {clientId}");

            string topic = "Hello/World";
            client.Subscribe(new string[] { topic }, new byte[] {MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

            // Keep the main thread alive for the event receivers to get invoked
            KeepConsoleAppRunning(() => {
                client.Disconnect();
                Console.WriteLine("Disconnecting client..");
            });
        }

        private static void IotClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(e.Message));
        }

        private static void IotClient_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            Console.WriteLine($"Subscribed to the AWS IoT MQTT topic.");
        }

        private static void KeepConsoleAppRunning(Action onShutdown)
        {
            manualResetEvent = new ManualResetEvent(false);
            Console.WriteLine("Press CTRL + C or CTRL + Break to exit...");

            Console.CancelKeyPress += (sender, e) =>
            {
                onShutdown();
                e.Cancel = true;
                manualResetEvent.Set();
            };

            manualResetEvent.WaitOne();
        }
    }
}
