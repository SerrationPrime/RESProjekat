using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AMIDevice
{
    class Program
    {
        static AMIMeasurement Message;
        static IMessageForAggregator proxy;

        
        static void Main(string[] args)
        {
            Start();
            SimulationLoop();
        }

        /// <summary>
        /// Setup funkcija za rad uredjaja, podrzava rucni unos ID-a uredjaja
        /// </summary>
        static void Start()
        {
            int Input=0;
            bool InputSuccess=false;
            Console.WriteLine("Enter device ID (1), or randomly generate it? (2)");
            while (!InputSuccess || Input<1 || Input>2)
            {
                InputSuccess=Int32.TryParse(Console.ReadLine(), out Input);   
            }

            if (Input == 1)
            {
                Console.WriteLine("Please input device ID");
                Message = new AMIMeasurement(Console.ReadLine());
            }
            else
            {
                Message = new AMIMeasurement();
                Console.WriteLine("Your device ID is {0}",Message.DeviceCode);
            }

            //TODO: dodati pokretanje WCF proksija
            //prebacila sam povezivanje u SimulationLoop, kako bi obezbedila da se za svaku poruku otvara nova konekcija
            //ako ispadne agregator ili ne prodje konekcija (jer AGGr ne postoji) -\> catch exceptioon

        }

        /// <summary>
        /// Svake sekunde, vrsi malu promenu na trenutnom stanju uredjaja i promenjeno stanje salje na agregator
        /// </summary>
        static void SimulationLoop()
        {
            while (true)
            {
                //TODO: pre slanja poruke, izaberi kom agregatoru ce da salje
                // ili se to radi u klasi Device? mozda polje, da izabere pripapdnost agregatoru?
                Connect();
                Console.WriteLine("Current device state:" + Message.ToString());
                //TODO: ubaciti logiku za slanje
                System.Threading.Thread.Sleep(1000);
                Message.PerturbValues();
                try
                {
                    proxy.SendMessageToAggregator(Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    //uredjaj nastavlja da salje, cak i kada nema ko to da primi
                }
            }
        }
        
        static void Connect()
        {
            //TODO: obezbedi da vise uredjaja salje na agregator
            NetTcpBinding binding = new NetTcpBinding();
            string uri = "net.tcp://localhost:10100/IMessageForAggregator";
            ChannelFactory<IMessageForAggregator> factory = new ChannelFactory<IMessageForAggregator>(binding, new EndpointAddress(uri));
            proxy = factory.CreateChannel();
        }
    }
}
