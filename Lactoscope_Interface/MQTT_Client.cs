using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using Lactoscope_Interface.Models;

namespace Lactoscope_Interface
{
   public static class MQTT_Client
    {
        public static async Task Publish_Lactoscope_Message(MqttBrokerProperties _brokerConnection, Lactoscope lacto)
        {
            /*
             * This sample pushes a simple application message including a topic and a payload.
             *
             * Always use builders where they exist. Builders (in this project) are designed to be
             * backward compatible. Creating an _MqttApplicationMessage_ via its constructor is also
             * supported but the class might change often in future releases where the builder does not
             * or at least provides backward compatibility where possible.
             */

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(_brokerConnection.Address,_brokerConnection.Port)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                // Publish Lactoscope properties Manufacturer
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope"+lacto.Id+"/manufacturer")
                    .WithPayload(lacto.Manufacturer)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Lactoscope properties Model
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/model")
                    .WithPayload(lacto.Model)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Lactoscope properties Serial Number
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/serialnumber")
                    .WithPayload(lacto.SerialNumber)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Analysis ID
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/id")
                    .WithPayload(lacto.Sample.Id)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Analysis Datetime
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/datetime")
                    .WithPayload(lacto.Sample.Datetime)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Analysis Product
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/product")
                    .WithPayload(lacto.Sample.Product)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Analysis Product Type
                applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/producttype")
                    .WithPayload(lacto.Sample.ProductType)
                    .Build();

                await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                // Publish Analysis properties data.
                int pCount = lacto.Sample.Properties.Count();

                for (int i = 0; i < pCount; i++)
                {
                    // Property Name
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "name")
                    .WithPayload(lacto.Sample.Properties[i].PropertyName)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property Description
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "description")
                    .WithPayload(lacto.Sample.Properties[i].Description)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property Units
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "units")
                    .WithPayload(lacto.Sample.Properties[i].Units)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property Value
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "average")
                    .WithPayload(lacto.Sample.Properties[i].Average)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property Std Deviation
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "stddev")
                    .WithPayload(lacto.Sample.Properties[i].StdDev)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Calibration Data

                    // Property calibration datetime
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "calibration/datetime")
                    .WithPayload(lacto.Sample.Properties[i].Calibration.Datetime)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property calibration slope
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "calibration/slope")
                    .WithPayload(lacto.Sample.Properties[i].Calibration.Slope)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    // Property calibration intercect
                    applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("lactoscope" + lacto.Id + "/sample/property" + i + "/" + "calibration/intercept")
                    .WithPayload(lacto.Sample.Properties[i].Calibration.Intercept)
                    .Build();

                    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                }

                await mqttClient.DisconnectAsync();

            }
        }
    }
}
