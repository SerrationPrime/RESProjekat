using AMICommons;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace AMIAggregator
{
    class Program
    {
        const string AggregatorLogName = "../AggregatorList.xml";
        public static AggregatorMessage Message;
        /// <summary>
        /// Objekat se koristi za pravilno slanje prema SM komponenti prema naznačenom vremenskom periodu
        /// </summary>
        static System.Timers.Timer timer = new System.Timers.Timer();

        static void Main(string[] args)
        {
            Console.WriteLine("Please input aggregator code.");
            string AggregatorCode = "";
            //Samo kontrola unosa
            while (AggregatorCode == "")
            {
                AggregatorCode = Console.ReadLine();
            }
            AddToList(AggregatorCode);
            Message = new AggregatorMessage(AggregatorCode);

            //Naziv endpointa je baziran na nazivu agregatora
            string AggregatorPath = String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.Port, AggregatorCode);

            var binding = new NetTcpBinding();
            //Podrska za vise endpointova na istom agregatoru, ali zahteva neko adminsko nameštanje
            //proveriti https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/how-to-enable-the-net-tcp-port-sharing-service
            binding.PortSharingEnabled = true;
            ServiceHost AggregatorHost = new ServiceHost(typeof(AggregatorManager));
            AggregatorHost.AddServiceEndpoint(typeof(IAggregator), binding, AggregatorPath);
            AggregatorHost.Open();

            Console.WriteLine();

            double temp;
            if (!double.TryParse(ConfigurationManager.AppSettings["TimeDelay"], out temp))
            {
                Console.WriteLine("The configuration file is improperly formatted, and it could not be read from. Press any key to close application.");
                Console.ReadKey();
                return;
            }
            timer.Interval = temp;
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            Console.WriteLine("Aggregator simulator is now active with a message interval of {0} seconds.", timer.Interval / 1000);

            //Petlja dodata zbog problema sa automatskim testiranjem i Console.ReadKey() blokiranjem.
            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        /// <summary>
        /// Nit zaduzena za periodicno slanje podataka ka SM.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            if (Message.Buffer.Count >= 0)
            {
                IMessageForSystemManagement Proxy;
                //Potrebna podešavanja zbog veličine prenesenih podataka
                var binding = new NetTcpBinding();
                binding.MaxBufferPoolSize = 20000000;
                binding.MaxBufferSize = 20000000;
                binding.MaxReceivedMessageSize = 20000000;

                ChannelFactory<IMessageForSystemManagement> Factory = new ChannelFactory<IMessageForSystemManagement>(binding, new EndpointAddress(String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.SysPort, AggregatorMessage.SysEndpointName)));
                Proxy = Factory.CreateChannel();
                //poziv slanja ka SM
                //Podesi timestamp poruke u trenutku slanja
                try
                {
                    Message.Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                    Proxy.SendMessageToSystemManagement(Message);
                    //poziv brisanja podataka ako je slanje uspesno
                    Console.WriteLine("Data successfuly sent.");
                    AggregatorManager.ClearData();
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                    Console.WriteLine("Could not send data");
                }
            }
            else
            {
                Console.WriteLine("Nothing to send.");
            }

        }

        /// <summary>
        /// Drzimo evidenciju svih mogucih naziva agregatora, koje device preuzima, ispisuje, i protiv kojih vrsi kontrolu.
        /// Mozda bi bilo moguce drzati evidenciju AKTIVNIH agregatora, ali bi to zahtevalo ili komplikovano WCF resenje, ili neke metode koje se
        /// pokrecu pri zatvaranju aplikacije. U svakom slucaju, ovo spada van opsega zadatka.
        /// </summary>
        /// <param name="aggregatorName"></param>
        private static void AddToList(string aggregatorName)
        {

            if (!File.Exists(AggregatorLogName))
            {
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                xmlWriterSettings.NewLineOnAttributes = true;
                using (XmlWriter xmlWriter = XmlWriter.Create(AggregatorLogName))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Aggregators");
                    xmlWriter.WriteElementString("Aggregator", aggregatorName);
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                using (XmlReader read = XmlReader.Create(AggregatorLogName))
                {
                    while (read.Read())
                    {
                        if (read.Value == aggregatorName)
                            return;
                    }
                }


                XDocument xDocument = XDocument.Load(AggregatorLogName);
                XElement root = xDocument.Element("Aggregators");
                IEnumerable<XElement> rows = root.Descendants("Aggregator");
                XElement firstRow = rows.First();

                firstRow.AddBeforeSelf(new XElement("Aggregator", aggregatorName));
                xDocument.Save(AggregatorLogName);

            }
        }
    }
}
    