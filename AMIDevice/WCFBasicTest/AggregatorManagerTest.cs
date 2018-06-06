using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AMIAggregator;
using AMICommons;

namespace WCFBasicTests
{
    [TestClass]
    public class AggregatorManagerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConnectTestNullMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.DeviceCode = null;
            test.Connect(measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConnectTestFaultyMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.Measurement.RemoveAt(0);
            test.Connect(measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConnectTestWrongTypeMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.Measurement[0].Type = AMIMeasurementType.ReactivePower;
            test.Connect(measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendMeasurementTestNullMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.DeviceCode = null;
            test.SendMeasurement(measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendMeasurementTestFaultyMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.Measurement.RemoveAt(0);
            test.SendMeasurement(measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendMeasurementTestWrongTypeMeasurement()
        {
            AggregatorManager test = new AggregatorManager();
            AMIMeasurement measurement = new AMIMeasurement();
            measurement.Measurement[0].Type = AMIMeasurementType.ReactivePower;
            test.SendMeasurement(measurement);
        }
    }
}
