using AMICommons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace AMIAggregator
{
    //naziv je malo gadan, za sada nemam bolje ime
    public class MessageForAggregator : IMessageForAggregator
    {
        //TODO: dodati logiku za slanje ka SM
        private string localStorage = "localStorage.xml";
        private List<AMIMeasurement> buffer = new List<AMIMeasurement>();
        private System.Timers.Timer timer = new System.Timers.Timer();

        //po ovoj implementaciji ce u fajl upisivati samo merenja pristigla od poslednjeg prijavljenog uredjaja
        //TODO: neka se sva merenja od svih uredjaja upisuju u isti fajl i neka se ne prepisuju jedan preko drugog
        public void SendMessageToAggregator(AMIMeasurement message)
        {
            Console.WriteLine("Poruka prispela");
            //ako fajl nije poslat zbog ispada agregatora, upisi merenja iz njega u bafer i nastavi da pises u fajl po starom
            //ovo radim da se merenja ne bi izgubila
            if (File.Exists(localStorage) && buffer.Count == 0)
            {
                buffer = Load(localStorage);
            }
            buffer.Add(message);
            Save(buffer, localStorage);           
        }

        private static bool IsEmpty(string filename)
        {
            var fs = File.OpenRead(filename);
            byte[] array = new byte[5];

            if (fs.Read(array, 0, 1) == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// poziva se kada se podaci posalju ka SM
        /// </summary>
        public void ClearData()
        {
            if(File.Exists(localStorage))
            {
                File.Delete(localStorage);
            }
            buffer.Clear();
        }

        /// <summary>
        /// Metoda koja serijalizuje listu dobijenih merenja u localStorage.xml
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        public void Save(List<AMIMeasurement> data, string filename)
        {
            using (var writer = new System.IO.StreamWriter(filename))
            {
                var serializer = new XmlSerializer(typeof(List<AMIMeasurement>));
                serializer.Serialize(writer, data);
                writer.Flush();
            }
        }
        /// <summary>
        /// Metoda koja iz localStorage.xml iscitava sva merenja i smesta ih u listu
        /// </summary>
        /// <returns></returns>
        public List<AMIMeasurement> Load(string filename)
        {
            using (var stream = System.IO.File.OpenRead(filename))
            {
                var serializer = new XmlSerializer(typeof(List<AMIMeasurement>));
                return serializer.Deserialize(stream) as List<AMIMeasurement>;
            }
        }
    }
}
