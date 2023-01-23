using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTT_Broker
{
    public static class MQTT_Broker
    {

        public static async Task Force_Disconnecting_Client()
        {
            using (var mqttServer = await StartMqttServer())
            {
                // Let the client connect.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Now disconnect the client (if connected)
                var affectedClient = (await mqttServer.GetClientsAsync()).FirstOrDefault(c => c.Id == "MyClient");
                if (affectedClient != null)
                {
                    await affectedClient.DisconnectAsync();
                }
            }
        }

        public static async Task Run_Server_With_Logging()
        {
            /*
             * This sample starts a simple MQTT server and prints the logs to the output.
             *
             * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
             *
             * See sample "Run_Minimal_Server" for more details.
             */

            var mqttFactory = new MqttFactory(new ConsoleLogger());

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();

                Console.WriteLine("Press F9 to exit.");
                var key = Console.ReadKey();

                while (key.Key != ConsoleKey.F9)
                {
                    key = Console.ReadKey();
                    // Application main LOOP
                }

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        public static async Task Run_Server_Without_Logging()
        {

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();

                Console.WriteLine("MQTT broker running at 127.0.0.1:1883");
                Console.WriteLine(".....................................");

                var key = Console.ReadKey();

                while (key.Key != ConsoleKey.F9)
                {
                    key = Console.ReadKey();
                    // Application main LOOP
                }

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
                Console.WriteLine("MQTT Broker: Server stopped at  " + DateTime.Now);
            }
        }

        public static async Task Validating_Connections()
        {
            /*
             * This sample starts a simple MQTT server which will check for valid credentials and client ID.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Setup connection validation before starting the server so that there is 
                // no change to connect without valid credentials.
                mqttServer.ValidatingConnectionAsync += e =>
                {
                    if (e.ClientId != "ValidClientId")
                    {
                        e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    }

                    if (e.UserName != "ValidUser")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    if (e.Password != "SecretPassword")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                };

                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }

        static async Task<MqttServer> StartMqttServer()
        {
            var mqttFactory = new MqttFactory();

            // Due to security reasons the "default" endpoint (which is unencrypted) is not enabled by default!
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder().WithDefaultEndpoint().Build();
            var server = mqttFactory.CreateMqttServer(mqttServerOptions);
            await server.StartAsync();
            return server;
        }

        class ConsoleLogger : IMqttNetLogger
        {
            readonly object _consoleSyncRoot = new();

            public bool IsEnabled => true;

            public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters, Exception? exception)
            {
                var foregroundColor = ConsoleColor.White;
                switch (logLevel)
                {
                    case MqttNetLogLevel.Verbose:
                        foregroundColor = ConsoleColor.White;
                        break;

                    case MqttNetLogLevel.Info:
                        foregroundColor = ConsoleColor.Green;
                        break;

                    case MqttNetLogLevel.Warning:
                        foregroundColor = ConsoleColor.DarkYellow;
                        break;

                    case MqttNetLogLevel.Error:
                        foregroundColor = ConsoleColor.Red;
                        break;
                }

                if (parameters?.Length > 0)
                {
                    message = string.Format(message, parameters);
                }

                lock (_consoleSyncRoot)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(message);

                    if (exception != null)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}
