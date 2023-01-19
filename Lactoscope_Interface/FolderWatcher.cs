using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Lactoscope_Interface.Models;
using System.Threading;

namespace Lactoscope_Interface
{
    class FolderWatcher
    {
        FileSystemWatcher watcher;
        /// XML_Parser reader;
        Lactoscope lacto;

        public string Filename { get; set; }
        public string Path { get; set; }
        public string OutputFolder { get; set; }
        public MqttBrokerProperties BrokerConnection { get; set; }

        public FolderWatcher(string _outputFolder, Lactoscope _lacto, MqttBrokerProperties _brokerConnection)
        {
            lacto = new Lactoscope();
            lacto = _lacto;
            OutputFolder = _outputFolder;
            BrokerConnection = _brokerConnection;
            watch();
        }

        private void watch()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = OutputFolder;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.xml";
            watcher.Created += new FileSystemEventHandler(OnCreated);
            watcher.EnableRaisingEvents = true;
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            
            try
            {
                watcher.EnableRaisingEvents = false;

                Thread.Sleep(500);

                XML_Parser reader = new XML_Parser();
                reader.ReadFile(e.FullPath);

                Console.WriteLine(DateTime.Now + " - Lactoscope Interface - Processing output file -> " + e.Name);

                Sample newSample = new Sample();

                newSample.Id = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS", "id");
                newSample.Datetime = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS", "datetime");
                newSample.Product = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/SAMPLE", "product");
                newSample.ProductType = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/MODEL", "name");
                newSample.Properties = new List<AnalysisProperty>();

                // Get Analysis Properties and Calibration data for each property
                // int pCount = int.Parse(reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/STATS/MEAN/PREDICTIONS", "itemcount"));
                // TODO: Set manually to 5 properties... Need to convert do dymanic according file...

                for (int i = 1; i <= 5; i++)
                {
                    AnalysisProperty prop = new AnalysisProperty();
                    prop.PropertyName = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]", "name");
                    prop.Units = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]", "units");
                    prop.Description = reader.GetElementValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]/COMMENT");
                    prop.Average = reader.GetElementValue("/ANALYSISLIST/ANALYSIS/STATS/MEAN/PREDICTIONS/PREDICTION[" + i + "]");
                    prop.StdDev = reader.GetElementValue("/ANALYSISLIST/ANALYSIS/STATS/STDEV/PREDICTIONS/PREDICTION[" + i + "]");

                    AnalysisCalibration propCal = new AnalysisCalibration();
                    propCal.Datetime = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]/CALIBRATION", "datetime");
                    propCal.Slope = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]/CALIBRATION", "slope");
                    propCal.Intercept = reader.GetAttributeValue("/ANALYSISLIST/ANALYSIS/PRODUCT/COMPONENTS/COMPONENT[" + i + "]/CALIBRATION", "intercept");

                    prop.Calibration = propCal;

                    newSample.Properties.Add(prop);
                }

                lacto.Sample = newSample;

                MQTT_Client.Publish_Lactoscope_Message(BrokerConnection, lacto);

            }
            finally
            {
                watcher.EnableRaisingEvents = true;
            }

        }
    }
}
