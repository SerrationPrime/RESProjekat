using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Configuration;

namespace AMIDevice
{
    class Program
    {
        static AMIMeasurement Message;
        static IAggregator Proxy;

        static void Main(string[] args)
        {
            if (Start())
                SimulationLoop();
            else
                Console.ReadKey();
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
            ChannelFactory<IAggregator> Factory = new ChannelFactory<IAggregator>(new NetTcpBinding(), new EndpointAddress(String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.Port, ConfigurationManager.AppSettings["Aggregator"])));
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
                    return false;
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
                Proxy.SendMeasurement(Message);
            }
        }
    }
}
