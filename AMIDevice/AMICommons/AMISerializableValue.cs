using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Verzija liste AMIValuePair prosirena dodatkom vremenske oznake. Koristi je agregator za svoje poruke
    /// </summary>
    [Serializable]
    [DataContract]
    public class AMISerializableValue
    {
        [DataMember]
        public List<AMIValuePair> Measurements = new List<AMIValuePair>();
        [DataMember]
        public long Timestamp { get; set; }
 
        public AMISerializableValue(long timestamp, List<AMIValuePair> list)
        {
            Timestamp = timestamp;
            Measurements = list;
        }

        public AMISerializableValue()
        {

        }
    }
}
