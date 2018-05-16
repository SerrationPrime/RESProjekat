using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMIDevice
{
    class Program
    {
        static AMIMeasurement Message;

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
        }

        /// <summary>
        /// Svake sekunde, vrsi malu promenu na trenutnom stanju uredjaja i promenjeno stanje salje na agregator
        /// </summary>
        static void SimulationLoop()
        {
            while (true)
            {
                Console.WriteLine("Current device state:" + Message.ToString());
                //TODO: ubaciti logiku za slanje
                System.Threading.Thread.Sleep(1000);
                Message.PerturbValues();
            }
        }
    }
}
