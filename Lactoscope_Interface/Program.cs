using System;
using System.IO;
using System.Threading;
using Lactoscope_Interface.Models;

namespace Lactoscope_Interface
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Lactoscope Interface");

            // Create one instance of the Lactoscope Model
            Lactoscope lacto = new Lactoscope();

            // Read configuration from XML file.
            XML_Parser config = new XML_Parser();
            config.ReadFile("configuration.xml");

            lacto.Manufacturer = config.GetElementValue("/interface/device/manufacturer");
            lacto.Model = config.GetElementValue("/interface/device/model");
            lacto.SerialNumber = config.GetElementValue("/interface/device/serialnumber");

            Console.WriteLine("Manufacturer: " + lacto.Manufacturer);
            Console.WriteLine("Model: " + lacto.Model);
            Console.WriteLine("Serial N.: " + lacto.SerialNumber);
            Console.WriteLine(".....................................");

            string outputFolder = config.GetElementValue("/interface/device/outputfolder");
            bool runBroker = bool.Parse(config.GetElementValue("/interface/mqttbroker/local_broker/enabled"));
            bool brokerLogging = bool.Parse(config.GetElementValue("/interface/mqttbroker/local_broker/logging"));
            MqttBrokerProperties brokerConnection = new MqttBrokerProperties();

            brokerConnection.Address = config.GetElementValue("/interface/mqttbroker/remote_broker/address");
            brokerConnection.Port = int.Parse(config.GetElementValue("/interface/mqttbroker/remote_broker/port"));
            brokerConnection.Username = config.GetElementValue("/interface/mqttbroker/remote_broker/username");
            brokerConnection.Password = config.GetElementValue("/interface/mqttbroker/remote_broker/password");

            // Start MQTT broker as Thread if allowed in configuration file.
            if (runBroker)
            {
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    if (brokerLogging)
                    {
                        MQTT_Broker.Run_Server_With_Logging();
                    } else
                    {
                        MQTT_Broker.Run_Server_Without_Logging();
                    }           

                }).Start();
            }

            // Start FolderWatching
            FolderWatcher folder = new FolderWatcher(outputFolder,lacto, brokerConnection);

            while (true)
            {

            }

        }

    }
}
