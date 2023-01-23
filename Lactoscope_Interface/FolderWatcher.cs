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
            watcher.Error += new ErrorEventHandler(OnError);
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

        private void OnError(object source, ErrorEventArgs e)
        {
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                // txtResults.Text += "Error: File System Watcher internal buffer overflow at " + DateTime.Now + "\r\n";
                Console.WriteLine("Error: File System Watcher internal buffer overflow at " + DateTime.Now);
            }
            else
            {
                // txtResults.Text += "Error: Watched directory not accessible at " + DateTime.Now + "\r\n";
                Console.WriteLine("Error: Watched directory not accessible at " + DateTime.Now);
            }
            NotAccessibleError((FileSystemWatcher)source, e);
        }

        void NotAccessibleError(FileSystemWatcher source, ErrorEventArgs e)
        {
            int iMaxAttempts = 120;
            int iTimeOut = 5000;
            int i = 0;
            while ((!Directory.Exists(source.Path) || source.EnableRaisingEvents == false) && i < iMaxAttempts)
            {
                i += 1;
                try
                {
                    source.EnableRaisingEvents = false;
                    if (!Directory.Exists(source.Path))
                    {
                        // MyEventLog.WriteEntry("Directory Inaccessible " + source.Path + " at " + DateTime.Now.ToString("HH:mm:ss"));
                        Console.WriteLine("Directory Inaccessible " + source.Path + " at " + DateTime.Now.ToString("HH:mm:ss"));
                        Thread.Sleep(iTimeOut);
                    }
                    else
                    {
                        // ReInitialize the Component
                        source.Dispose();
                        source = null;
                        source = new System.IO.FileSystemWatcher();
                        ((System.ComponentModel.ISupportInitialize)(source)).BeginInit();
                        source.EnableRaisingEvents = true;
                        source.Filter = "*.xml";
                        source.Path = OutputFolder;
                        source.NotifyFilter = System.IO.NotifyFilters.FileName;
                        source.Created += new System.IO.FileSystemEventHandler(OnCreated);
                        source.Error += new ErrorEventHandler(OnError);
                        ((System.ComponentModel.ISupportInitialize)(source)).EndInit();
                        Console.WriteLine("Try to Restart RaisingEvents Watcher at " + DateTime.Now.ToString("HH:mm:ss"));
                        // MyEventLog.WriteEntry("Try to Restart RaisingEvents Watcher at " + DateTime.Now.ToString("HH:mm:ss"));
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine("Error trying Restart Service " + error.StackTrace + " at " + DateTime.Now.ToString("HH:mm:ss"));
                    // MyEventLog.WriteEntry("Error trying Restart Service " + error.StackTrace + " at " + DateTime.Now.ToString("HH:mm:ss"));
                    source.EnableRaisingEvents = false;
                    Thread.Sleep(iTimeOut);
                }
            }
        }

    }
}
