using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    /// <summary>
    /// Vrsta merenja izvrsenog na uredjaju
    /// </summary>
    public enum AMIMeasurementType { Voltage, Current, ActivePower, ReactivePower}
    /// <summary>
    /// Merenje jednog parametra na AMI uredjaju
    /// </summary>
    public class AMIValuePair
    {
        public AMIMeasurementType Type { get; set; }
        public double Value { get; set; }

        public AMIValuePair(AMIMeasurementType type, double value)
        {
            Type = type;
            Value = value;
        }
    }
    /// <summary>
    /// Klasa koja predstavlja jedno, specificno merenje koje AMI uredjaj salje na agregator.
    /// </summary>
    /// 
    //Javna polja i propertije bi verovatno najpametnije bilo oznaciti kao private ako ovu klasu ne budemo koristili van konstruktora
    public class AMIMeasurement
    {
        /// <summary>
        /// Konstante koriscene za generisanje nasumicnih merenja
        /// </summary>
        const int UpperRandomMeasurementLimit = 300;
        const int LowerRandomMeasurementLimit = 100;
        const int PerturbationLimit = 10;

        /// <summary>
        /// Nasumicni generator brojeva koriscen u nekim konstruktorima i metodama
        /// </summary>
        Random RNGGen = new Random();

        /// <summary>
        /// Jedinstven kod AMI uredjaja. Znam da si ti spominjala int, ali ovako nesto u praksi bi verovatno bilo string,
        /// na primer, "10-16612A220V-01" ili tako nesto
        /// </summary>
        public string DeviceCode { get; set; }

        /// <summary>
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
        /// Samo merenje: cetiri tupla koja oznacavaju cetiri merene velicine
        /// </summary>
        public List<AMIValuePair> Measurement = new List<AMIValuePair>();

        /// <summary>
        /// Prazan konstruktor za svrhe testiranja, daje nasumican tip merenja sa nasumicnim tipom merenja i nasumicnim uredjajem
        /// </summary>
        public AMIMeasurement()
        {
            DeviceCode = this.GetHashCode().ToString();
            MeasurementTime = DateTimeOffset.Now;
            //Proveri komentar metode
            GenerateMeasurementValues();
        }

        /// <summary>
        /// Test konstruktor, uzima ime uredjaja i generise nasumicna merenja za njega
        /// </summary>
        /// <param name="id"></param>
        public AMIMeasurement(string id)
        {
            DeviceCode = id;
            MeasurementTime = DateTimeOffset.Now;
            GenerateMeasurementValues();
        }

        public AMIMeasurement(string id, double voltageValue, double currentValue, double activePowerValue, double reactivePowerValue)
        {
            DeviceCode = id;
            MeasurementTime = DateTimeOffset.Now;
            Measurement.Add(new AMIValuePair(AMIMeasurementType.Voltage, voltageValue));
            Measurement.Add(new AMIValuePair(AMIMeasurementType.Current, currentValue));
            Measurement.Add(new AMIValuePair(AMIMeasurementType.ActivePower, activePowerValue));
            Measurement.Add(new AMIValuePair(AMIMeasurementType.ReactivePower, reactivePowerValue));
        }

        /// <summary>
        /// Nasumicno generise sveze vrednosti merenja za klasu. Mogu biti pozitivne ili negative, u opsegu od (+/-)LowerRandomMeasurementLimit do UpperRandomMeasurementLimit
        /// </summary>
        /// <param name="RNGGen"></param>
        private void GenerateMeasurementValues()
        {
            for (int i = 0; i < 4; i++)
                //Ok, ova linija je malo gadna
                //Idemo redom kroz sve tipove merenja i generisem po jednu vrednost za svaku
                //(RNGGen.Next(0,1)*2-1) znaci da nasumice generisem +1 ili -1, sto daje podrsku za negativne vrednosti
                Measurement.Add(new AMIValuePair((AMIMeasurementType)i, (RNGGen.Next(0,2)*2-1)*(RNGGen.Next(LowerRandomMeasurementLimit, UpperRandomMeasurementLimit)+RNGGen.NextDouble())));
        }

        /// <summary>
        /// Menja merenja za maksimalno PerturbationLimit u bilo kom smeru i osvezava vremensku oznaku
        /// </summary>
        public void PerturbValues()
        {
            for (int i=0; i < 4; i++)
            {
               Measurement[i].Value += ((RNGGen.Next(0, 2) * 2 - 1) * (RNGGen.Next(0, PerturbationLimit) + RNGGen.NextDouble()));
               MeasurementTime = DateTimeOffset.Now;
            }
        }

        public override string ToString()
        {
            return String.Format("V:{0} A:{1} P:{2} Q:{3}", Measurement[0].Value, Measurement[1].Value, Measurement[2].Value, Measurement[3].Value);
        }
    }
}
