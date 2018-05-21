using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using AMICommons;

namespace AMIAggregator
{
    class Program
    {
        public static AggregatorMessage Message;
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
            string AggregatorPath = String.Format("net.tcp://localhost:{0}/{1}", AggregatorMessage.Port,AggregatorCode);

            ServiceHost AggregatorHost = new ServiceHost(typeof(AggregatorManager));
            AggregatorHost.AddServiceEndpoint(typeof(IAggregator), new NetTcpBinding(), AggregatorPath);
            AggregatorHost.Open();

            Console.WriteLine("Aggregator {0} is now active.", AggregatorCode);

            Console.ReadKey();

            //Implementirati za kasnije, vecinu logike za XML radi AggregatorManager, ovde samo treba logiku za cekanje i slanje na SystemManagement
            //while(true)
            //{
                //    //brojac % vremeizkonfiguracije
                //    //event po isteku tajmera
                //    //ako vec postoji xml  (pukao je pre nego sto je poslao)
                //    //nastavi sa citanjem jos 5 minuta pa salji ka SM

                //    //ako je bilo neuspesno slanje ka SM
                //    //onda ne brise svoju bazu
                //    //vec ponovo ceka 5 minuta pa salje
            //}
        }
    }
}
