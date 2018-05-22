using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIAggregator
{
    /// <summary>
    /// Implementacija agregatorske WCF sluzbe, bavi se prijemom podataka i skladistenjem u kolekciju i XML
    /// </summary>
    public class AggregatorManager : IAggregator
    {
        /// <summary>
        /// Primeti dodavanje agregatorskog koda; proveriti sa asistentom kako bi trebalo to da se sredi
        /// .pdf spominje listanje agregatora, ali kako sve skladistiti?
        /// Da li SystemManagement pamti?
        /// </summary>
        public AggregatorManager()
        {
            if (CheckIfDocumentExists())
            {
                //TODO:Uraditi deserijalizaciju
            }
        }

        public bool CheckIfDocumentExists()
        {
            //TODO: Implement
            return false;
        }

        public bool Connect(AMIMeasurement measurement)
        {
            Console.WriteLine("Received: " + measurement.ToString());
            if (Program.Message.Buffer.ContainsKey(measurement.DeviceCode))
            {
                return false;
            }
            else
            {
                Program.Message.Add(measurement);
                //UpdateLog(measurement)
                return true;
            }
        }

        public bool SendMeasurement(AMIMeasurement measurement)
        {
            Console.WriteLine("Received: " + measurement.ToString());
            Program.Message.Add(measurement);
            //UpdateLog(measurement)
            return true;
        }

        /*void UpdateLog()
        {
            //TODO: Implement
        }*/
    }
}
