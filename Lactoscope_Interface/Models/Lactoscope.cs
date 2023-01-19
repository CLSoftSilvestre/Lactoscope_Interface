using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lactoscope_Interface.Models
{
    public class Lactoscope
    {
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public Sample Sample { get; set; }

    }
}
