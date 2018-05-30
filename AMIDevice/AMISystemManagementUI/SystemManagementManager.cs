using AMICommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMISystemManagementUI
{
    class SystemManagementManager : IMessageForSystemManagement
    {
        public bool SendMessageToSystemManagement(AggregatorMessage message)
        {
            MainWindow.LastAggregatorMessage = message;
            //Ovde radimo serijalizaciju? Mozda ni ne treba skladistiti LastAggregatorMessage, ali je korisno za debug
            return true;
        }
    }
}
