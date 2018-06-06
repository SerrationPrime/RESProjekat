using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AMISystemManagementUI
{
    class SystemManagementManager : IMessageForSystemManagement
    {
        private string Filename = "globalStorage.xml";
        public bool SendMessageToSystemManagement(AggregatorMessage message)
        {
            MainWindow.LastAggregatorMessage = message;
            //Ovde radimo serijalizaciju? Mozda ni ne treba skladistiti LastAggregatorMessage, ali je korisno za debug
            UpdateLog(message);
            return true;
        }

        public void UpdateLog(AggregatorMessage message)
        {
            if (message.Buffer.Count == 0)
                return;
            if(!File.Exists(Filename))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                //xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(Filename, xmlWriterSettings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("AllAggregatorMeasurements");

                    xmlWriter.WriteStartElement("AggregatorCode");
                    xmlWriter.WriteAttributeString("value", message.AggregatorCode);
                    //xmlWriter.WriteStartElement("AggregatorMeasurement");
                    //xmlWriter.WriteElementString("AggregatorTimestamp", message.Timestamp.ToString());
                    foreach (var devCode in message.Buffer.Keys)
                    {
                        xmlWriter.WriteStartElement("DeviceCode");
                        xmlWriter.WriteAttributeString("value", devCode);
                        foreach (var amisv in message.Buffer[devCode])    // za svaki element u listi amiSeriaValue
                        {
                            xmlWriter.WriteStartElement("Measurement");
                            xmlWriter.WriteElementString("Timestamp", amisv.Timestamp.ToString()); //pisi kada su izmerena sva merenja iz sledece liste
                            foreach (var amivp in amisv.Measurements)  //prodji kroz listu svih merenja tipa amiValuePair, vezanih za ovo vreme
                            {
                                xmlWriter.WriteStartElement("AMIValuePair");
                                xmlWriter.WriteElementString("Type", amivp.Type.ToString());
                                xmlWriter.WriteElementString("Value", amivp.Value.ToString());
                                xmlWriter.WriteEndElement();    //za AMIValuePair
                            }
                            xmlWriter.WriteEndElement(); //za Measurement
                        }
                        xmlWriter.WriteEndElement();    //za DeviceCode
                    }

                    //xmlWriter.WriteEndElement();    //za AggregatorMeasurement
                    xmlWriter.WriteEndElement();    //za AggregatorCode
                    xmlWriter.WriteEndElement();    //za AllAggregatorMeasurements
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }                      
            }
            else
            {
                XDocument xDocument = XDocument.Load(Filename);
                XElement root = xDocument.Element("AllAggregatorMeasurements");
                IEnumerable<XElement> aggregatorCodes = root.Descendants("AggregatorCode");
                if (CheckIfAggregatorIsInStorage(message.AggregatorCode))
                {
                   // var elementToAdd = new XElement("AggregatorMeasurement");
                    IEnumerable<XElement> deviceCodes = aggregatorCodes.First(x => x.Attribute("value").Value==message.AggregatorCode).Descendants("DeviceCode");
                    //elementToAdd.Add(new XElement("AggregatorTimestamp", message.Timestamp));
                    foreach (var devCode in message.Buffer.Keys)
                    {
                        if (CheckIfDeviceIsInStorage(devCode))
                        {
                            foreach (var measurement in message.Buffer[devCode])
                            {
                                var elementToAdd2 = new XElement("Measurement");
                                elementToAdd2.Add(new XElement("Timestamp", measurement.Timestamp));

                                foreach (var amivp in measurement.Measurements)
                                {
                                    XElement el2 = new XElement("AMIValuePair",
                                                new XElement("Type", amivp.Type),
                                                new XElement("Value", amivp.Value));
                                    elementToAdd2.Add(el2);
                                }
                                deviceCodes.First(node => node.Attribute("value").Value == devCode).Add(elementToAdd2);
                               
                            }
                            xDocument.Save(Filename);

                        }
                        else
                        {
                            XElement firstRow = deviceCodes.First();
                            foreach (var measurement in message.Buffer[devCode])
                            {
                                XElement element = new XElement("Measurement",
                                                   new XElement("Timestamp", measurement.Timestamp.ToString()));

                                foreach (var amivp in measurement.Measurements)
                                {
                                    element.Add(
                                        new XElement("AMIValuePair",
                                            new XElement("Type", amivp.Type),
                                            new XElement("Value", amivp.Value))
                                            );

                                }

                                firstRow.AddBeforeSelf(
                                    new XElement("DeviceCode", new XAttribute("value", devCode),
                                         element)
                                    );

                                
                            }
                            xDocument.Save(Filename);
                        }
                    }

                    //ovde ide nema ide linija koja kaze gde da smestis elementToAdd

                    xDocument.Save(Filename);
                }
                else
                {
                    var newAgg = new XElement("AggregatorCode", new XAttribute("value", message.AggregatorCode));
                    aggregatorCodes.Last().AddAfterSelf(newAgg);

                    foreach (var devCode in message.Buffer.Keys)
                    {
                        foreach (var measurement in message.Buffer[devCode])
                        {
                            var elementToAdd2 = new XElement("Measurement");
                            elementToAdd2.Add(new XElement("Timestamp", measurement.Timestamp));

                            foreach (var amivp in measurement.Measurements)
                            {
                                XElement el2 = new XElement("AMIValuePair",
                                            new XElement("Type", amivp.Type),
                                            new XElement("Value", amivp.Value));
                                elementToAdd2.Add(el2);
                            }
                            aggregatorCodes.Elements("DeviceCode").First(node => node.Attribute("value").Value == devCode).Add(elementToAdd2);

                        }
                        xDocument.Save(Filename);
                    }
                    xDocument.Save(Filename);
                }
            }
        }

        private bool CheckIfAggregatorIsInStorage(string aggregatorCode)
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while(reader.Read())
                {
                    if (reader.Name == "AggregatorCode" && (reader.GetAttribute("value") == aggregatorCode))
                        return true;
                }
            }
            return false;
        }

        bool CheckIfDeviceIsInStorage(string deviceCode)
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                while (reader.Read())
                {
                    if (reader.Name == "DeviceCode" && (reader.GetAttribute("value") == deviceCode))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void Load()
        {

        }
    }
}
