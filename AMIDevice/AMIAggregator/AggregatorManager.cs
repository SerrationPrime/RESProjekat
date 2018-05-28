using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
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
                //Load();
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
                UpdateLog(measurement);
                return true;
            }
        }

        public bool SendMeasurement(AMIMeasurement measurement)
        {
            Console.WriteLine("Received: " + measurement.ToString());
            Program.Message.Add(measurement);
            UpdateLog(measurement);
            return true;
        }

        //Što se tiče poruke: to je Public.Message.Buffer, to koristi za ove metode

        /// <summary>
        /// poziva se kada se podaci posalju ka SM
        /// </summary>
        public static void ClearData()
        {
            if (File.Exists(Filename))
            {
                File.Delete(Filename);
            }
            Program.Message.Buffer.Clear();
        }

       
        void UpdateLog(AMIMeasurement measurement)
        {
            if (!File.Exists(Filename))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(Filename, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("AllMeasurements");

                    xmlWriter.WriteStartElement("AMIMeasurement");
                    xmlWriter.WriteElementString("DeviceCode", measurement.DeviceCode);
                    xmlWriter.WriteElementString("Timestamp", measurement.Timestamp.ToString());

                    xmlWriter.WriteStartElement("Measurement");
                    foreach(var amivp in measurement.Measurement)
                    {
                        xmlWriter.WriteStartElement("AMIValuePair");
                        xmlWriter.WriteElementString("Type", amivp.Type.ToString());
                        xmlWriter.WriteElementString("Value", amivp.Value.ToString());
                        //xmlWriter.WriteElementString("Timestamp",amivp.Timestamp.ToString()); //ovo je daleko bolje resenje, treba da se koristi amiSerializableBValue, ili da se doda timestamp unutar AmiValuePair. Onda uklanjam timestamp tag od gore. U svrhe crtanja dijagrama, ovo je jedino korektno resenje.
                        xmlWriter.WriteEndElement();    //za AMIValuePair
                    }
                    xmlWriter.WriteEndElement();    //za Measurement

                    xmlWriter.WriteEndElement();    //za AMIMeasurements
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                XDocument xDocument = XDocument.Load(Filename);
                XElement root = xDocument.Element("AllMeasurements");
                IEnumerable<XElement> rows = root.Descendants("AMIMeasurement");
                XElement firstRaw = rows.First();

                XElement element = new XElement("Measurement");
                foreach (var amivp in measurement.Measurement)
                {
                    element.Add(
                        new XElement("AMIValuePair", 
                            new XElement("Type", amivp.Type), 
                            new XElement("Value", amivp.Value))
                            );

                }

                firstRaw.AddBeforeSelf(
                    new XElement("AMIMeasurement",
                        new XElement("DeviceCode", measurement.DeviceCode),
                        new XElement("TimeStamp", measurement.Timestamp.ToString()),
                        new XElement("Measurement", element))
                    );
                
                xDocument.Save(Filename);
            }
        }
        
        void Load()
        {
            using (var stream = System.IO.File.OpenRead(Filename))
            {
                //ovde je potrena izmena, posto se u lokalnu bazu ne ucitava dictionary, vec lista merenja
                //kljuc je device code
                var serializer = new XmlSerializer(typeof(Dictionary<string,List<AMISerializableValue>>));
                Program.Message.Buffer = serializer.Deserialize(stream) as Dictionary<string, List<AMISerializableValue>>;
            }
        }
    }
}
