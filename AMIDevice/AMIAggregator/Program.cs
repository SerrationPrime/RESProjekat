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
        static ServiceHost serviceHost = new ServiceHost(typeof(MessageForAggregator));
        static System.Timers.Timer timer = new System.Timers.Timer();
        static MessageForAggregator obj = new MessageForAggregator();

        static void Main(string[] args)
        {
            //ovo je za prijem od AMIDevice-a
            OpenConnectionToAMIDevice();
            
            //ovo je za slanje ka SM, to be implemented

            timer.Interval = 3000000; //5 minuta u milisekundama
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;

            Console.ReadLine();
        }

        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            //poziv slanja ka SM
            //poziv brisanja podataka ako je slanje uspesno
            obj.ClearData();
        }

        static void OpenConnectionToAMIDevice()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string uri = "net.tcp://localhost:10100/IMessageForAggregator";
            serviceHost.AddServiceEndpoint(typeof(IMessageForAggregator), binding, new Uri(uri));
            serviceHost.Open();
        }
        static void CloseConnectionToAMIDevice()
        {
            serviceHost.Close();
        }
    }
}
