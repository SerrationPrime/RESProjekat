using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace AMICommons
{
    [ServiceContract]
    public interface IMessageForSystemManagement
    {
        [OperationContract]
        bool SendMessageToSystemManagement(AggregatorMessage message);
    }
}
