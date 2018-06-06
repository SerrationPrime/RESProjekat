using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMISystemManagementUI;
using AMICommons;

namespace WCFBasicTest
{
    [TestClass]
    public class SystemManagementManagerTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendMessageSMTestNullMeasurement()
        {
            var test = new SystemManagementManager();
            var testMessage = SetUpMessage();
            testMessage.Buffer = null;

            test.SendMessageToSystemManagement(testMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendMessageSMTestFaultyMeasurement()
        {
            var test = new SystemManagementManager();
            var testMessage = SetUpMessage();
            foreach (var message in testMessage.Buffer.Values)
            {
                message[0].Measurements.RemoveAt(0);
            }

            test.SendMessageToSystemManagement(testMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendMessageSMTestWrongType()
        {
            var test = new SystemManagementManager();
            var testMessage = SetUpMessage();
            testMessage.Buffer["100"][0].Measurements[0].Type = AMIMeasurementType.ReactivePower;
            test.SendMessageToSystemManagement(testMessage);
        }

        AggregatorMessage SetUpMessage()
        {
            AMIMeasurement measurement = new AMIMeasurement("100");
            AggregatorMessage message = new AggregatorMessage("Test1");
            message.Add(measurement);
            return message;
        }
    }
}
