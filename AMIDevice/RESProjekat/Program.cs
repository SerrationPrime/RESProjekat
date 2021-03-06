﻿using AMICommons;
using System;
using System.ServiceModel;
using System.Configuration;
using System.Xml;

namespace AMIDevice
{
    class Program
    {
        static AMIMeasurement Message;
        static IAggregator Proxy;

        static ChannelFactory<IAggregator> Factory;

        static void Main(string[] args)
        {
            if (Start())
                SimulationLoop();
            else
                return;
        }

        /// <summary>
        /// Setup funkcija za rad uredjaja, podrzava rucni unos ID-a uredjaja
        /// </summary>
        /// <returns>True u slucaju uspesne konekcije, false u slucaju neuspesne.</returns>
        static bool Start()
        {
            int Input = 0;
            bool InputSuccess = false;
            Console.WriteLine("Enter device ID (1), or randomly generate it? (2)");
            while (!InputSuccess || Input < 1 || Input > 2)
            {
                InputSuccess = Int32.TryParse(Console.ReadLine(), out Input);
            }

            if (Input == 1)
            {
                Console.WriteLine("Please input device ID");
                Message = new AMIMeasurement(Console.ReadLine());
            }
            else
            {
                Message = new AMIMeasurement();
                Console.WriteLine("Your device ID is {0}", Message.DeviceCode);
            }

            //Aplikacija se trenutno konfigurise preko app.config, i trenutno se veze na endpoint Aggregator1
            string ConfiguredAggregator = ReadAndValidateFromConfig();

            if (ConfiguredAggregator == "")
            {
                throw new ConfigurationErrorsException("Aggregator not found in configuration");
            }

            Factory = new ChannelFactory<IAggregator>(new NetTcpBinding(), new EndpointAddress(String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.Port, ConfiguredAggregator)));
            Proxy = Factory.CreateChannel();

            Console.WriteLine("Attempting to connect to device...");
            bool SuccessfulConnection = false;
            while (!SuccessfulConnection)
            {
                try
                {
                    SuccessfulConnection = Proxy.Connect(Message);
                    if (!SuccessfulConnection)
                    {
                        Console.WriteLine("Device already exists on the system, regenerating.");
                        Message = new AMIMeasurement();
                        Console.WriteLine("Your new device ID is {0}", Message.DeviceCode);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(10000);
                    Proxy = Factory.CreateChannel();
                }
            }
            Console.WriteLine("Successfully connected.");
            return true;
        }

        /// <summary>
        /// Svake sekunde, vrsi malu promenu na trenutnom stanju uredjaja i promenjeno stanje salje na agregator
        /// </summary>
        static void SimulationLoop()
        {
            while (true)
            {
                Console.WriteLine("Current device state:" + Message.ToString());
                System.Threading.Thread.Sleep(1000);
                Message.PerturbValues();
                try
                {
                    Proxy.SendMeasurement(Message);
                }
                catch
                {
                    Console.WriteLine("Aggregator unavailable. Attempting to reconnect...");
                    Proxy = Factory.CreateChannel();
                }
            }
        }

        /// <summary>
        /// Proverava konfigurisani AggregatorLogPath fajl za postojanje konfigurisanog Aggregator fajla.
        /// Takođe ispisuje sve agregatore iz AggregatorLogPath fajla.
        /// </summary>
        /// <returns></returns>
        static string ReadAndValidateFromConfig()
        {
            string AggregatorFromConfig=ConfigurationManager.AppSettings["Aggregator"];

            string retVal = "";
            Console.WriteLine("Available aggregators:");

            using (XmlReader read = XmlReader.Create(ConfigurationManager.AppSettings["AggregatorLogPath"]))
            {
                while (read.Read())
                {
                    if (read.Name == "Aggregator" && read.NodeType!=XmlNodeType.EndElement)
                    {
                        read.Read();
                        Console.WriteLine(read.Value);
                        if (read.Value == AggregatorFromConfig)
                        {
                            retVal = AggregatorFromConfig;
                        }
                    }       
                }
            }

            Console.WriteLine("----------------------------");

            if (retVal=="")
                Console.WriteLine("Invalid aggregator name in config. Press any key to exit program.");
            return retVal;
        }
    }
}
