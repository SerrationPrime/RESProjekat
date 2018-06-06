using AMICommons;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMICommonsTest
{
    [TestClass]
    public class AMIMeasurementTest
    {
        [TestMethod]
        public void ConstructorTestNominal()
        {
            string tVal = "111";

            AMIMeasurement m1 = new AMIMeasurement();
            AMIMeasurement m2 = new AMIMeasurement(tVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestEmpty()
        {
            string tVal = "";
            AMIMeasurement test = new AMIMeasurement(tVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestWhitespace()
        {
            string tVal = " ";

            AMIMeasurement test = new AMIMeasurement(tVal);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorTestNull()
        {
            string tVal = null;

            AMIMeasurement test = new AMIMeasurement(tVal);
        }

        //Sve metode su bez parametara, očekuje se normalan rad
        [TestMethod]
        public void NominalMethodTest()
        {
            AMIMeasurement guinea = new AMIMeasurement();
            guinea.PerturbValues();
            Assert.AreEqual(false, guinea.IsNullOrEmpty());
        }
    }
}
