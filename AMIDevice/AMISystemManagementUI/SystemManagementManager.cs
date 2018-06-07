using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AMISystemManagementUI
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class SystemManagementManager : IMessageForSystemManagement
    {
        private string Filename = "globalStorage.xml";
        public bool SendMessageToSystemManagement(AggregatorMessage message)
        {
            Validate(message);
            //Ovde radimo serijalizaciju? Mozda ni ne treba skladistiti LastAggregatorMessage, ali je korisno za debug
            UpdateLog(message);
            return true;
        }

        /// <summary>
        /// Validacija, baca izuzetke ako je message u neocekivanom stanju
        /// </summary>
        /// <param name="message"></param>
        void Validate(AggregatorMessage message)
        {
            if (message.Buffer == null)
                throw new ArgumentNullException();
            foreach (var measurementList in message.Buffer.Values)
            {
                foreach (var measurementGroup in measurementList)
                {
                    if (!measurementGroup.IsValid())
                        throw new ArgumentException();
                    if (measurementGroup.Measurements == null)
                        throw new ArgumentNullException();
                }
            }
        }

        /// <summary>
        /// Serijalizacija, slična onoj implementiranoj u AggregatorManager, ali sa dodatnim nivoom hijerarhije AggregatorCode.
        /// Primetiti kako se ne serijalizuje Message.Timestamp, jer ga ne koristi nijedan grafik, i nije jasno kako bi on bio implementiran.
        /// </summary>
        /// <param name="message"></param>
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
                        if (CheckIfDeviceIsInStorage(devCode, message.AggregatorCode))
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
                            XElement SumElement = new XElement("DeviceCode", new XAttribute("value",devCode));
                            foreach (var measurement in message.Buffer[devCode])
                            {
                                var element = new XElement("Measurement",
                                                   new XElement("Timestamp", measurement.Timestamp.ToString()));

                                foreach (var amivp in measurement.Measurements)
                                {
                                    element.Add(
                                        new XElement("AMIValuePair",
                                            new XElement("Type", amivp.Type),
                                            new XElement("Value", amivp.Value))
                                            );

                                }
                                SumElement.Add(new XElement(element));
                            }
                            firstRow.AddBeforeSelf(SumElement);
                            xDocument.Save(Filename);
                        }
                    }

                    xDocument.Save(Filename);
                }
                else
                {
                    var newAgg = new XElement("AggregatorCode", new XAttribute("value", message.AggregatorCode));
                    aggregatorCodes.Last().AddAfterSelf(newAgg);

                    foreach (var devCode in message.Buffer.Keys)
                    {
                        var SuperElement = new XElement("DeviceCode", new XAttribute("value", devCode));
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
                            SuperElement.Add(elementToAdd2);
                        }
                        aggregatorCodes.Last().Add(SuperElement);
                        xDocument.Save(Filename);
                        
                    }
                    xDocument.Save(Filename);
                }
            }
        }
        
        /// <summary>
        /// Provera da li agregator AggregatorCode postoji u skladistu.
        /// </summary>
        /// <param name="aggregatorCode"></param>
        /// <returns>True ako postoji</returns>
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

        /// <summary>
        /// Provera da li uređaj DeviceCode postoji u skladistu u okviru specificnog agregatora aggCode
        /// </summary>
        /// <param name="deviceCode"></param>
        /// <returns>True ako postoji</returns>
        bool CheckIfDeviceIsInStorage(string deviceCode, string aggCode)
        {
            using (XmlReader reader = XmlReader.Create(Filename))
            {
                reader.ReadToFollowing("AggregatorCode");
                if (reader.GetAttribute("value") == aggCode)
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "DeviceCode" && (reader.GetAttribute("value") == deviceCode))
                        {
                            return true;
                        }
                        if (reader.Name == "AggregatorCode")
                            break;
                    }
                }
            }
            return false;
        }
    }
}
