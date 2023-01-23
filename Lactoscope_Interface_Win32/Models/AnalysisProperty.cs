using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lactoscope_Interface_Win32.Models
{
    public class AnalysisProperty
    {
        public string PropertyName { get; set; }
        public string Description { get; set; }
        public string Units { get; set; }
        public string Average { get; set; }
        public string StdDev { get; set; }

        public AnalysisCalibration Calibration { get; set; }
    }
}
