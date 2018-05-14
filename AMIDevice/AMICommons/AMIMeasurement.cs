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
        public List<Tuple<AMIMeasurementType, double>> Measurement = new List<Tuple<AMIMeasurementType, double>>();

        /// <summary>
        /// Prazan konstruktor za svrhe testiranja, daje nasumican tip merenja sa nasumicnim tipom merenja i nasumicnim uredjajem
        /// </summary>
        public AMIMeasurement()
        {
            Random RNGGen = new Random();
            DeviceCode = "Device" + RNGGen.Next().ToString();
            MeasurementTime = DateTimeOffset.Now;
            //Proveri komentar metode
            GenerateMeasurementValues(RNGGen);
        }

        /// <summary>
        /// Test konstruktor, uzima ime uredjaja i generise nasumicna merenja za njega
        /// </summary>
        /// <param name="id"></param>
        public AMIMeasurement(string id)
        {
            DeviceCode = id;
            Random RNGGen = new Random();
            MeasurementTime = DateTimeOffset.Now;
            GenerateMeasurementValues(RNGGen);
        }

        public AMIMeasurement(string id, AMIMeasurementType type, double voltageValue, double currentValue, double activePowerValue, double reactivePowerValue)
        {
            DeviceCode = id;
            MeasurementTime = DateTimeOffset.Now;
            Measurement.Add(new Tuple<AMIMeasurementType, double>(type, voltageValue));
            Measurement.Add(new Tuple<AMIMeasurementType, double>(type, currentValue));
            Measurement.Add(new Tuple<AMIMeasurementType, double>(type, activePowerValue));
            Measurement.Add(new Tuple<AMIMeasurementType, double>(type, reactivePowerValue));
        }

        private void GenerateMeasurementValues(Random RNGGen)
        {
            for (int i = 0; i < 3; i++)
                //Ok, ova linija je malo gadna
                //Idemo redom kroz sve tipove merenja i generisem po jednu vrednost za svaku
                //(RNGGen.Next(0,1)*2-1) znaci da nasumice generisem +1 ili -1, sto daje podrsku za negativne vrednosti
                Measurement.Add(new Tuple<AMIMeasurementType, double>((AMIMeasurementType)i, (RNGGen.Next(0,1)*2-1)*RNGGen.Next(LowerRandomMeasurementLimit, UpperRandomMeasurementLimit)));
        }
    }
}
