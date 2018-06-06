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
            Measurements = list ?? throw new ArgumentNullException();
            if (!IsValid())
            {
                throw new ArgumentException();
            }
            Timestamp = timestamp;
           
        }
        public AMISerializableValue()
        {

        }

        /// <summary>
        /// Vrsi identicnu funkcionalnost kao AMIMeasurement.IsValid()
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            if (Measurements.Count != Enum.GetNames(typeof(AMIMeasurementType)).Count()*AMIMeasurement.MeasurementPairsPerType)
                return false;
            foreach (AMIMeasurementType type in Enum.GetValues(typeof(AMIMeasurementType)))
            {
                int typeCount = 0;
                foreach (var pair in Measurements)
                {
                    if (pair.Type == type)
                    {
                        typeCount++;
                    }
                }
                if (typeCount != AMIMeasurement.MeasurementPairsPerType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
