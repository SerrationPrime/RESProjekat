using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AMIAggregator
{
    /// <summary>
    /// Implementacija agregatorske WCF sluzbe, bavi se prijemom podataka i skladistenjem u kolekciju i XML
    /// </summary>
    public class AggregatorManager : IAggregator
    {
        //Kako podržavamo više različitih agregatora? Svakom treba sopstveni fajl
        //Verovatno ćemo i ovo morati preko configa
        //Verovatno namestiti i singleton, nema smisla da više managera radi konkurentno
        private static string Filename="localStorage.xml";

        /// <summary>
        /// Primeti dodavanje agregatorskog koda; proveriti sa asistentom kako bi trebalo to da se sredi
        /// .pdf spominje listanje agregatora, ali kako sve skladistiti?
        /// Da li SystemManagement pamti?
        /// </summary>
        public AggregatorManager()
        {
            if (File.Exists(Filename))
            {
                Load();
            }
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

        //Što se tiče poruke: to je Public.Message.Buffer, to koristi za ove metode

        /// <summary>
        /// Poziva se kada se podaci posalju ka SM, brise skladistene podatke u agregatoru
        /// </summary>
        public static void ClearData()
        {
            if (File.Exists(Filename))
            {
                File.Delete(Filename);
            }
            Program.Message.Buffer.Clear();
        }

        //Problem sa tvojim pristupom: serijalizuješ sve od jednom, umesto jedan po jedan; Ako program padne, gubiš sva merenja
        void UpdateLog()
        {
            throw new NotImplementedException();
        }
        
        void Load()
        {
            using (var stream = System.IO.File.OpenRead(Filename))
            {
                var serializer = new XmlSerializer(typeof(Dictionary<string, List<AMISerializableValue>>));
                Program.Message.Buffer = serializer.Deserialize(stream) as Dictionary<string, List<AMISerializableValue>>;
            }
        }
    }
}
