﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿

#  1.Objective

The aim of this repository is to provide code samples for simple IOT device publisher and IOT device consumer using .NET framework and .NET core.

# 2. Why .NET publisher and .NET consumer for AWS IOT

At this point in time, there is no AWS IOT device SDK for Microsoft C#. This does not impact the micro, small and medium sized IOT nodes leveraging AWS IOT Framework. The current plethora of IOT device SDKs offered by AWS is more than enough for micro, small and medium-large nodes. However, the large IOT nodes are typically massive enterprise servers running in a smart city use case or a large IIOT implementation in process industries such as petroleum, paper or pulp. There the very nature of network segmentation of IIOT architecture and presence of enterprise technology stack on these IIOT layers would necessitate to leverage a programming language such as C# or Java in implementing those IOT nodes. AWS already offers IOT device SDKs in Java. This post is all about the covering the edge case of implementing IOT device publisher and device consumer using Microsoft C#. 

# 3. AWS IOT device publisher and consumer using .NET framework


## 3a. Development environment
- Windows 10
- Visual Studio 2017 with latest updates
- Windows Subsystem for Linux 

## 3b. Create  an AWS IOT Thing

Let's create an IOT thing called 'dotnnetdevice' using the AWS IOT console.

![](/images/pic1.JPG)

![](/images/pic2.JPG)


Let's associate the following policy with the thing.

``` json

{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "iot:Publish",
        "iot:Subscribe",
        "iot:Connect",
        "iot:Receive"
      ],
      "Effect": "Allow",
      "Resource": [
        "*"
      ]
    }
  ]
}

``` 
During the Thing creation process you should get the following four security artifacts

- **Device certificate** - This file usually ends with ".pem.crt". When you download this it will save as .txt file extension in windows. Save it as 'dotnet-devicecertificate.pem' for convenience and make sure that it is of file type '.pem', not 'txt' or '.crt'

- **Device public key** - This file usually ends with ".pem" and is of file type ".key".  Rename this file as 'dotnet-public.pem' for convenience. 

- **Device private key** -  This file usually ends with ".pem" and is of file type ".key".  Rename this file as 'dotnet-private.pem' for convenience. Make sure that this file is referred with suffix ".key" in the code while making MQTT connection to AWS IOT.

- **Root certificate** - The default name for this file is VeriSign-Class 3-Public-Primary-Certification-Authority-G5.pem and rename it as "root-CA.crt" for convenience.


##  3c. Converting device certificate from .pem to .pfx

In order to establish an MQTT connection with the AWS IOT platform, the root CA certificate, the private key of the thing and the certificate of the thing/device are needed. The .NET cryptographic APIs can understand root CA (.crt), device private key (.key) out-of-box. It expects the device certificate to be in the .pfx format, not the .pem format. Hence we need to convert the device certificate from .pem to .pfx.

We'll leverage the openssl for converting .pem to .pfx. Navigate to the folder where all the security artifacts are present and launch bash for Windows 10.

The syntax for converting .pem to .pfx is below :-

openssl pkcs12 -export -in **iotdevicecertificateinpemformat** -inkey **iotdevivceprivatekey** -out **devicecertificateinpfxformat** -certfile **rootcertificatefile**

If you replace with actual file names the syntax will look like below

openssl pkcs12 -export -in dotnet-devicecertificate.pem -inkey dotnet-private.pem.key -out dotnet_devicecertificate.pfx -certfile root-CA.crt

![](/images/pic3.JPG)




##  3d. Device publisher using .NET framework

Let's create a windows application in Visual Studio 2017 and name it as 'Iotpublisher'.

On project reference --> Manage Nuget pakcages --> Browse --> 'M2mqtt' and install M2mqtt.

Import the following namespaces.

```  c#

using System;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using System.IO;

```

Then create an instance of Mqtt client object with IOT endpoint, broker port for MQTT, X509Certificate object for root certificate, X5092certificate object for device certificate and Mqttsslprotocols enumeration for TLS1.2. 

Once the connection is successful publish to AWS IOT by specifying the  Topic and Payload. The following code snippet covers all of these :-


```  c#
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
            
``` 

Hit F5 in visual studio and you should see the messages getting pushed to the AWS IOT Mqtt topic.
 
![](/images/pic5.JPG)


The complete visual studio solution for this publisher is available under the 'Dotnetsamples' folder in this repository. 


##  3e. Device consumer using .NET framework

Let's create a windows application in Visual Studio 2017 and name it as 'Iotconsumer'.

On project reference --> Manage Nuget pakcages --> Browse --> 'M2mqtt' and install M2mqtt.

Import the following namespaces.

```  c#
using System;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Threading;
using System.Text;
```

Then create an instance of Mqtt client object with IOT endpoint, broker port for MQTT, X509Certificate object for root certificate, X5092certificate object for device certificate and Mqttsslprotocols enumeration for TLS1.2. 

You can subscribe to the AWS IOT messages by specifying the Topic as string array and QoS level as byte array. Prior to this event callbacks for MqttMsgSubscribed and MqttMsgPublishReceived should be implemented. The following code snippet covers all of that :

```  c#
private static ManualResetEvent manualResetEvent;

static void Main(string[] args)
{
    string iotEndpoint = "<<your-iot-endpoint>>";
    int brokerPort = 8883;

    Console.WriteLine("AWS IoT dotnet message consumer starting..");

    var caCert = X509Certificate.CreateFromCertFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AmazonRootCA1.crt"));
    var clientCert = new X509Certificate2(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificate.cert.pfx"), "MyPassword1");
    
    var client = new MqttClient(iotEndpoint, brokerPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
    client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;
    client.MqttMsgSubscribed += Client_MqttMsgSubscribed;

    string clientId = Guid.NewGuid().ToString();
    client.Connect(clientId);
    Console.WriteLine($"Connected to AWS IoT with client ID: {clientId}");

    string topic = "Hello/World";
    client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

    // Keep the main thread alive for the event receivers to get invoked
    KeepConsoleAppRunning(() => {
        client.Disconnect();
        Console.WriteLine("Disconnecting client..");
    });
}

private static void Client_MqttMsgSubscribed(object sender, MqttMsgSubscribedEventArgs e)
{
    Console.WriteLine($"Successfully subscribed to the AWS IoT topic.");
}

private static void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
{
    Console.WriteLine("Message received: " + Encoding.UTF8.GetString(e.Message));
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
``` 
The complete visual studio solution for this publisher is available under the 'Dotnetsamples' folder in this repository.

Hit F5 in visual studio and you should see the messages getting consumed by subscriber.

![](/images/pic6.JPG)

# 4. AWS IOT device publisher and consumer using .NET core

## 4a. Development environment

The following constitutes the development environment for developing AWS IOT device publisher and consumer using .NET core.

- Ubuntu 16.0.4 or higher (or) any other latest Linux distros
- .NET core 2.0 or higher
- AWS cli
- Openssl latest version


## 4b. Create an AWS IOT Thing 
Let's leverage the same IOT thing 'dotnetdevice' created in the section 3b for .NET core as well.

## 4c. Copying security artifacts to Linux environment 
Let's copy the device certificate, device private key and root certificate covered in the section 3c to the Linux the environment where we are going to develop IOT publisher and consumer using .NET core.

## 4d. Device publisher using .NET core 

Let's create the .NET core console application for the producer by issuing the following commands in the Terminal.


``` shell
mkdir Iotdotnetcorepublisher
cd Iotdotnetcorepublisher
dotnet new console
dotnet add package M2MqttClientDotnetCore --version 1.0.1
dotnet restore
```
Open the program.cs in Visual Studio and import the following namespaces.

``` c#
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;
``` 

Then perform a 'dotnet restore' in the terminal. It will grab the assemblies for System.Security.Cryptography.X509Certificates.

Then create an instance of Mqtt client object with IOT endpoint, broker port for MQTT, X509Certificate object for root certificate, X5092certificate object for device certificate and Mqttsslprotocols enumeration for TLS1.2. 

Once the connection is successful publish to AWS IOT by specifying the topic and payload. The following code snippet covers all of these :

``` c#
string iotEndpoint = "<<your-iot-endpoint>>";
Console.WriteLine("AWS IoT Dotnet core message publisher starting");
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
``` 
Make sure that the appropriate linux path format is followed for accessing the root certificate and .pfx. It is mentioned in the above snippet. The complete .NET core project source for the publisher is available under the Dotnetcoresamples folder in this repository.

Run the application using 'dotnet run' and you should see messages published by dotnet core.

![](/images/pic7.png)



## 4e. Device consumer using .NET core 

Let's create the .NET core console application for the consumer by issuing the following commands in the Terminal.


``` shell
mkdir Iotdotnetcoreconsumer
cd Iotdotnetcoreconsumer
dotnet new console
dotnet add package M2MqttClientDotnetCore --version 1.0.1
dotnet restore

```
Open the program.cs in Visual Studio and import the following namespaces.

``` c#
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using M2Mqtt;
using M2Mqtt.Messages;
``` 

Then perform a 'dotnet restore' in the terminal. It will grab the assemblies for System.Security.Cryptography.X509Certificates.

Then create an instance of Mqtt client object with IOT endpoint, broker port for MQTT, X509Certificate object for root certificate, X5092certificate object for device certificate and Mqttsslprotocols enumeration for TLS1.2. 

You can subscribe to the AWS IOT messages by specifying the Topic as string array and Qos level as byte array. Prior to this event callbacks for MqttMsgSubscribed and MqttMsgPublishReceived should be implemented. The following code snippet covers all of that :-


``` c#
private static ManualResetEvent manualResetEvent;

static void Main(string[] args)
{
    string iotEndpoint = "<<your-iot-endpoint>>";
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
    Console.WriteLine($"Subscribing to topic: {topic}");
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
    Console.WriteLine($"Successfully subscribed to the AWS IoT topic.");
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
``` 

The complete .NET core project source for the publisher is available under the Dotnetcoresamples folder in this repository.

Run the application using 'dotnet run' and you should see messages consumed by the dotnetcore

![](/images/pic8.png)