using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Predstavlja jednu poruku koju agregator salje System Managementu
    /// </summary>
    [DataContract]
    public class AggregatorMessage
    {
        /// <summary>
        /// Konstante za komunikaciju
        /// </summary>
        public const string Port = "13375";
        public const string AggregatorServiceName = "AggregatorManager";

        public const string SysEndpointName = "SystemManagement";
        public const string SysPort = "23111";

        /// <summary>
        /// Kod aggregatora, string iz razloga navedenih u AMIMeasurement.DeviceCode
        /// </summary>
        [DataMember]
        public string AggregatorCode { get; set; }
        // <summary>
        /// Privatno polje koje prati vreme merenja.
        /// Za zahtevani Unix timestamp, koristi se property Timestamp
        /// </summary>
        [DataMember]
        private DateTimeOffset MeasurementTime;

        /// <summary>
        /// 64-bitna Unix vremenska oznaka za trenutak merenja
        /// </summary>
        
        public long Timestamp
        {
            get
            {
                return MeasurementTime.ToUnixTimeSeconds();
            }
            set
            {
                MeasurementTime = DateTimeOffset.FromUnixTimeSeconds(value);
            }
        }

        /// <summary>
        /// Buffer koji se salje prema System Managementu
        /// .pdf kaze da bi elementi list trebala da bude tuple device code/lista merenja, ali nedostatak timestampa nema smisla.
        /// Ipak sam odlucio da uradim implementaciju kao listu
        /// </summary>
        [DataMember]
        public Dictionary<string, List<AMISerializableValue>> Buffer = new Dictionary<string, List<AMISerializableValue>>();

        /// <summary>
        /// Konstruktor koji kreira AggregatorMessage spreman za popunjavanje
        /// </summary>
        public AggregatorMessage(string id)
        {
            AggregatorCode = id;
            //Mozda necemo slati poruku u istom trenutku kada konstruisemo
            //Prosirenja ce biti uradjena po potrebi
            MeasurementTime = DateTimeOffset.Now;
        }

        /// <summary>
        /// Metoda koja popunjava buffer sa merenjem zavisno od id-a uredjaja
        /// Verovatno ce biti menjano
        /// </summary>
        /// <param name="deviceID">Kod AMI uredjaja</param>
        public void Add(AMIMeasurement measurement)
        {
            if (Buffer.ContainsKey(measurement.DeviceCode))
            {
                List<AMIValuePair> elementList = new List<AMIValuePair>();
                foreach (var pair in measurement.Measurement)
                {
                    elementList.Add(pair);
                }
                AMISerializableValue toAdd = new AMISerializableValue(measurement.Timestamp, elementList);
            }
            else
            {
                var NewList = new List<AMISerializableValue>();
                var elementList = new List<AMIValuePair>();
                foreach (var pair in measurement.Measurement)
                {
                    elementList.Add(pair);
                }
                NewList.Add(new AMISerializableValue(measurement.Timestamp, elementList));
                Buffer.Add(measurement.DeviceCode,NewList);
            }
        }
    }
}
