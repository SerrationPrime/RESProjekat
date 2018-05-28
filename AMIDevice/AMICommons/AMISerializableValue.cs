using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Verzija AMIValuePair prosirena dodatkom vremenske oznake. Koristi je agregator za svoje poruke
    /// Ne nasledjuje AMIValuePair zbog mogucih nejasnoca sa serijalizacijom
    /// </summary>
    [Serializable]
    [DataContract]
    public class AMISerializableValue
    {
        [DataMember]
        public long Timestamp { get; set; }
        [DataMember]
        public AMIMeasurementType Type { get; set; }
        [DataMember]
        public double Value { get; set; }

        public AMISerializableValue(long timestamp, AMIMeasurementType type, double value)
        {
            Timestamp = timestamp;
            Type = type;
            Value = value;
        }

        public AMISerializableValue()
        { }
    }
}
