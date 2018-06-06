using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMICommons;
using System.Collections.Generic;

namespace AMICommonsTest
{
    [TestClass]
    public class AggregatorMessageTest
    {
        [TestMethod]
        public void ConstructorTestNominal()
        {
            string testVal = "Aggregator7";

            AggregatorMessage test = new AggregatorMessage(testVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestEmpty()
        {
            string testVal = "";

            AggregatorMessage test = new AggregatorMessage(testVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestWhitespace()
        {
            string testVal = " ";

            AggregatorMessage test = new AggregatorMessage(testVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestNull()
        {
            string testVal = null;

            AggregatorMessage test = new AggregatorMessage(testVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidAddTestBadCount()
        {
            AggregatorMessage testVal = new AggregatorMessage("testAgg");
            AMIMeasurement faultyMeasurement = new AMIMeasurement();
            //Ovo apsolutno nikada ne bi trebalo da se desi, ali je dodato zbog pokrivenosti koda
            faultyMeasurement.Measurement = new List<AMIValuePair>();
            testVal.Add(faultyMeasurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidAddTestBadTypes()
        {
            AggregatorMessage testVal = new AggregatorMessage("testAgg");
            AMIMeasurement faultyMeasurement = new AMIMeasurement();
            //Ni ovo ne bi trebalo da se desi, ali je dodato zbog pokrivenosti koda
            faultyMeasurement.Measurement[0].Type = AMIMeasurementType.ReactivePower;
            testVal.Add(faultyMeasurement);
        }
    }
}
