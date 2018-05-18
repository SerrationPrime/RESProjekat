using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Verzija AMIValuePair prosirena dodatkom vremenske oznake. Koristi je agregator za svoje poruke
    /// Ne nasledjuje AMIValuePair zbog mogucih nejasnoca sa serijalizacijom
    /// </summary>
    [Serializable]
    public class AMISerializableValue
    {
        public long Timestamp { get; set; }
        public AMIMeasurementType Type { get; set; }
        public double Value { get; set; }

        public AMISerializableValue(long timestamp, AMIMeasurementType type, double value)
        {
            Timestamp = timestamp;
            Type = type;
            Value = value;
        }
    }
}
