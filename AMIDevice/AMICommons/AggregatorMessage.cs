using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Predstavlja jednu poruku koju agregator salje System Managementu
    /// </summary>
    public class AggregatorMessage
    {
        /// <summary>
        /// Kod aggregatora, string iz razloga navedenih u AMIMeasurement.DeviceCode
        /// </summary>
        public string AggregatorCode { get; set; }
        // <summary>
        /// Privatno polje koje prati vreme merenja.
        /// Za zahtevani Unix timestamp, koristi se property Timestamp
        /// </summary>
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
        }

        /// <summary>
        /// Buffer koji se salje prema System Managementu
        /// .pdf kaze da bi elementi list trebala da bude tuple device code/lista merenja, ali nedostatak timestampa nema smisla.
        /// Ovo treba proveriti sa asistentom. Takodje, radije bih da je ova lista dictionary, ali pdf kaze lista
        /// </summary>
        List<Tuple<string, List<AMISerializableValue>>> Buffer = new List<Tuple<string, List<AMISerializableValue>>>();

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
            int index = Buffer.FindIndex(r => r.Item1 == measurement.DeviceCode);
            if (index!=-1)
            {
                foreach (var pair in measurement.Measurement)
                {
                    Buffer[index].Item2.Add(new AMISerializableValue(measurement.Timestamp, pair.Type, pair.Value));
                }
            }
            else
            {
                var NewElement=new Tuple<string, List<AMISerializableValue>>(measurement.DeviceCode, new List<AMISerializableValue>());
                foreach (var pair in measurement.Measurement)
                {
                    NewElement.Item2.Add(new AMISerializableValue(measurement.Timestamp, pair.Type, pair.Value));
                }
                Buffer.Add(NewElement);
            }
        }
    }
}
