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
        //Za rad sa vise uredjaja; bez ovoga, pokretanje nove instance AggregatorManager obara program jer se pokusava load
        //sa vec ucitanim recnikom
        private static bool isActive = false;

        /// <summary>
        /// Primeti dodavanje agregatorskog koda; proveriti sa asistentom kako bi trebalo to da se sredi
        /// .pdf spominje listanje agregatora, ali kako sve skladistiti?
        /// Da li SystemManagement pamti?
        /// </summary>
        public AggregatorManager()
        {
            if (File.Exists(Filename) && !isActive)
            {
                Load();
                isActive = true;
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

                   // xmlWriter.WriteStartElement("AMIMeasurement");
                    xmlWriter.WriteStartElement("DeviceCode");
                    xmlWriter.WriteAttributeString("value",measurement.DeviceCode);
                 
                    xmlWriter.WriteStartElement("Measurement");
                    xmlWriter.WriteElementString("Timestamp", measurement.Timestamp.ToString());
                    foreach (var amivp in measurement.Measurement)
                    {
                        xmlWriter.WriteStartElement("AMIValuePair");
                        xmlWriter.WriteElementString("Type", amivp.Type.ToString());
                        xmlWriter.WriteElementString("Value", amivp.Value.ToString());
                        //xmlWriter.WriteElementString("Timestamp",amivp.Timestamp.ToString()); //ovo je daleko bolje resenje, treba da se koristi amiSerializableBValue, ili da se doda timestamp unutar AmiValuePair. Onda uklanjam timestamp tag od gore. U svrhe crtanja dijagrama, ovo je jedino korektno resenje.
                        xmlWriter.WriteEndElement();    //za AMIValuePair
                    }
                    xmlWriter.WriteEndElement();    //za Measurement

                    xmlWriter.WriteEndElement();    //za DeviceCode
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                XDocument xDocument = XDocument.Load(Filename);
                XElement root = xDocument.Element("AllMeasurements");
                IEnumerable<XElement> deviceCodes = root.Descendants("DeviceCode");
                if (CheckIfDeviceIsInStorage(measurement.DeviceCode))
                {
                    var elementToAdd = new XElement("Measurement");
                    elementToAdd.Add(new XElement("Timestamp", measurement.Timestamp));

                    foreach (var amivp in measurement.Measurement)
                    {
                        XElement el2 = new XElement("AMIValuePair",
                                    new XElement("Type", amivp.Type),
                                    new XElement("Value", amivp.Value));
                        elementToAdd.Add(el2);
                    }

                    xDocument.Element("AllMeasurements").Elements("DeviceCode").First(node => node.Attribute("value").Value == measurement.DeviceCode).Add(elementToAdd);
                    xDocument.Save(Filename);
                }
                else
                {
                    XElement firstRow = deviceCodes.First();

                    XElement element = new XElement("Measurement",
                                       new XElement("Timestamp", measurement.Timestamp.ToString()));

                    foreach (var amivp in measurement.Measurement)
                    {
                        element.Add(
                            new XElement("AMIValuePair",
                                new XElement("Type", amivp.Type),
                                new XElement("Value", amivp.Value))
                                );

                    }

                    firstRow.AddBeforeSelf(
                        new XElement("DeviceCode", new XAttribute("value", measurement.DeviceCode),
                             element)
                        );

                    xDocument.Save(Filename);
                }                   
            }
        }

        bool CheckIfDeviceIsInStorage(string deviceCode)
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while (reader.Read())
                {
                    if (reader.Name == "DeviceCode" && (reader.GetAttribute("value")==deviceCode))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        void Load()
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                AMISerializableValue tempSerialisable = new AMISerializableValue();
                List<AMISerializableValue> tempList = new List<AMISerializableValue>();
                string currentDeviceCode = "";
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case ("DeviceCode"):
                            if (reader.NodeType == XmlNodeType.EndElement)
                                break;
                            currentDeviceCode = reader.GetAttribute("value");
                            Program.Message.Buffer.Add(currentDeviceCode, new List<AMISerializableValue>());
                            break;
                        case ("Timestamp"):
                            if (reader.NodeType == XmlNodeType.EndElement)
                                break;
                            reader.Read();
                            tempSerialisable.Timestamp = Int32.Parse(reader.Value);
                            break;
                        case ("AMIValuePair"):
                            if (reader.NodeType == XmlNodeType.EndElement)
                                break;
                            AMIValuePair temp = new AMIValuePair();
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            temp.Type = (AMIMeasurementType)Enum.Parse(typeof(AMIMeasurementType),reader.Value);
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            reader.Read();
                            temp.Value = Double.Parse(reader.Value);
                            tempSerialisable.Measurements.Add(temp);
                            break;
                        case ("Measurement"):
                            if (reader.NodeType != XmlNodeType.EndElement)
                                break;
                            Program.Message.Buffer[currentDeviceCode].Add(tempSerialisable);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
