using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Xml;
using System.Xml.Linq;

namespace AMIAggregator
{
    /// <summary>
    /// Implementacija agregatorske WCF sluzbe, bavi se prijemom podataka i skladistenjem u kolekciju i XML
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class AggregatorManager : IAggregator
    {
        //Kako podržavamo više različitih agregatora? Svakom treba sopstveni fajl
        //Potrebno je napraviti zaseban .exe za svaki
        private static string Filename="localStorage.xml";
        //Za rad sa vise uredjaja; bez ovoga, pokretanje nove instance AggregatorManager obara program jer se pokusava load
        //sa vec ucitanim recnikom
        public static bool isActive = false;

        /// <summary>
        /// Pozvano pri svakoj konekciji sa uređaja, inicijalizuje poruku za SystemManagement
        /// </summary>
        public AggregatorManager()
        {
            if (File.Exists(Filename) && !isActive)
            {
                Load(); 
            }
            isActive = true;
        }

        public bool Connect(AMIMeasurement measurement)
        {
            CheckMeasurement(measurement);

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
            CheckMeasurement(measurement);

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

       
        /// <summary>
        /// Serijalizacija poruke, uradena ručno zbog kompleksne strukture podataka
        /// </summary>
        /// <param name="measurement"></param>
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

                    xmlWriter.WriteStartElement("DeviceCode");
                    xmlWriter.WriteAttributeString("value",measurement.DeviceCode);
                 
                    xmlWriter.WriteStartElement("Measurement");
                    xmlWriter.WriteElementString("Timestamp", measurement.Timestamp.ToString());
                    foreach (var amivp in measurement.Measurement)
                    {
                        xmlWriter.WriteStartElement("AMIValuePair");
                        xmlWriter.WriteElementString("Type", amivp.Type.ToString());
                        xmlWriter.WriteElementString("Value", amivp.Value.ToString());
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

        /// <summary>
        /// Proverava postojanje deviceCode-a u skladistu sa vrednosti deviceCode
        /// </summary>
        /// <param name="deviceCode"></param>
        /// <returns>True ako deviceCode postoji u lokalnom skladistu</returns>
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

        /// <summary>
        /// Validacija prosleđenog merenja, baca izuzetak u neočekivanom slučaju
        /// </summary>
        /// <param name="measurement"></param>
        void CheckMeasurement(AMIMeasurement measurement)
        {
            if (measurement.IsNullOrEmpty())
            {
                throw new ArgumentNullException();
            }
            if (!measurement.IsValid())
            {
                throw new ArgumentException("The list of measurements in the passed object is improperly formed.");
            }
        }

        /// <summary>
        /// Deserijalizacija iz lokalnog skladišta, pozvana pri inicijalnom pokretanju agregatora u okviru praznog konstruktora i WCF protokola:
        /// WCF kreira instancu svake implementacije WCF usluge pri pozivu ChannelFactory.CreateChannel().
        /// </summary>
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
                            if (!tempSerialisable.IsValid())
                                throw new Exception("Something went wrong with loading the .xml file : SerialisableValue has this many pairs: " + tempSerialisable.Measurements.Count);
                            Program.Message.Buffer[currentDeviceCode].Add(tempSerialisable);
                            tempSerialisable = new AMISerializableValue();
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
