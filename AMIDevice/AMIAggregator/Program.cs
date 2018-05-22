using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AMIAggregator
{
    class Program
    {
        public static AggregatorMessage Message;
        static ServiceHost serviceHost = new ServiceHost(typeof(MessageForAggregator));
        static System.Timers.Timer timer = new System.Timers.Timer();
        static MessageForAggregator obj = new MessageForAggregator();

        static void Main(string[] args)
        {
            Console.WriteLine("Please input aggregator code.");
            string AggregatorCode = "";
            //Samo kontrola unosa
            while (AggregatorCode == "")
            {
                AggregatorCode = Console.ReadLine();
            }
            Message = new AggregatorMessage(AggregatorCode);

            //WCF mi je malo slab, mozda treba drugacije?
            //Naziv endpointa je baziraan na nazivu agregatora
            string AggregatorPath = String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.Port, AggregatorCode);

            ServiceHost AggregatorHost = new ServiceHost(typeof(AggregatorManager));
            AggregatorHost.AddServiceEndpoint(typeof(IAggregator), new NetTcpBinding(), AggregatorPath);
            AggregatorHost.Open();

            //ovo je za slanje ka SM, to be implemented

            timer.Interval = 3000000; //5 minuta u milisekundama
            //TODO: Ubaciti u konfig!
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            Console.ReadLine();
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //poziv slanja ka SM
            //poziv brisanja podataka ako je slanje uspesno
            //Podesi timestamp poruke u trenutku slanja
            obj.ClearData();
        }
    }
}
