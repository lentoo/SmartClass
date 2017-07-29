using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Actuators
{
    public class AirConditioning:SensorBase
    {
        public bool? IsOpen { get; set; }
        public string Value { get; set; }
        public string Model { get; set; }
        public int Speed { get; set; }

        public string SweepWind { get; set; }
    }
}
