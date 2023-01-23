using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Lactoscope_Interface_Win32.Models;

namespace Lactoscope_Interface_Win32
{
    public partial class Form1 : Form
    {
        Lactoscope lacto = new Lactoscope();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Read configuration from XML file.
            XML_Parser config = new XML_Parser();
            config.ReadFile("configuration.xml");

            lacto.Id = config.GetElementValue("/interface/device/deviceid");
            numUpDownId.Value = decimal.Parse(lacto.Id);

            lacto.Manufacturer = config.GetElementValue("/interface/device/manufacturer");
            txtManufacturer.Text = config.GetElementValue("/interface/device/manufacturer");

            lacto.Model = config.GetElementValue("/interface/device/model");
            txtModel.Text = config.GetElementValue("/interface/device/model");

            lacto.SerialNumber = config.GetElementValue("/interface/device/serialnumber");
            txtSerialNumber.Text = config.GetElementValue("/interface/device/serialnumber");

            txtOutputFolder.Text = config.GetElementValue("/interface/device/outputfolder");
            fsWatch.Path = txtOutputFolder.Text;

        }

        private void fsWatch_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            Thread.Sleep(1000);

            // Read Analysis data
            XML_Parser reader = new XML_Parser();
            reader.ReadFile(e.FullPath);

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

            ConsoleOutput("Analysing file " + e.Name);

            propertyGrid1.SelectedObject = lacto.Sample;
        }

        private void fsWatch_OnError(object source, System.IO.ErrorEventArgs e)
        {
            txtOutput.AppendText(DateTime.Now + " - Error: " + e.ToString());
        }

        private void ConsoleOutput(string info)
        {
            txtOutput.AppendText(DateTime.Now + " - " + info + System.Environment.NewLine);
        }
    }
}
