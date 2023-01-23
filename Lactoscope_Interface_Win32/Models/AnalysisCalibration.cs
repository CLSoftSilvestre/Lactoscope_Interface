using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lactoscope_Interface_Win32.Models
{
    public class AnalysisCalibration
    {
        public string Datetime { get; set; }
        public string Slope { get; set; }
        public string Intercept { get; set; }
    }
}
