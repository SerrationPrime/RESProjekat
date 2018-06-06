using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMICommons;

namespace AMICommonsTest
{
    [TestClass]
    public class AMISerialisableTest
    {
        [TestMethod]
        public void ConstructorTestNominal()
        {
            AMISerializableValue test = new AMISerializableValue(DateTimeOffset.Now.ToUnixTimeSeconds(), new AMIMeasurement().Measurement);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTestEmptyList()
        {
            AMISerializableValue test = new AMISerializableValue(DateTimeOffset.Now.ToUnixTimeSeconds(), new List<AMIValuePair>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConstructorTestInvalidList()
        {
            List<AMIValuePair> testList = new List<AMIValuePair> { new AMIValuePair(AMIMeasurementType.ActivePower, 100) };
            AMISerializableValue test = new AMISerializableValue(DateTimeOffset.Now.ToUnixTimeSeconds(),testList);
        }
    }
}
