using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AMISystemManagementUI
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class SystemManagementManager : IMessageForSystemManagement
    {
        public bool SendMessageToSystemManagement(AggregatorMessage message)
        {
            Validate(message);

            MainWindow.LastAggregatorMessage = message;
            //Ovde radimo serijalizaciju? Mozda ni ne treba skladistiti LastAggregatorMessage, ali je korisno za debug
            return true;
        }

        void Validate(AggregatorMessage message)
        {
            if (message.Buffer == null)
                throw new ArgumentNullException();
            foreach(var measurementList in message.Buffer.Values)
            {
                foreach(var measurementGroup in measurementList)
                {
                    if (!measurementGroup.IsValid())
                        throw new ArgumentException();
                    if (measurementGroup.Measurements == null)
                        throw new ArgumentNullException();
                }
            }
        }
    }
}
