using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lactoscope_Interface_Win32.Models
{
    public class Sample
    {
        public string Id { get; set; }
        public string Datetime { get; set; }
        public string Product { get; set; }
        public string ProductType { get; set; }
        public List<AnalysisProperty> Properties { get; set; }
    }
}
