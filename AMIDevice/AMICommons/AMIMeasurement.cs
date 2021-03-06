﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
    [DataContract]
    public class AMIValuePair
    {
        [DataMember]
        public AMIMeasurementType Type { get; set; }
        [DataMember]
        public double Value { get; set; }
        
        public AMIValuePair()
        {

        }

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
    [DataContract]
    public class AMIMeasurement
    {
        /// <summary>
        /// Konstante koriscene za generisanje nasumicnih merenja
        /// </summary>
        const int UpperRandomMeasurementLimit = 300;
        const int LowerRandomMeasurementLimit = 100;
        const int PerturbationLimit = 10;

        /// <summary>
        /// Oznacava koliko svaki AMIMeasurementType poseduje merenja u Measurement polju ove klase. Slicno vazi za AMISerialisableValue, i ona takodje koristi ovu const.
        /// </summary>
        public const int MeasurementPairsPerType= 1;

        /// <summary>
        /// Nasumicni generator brojeva koriscen u nekim konstruktorima i metodama
        /// </summary>
        Random RNGGen = new Random();

        /// <summary>
        /// Jedinstven kod AMI uredjaja. Znam da si ti spominjala int, ali ovako nesto u praksi bi verovatno bilo string,
        /// na primer, "10-16612A220V-01" ili tako nesto
        /// </summary>
        [DataMember]
        public string DeviceCode { get; set; }

        /// <summary>
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
        /// Samo merenje: cetiri tupla koja oznacavaju cetiri merene velicine
        /// </summary>
        [DataMember]
        public List<AMIValuePair> Measurement = new List<AMIValuePair>();

        /// <summary>
        /// Prazan konstruktor za svrhe testiranja, daje nasumican tip merenja sa nasumicnim tipom merenja i nasumicnim uredjajem
        /// </summary>
        public AMIMeasurement()
        {
           //Hash kod funkcije zavisi od njenog sadržaja
           //Stoga, potrebno je generisati neki nasumični objekat da bi hash bio nasumičan
            DeviceCode = Guid.NewGuid().GetHashCode().ToString();
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
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException();
            DeviceCode = id;
            MeasurementTime = DateTimeOffset.Now;
            GenerateMeasurementValues();
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


        /// <summary>
        /// Funkcija za kontrolu normalnosti merenja.
        /// </summary>
        /// <returns>True u slucaju da je ijedno polje neinicijalizovano, ili lista merenja nije u pravom obliku</returns>
        public bool IsNullOrEmpty()
        {
            return String.IsNullOrEmpty(DeviceCode) || MeasurementTime == default(DateTimeOffset);
        }
        public override string ToString()
        {
            return String.Format("V:{0} A:{1} P:{2} Q:{3}", Measurement[0].Value, Measurement[1].Value, Measurement[2].Value, Measurement[3].Value);
        }


        /// <summary>
        /// Metoda za proveru da li je polje Measurement dobro formirano. Ovo polje treba da ima po MeasurementPairPerType
        /// AMIValuePair-ova za svaki enum unutar AMIMeasurementType.
        /// </summary>
        /// <returns>True u slucaju da su navedeni uslovi zadovoljeni.</returns>
        public bool IsValid()
        {
            if (Measurement.Count != Enum.GetNames(typeof(AMIMeasurementType)).Count()*MeasurementPairsPerType)
                return false;
            foreach (AMIMeasurementType type in Enum.GetValues(typeof(AMIMeasurementType)))
            {
                int typeCount = 0;
                foreach(var pair in Measurement)
                {
                    if (pair.Type == type)
                    {
                        typeCount++;
                    }     
                }
                if (typeCount != MeasurementPairsPerType)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
