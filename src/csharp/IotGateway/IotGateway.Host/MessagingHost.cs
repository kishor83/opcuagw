/*
Copyright(c) Microsoft.All rights reserved.

The MIT License(MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using IotGateway.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeC;

namespace IotGateway.Host
{
    /// <summary>
    /// MessagingHost uses a message exchange to receive, queue and send messages. Useful for sending alarms and events to the cloud
    /// </summary>
    public class MessagingHost : HostBase
    {
        public override void Initialize(Dictionary<string, object> settings)
        {
            string pluginNamespace = GetContainerNamespace();
            Debug.Assert(pluginNamespace != null);
            TypeContainer tc = TypeContainer.Instance;
            //"IotHubConfigFile", "MessageExchange", "ExchangeToIotHubMessageConverter", "Logger" 
            //configure receiver
            IProcessor messageReceiver = tc.GetInstance<IReceiver>(pluginNamespace) as IProcessor;
            IConverter<object, Msg> receivedMessageConverter = tc.GetInstance <IConverter<object, Msg>>(pluginNamespace);
            settings["MessageConverter.OpcUa"] = receivedMessageConverter;
            if (messageReceiver != null)
            {
                this.AddReceiver("MessageReceiver", messageReceiver);
            }

            //configure sender
            IProcessor messageSender = tc.GetInstance<ISender>(pluginNamespace) as IProcessor;
            IConverter<object, Msg> messageConverter = tc.GetInstance<IConverter<object, Msg>>(pluginNamespace);
            IMessageExchange<Msg> messageExchange = tc.GetInstance<IMessageExchange<Msg>>(pluginNamespace);
            settings["MessageExchange.Messaging"] = messageExchange;
            settings["MessageConverter.IotHub"] = messageConverter;
            settings["Logger"] = tc.GetInstance<ILogger>();

            if (messageSender != null)
            {
                this.AddSender("MessageSender", messageSender);
            }

            base.Initialize(settings);
        }
    }
}
