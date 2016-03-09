using IotGateway.Core;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotGateway.PlugIn.EventHub
{
    public class ToEventDataConverter : IConverter<IMessage, object>
    {
        public object Convert(IMessage inObj)
        {
            EventData eventData = new EventData(Encoding.UTF8.GetBytes(inObj.Serialize()));
            string partitionKey = inObj.GetId();
            if (partitionKey != null)
            {
                eventData.PartitionKey = partitionKey;
            }
            return eventData;
        }
    }
}
