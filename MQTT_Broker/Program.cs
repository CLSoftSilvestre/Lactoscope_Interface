using System;
using System.Threading.Tasks;

namespace MQTT_Broker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MQTT Broker Basic");
            await MQTT_Broker.Run_Server_Without_Logging();
        }
    }
}
