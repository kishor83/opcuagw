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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotGateway.Core;
using UnifiedAutomation.UaClient;

namespace IotGateway.PlugIn.OpcUa
{
	public class OpcSubscriptionHandlers<TMsg>
	{
        private TagStateManager _tagStateManager;
        IConverter<object, Tag> _opcTagConverter;
        private ILogger _logger;

        public OpcSubscriptionHandlers(TagStateManager tagStateManager, IConverter<object, Tag> opcTagConverter, ILogger logger)
		{
            this._tagStateManager = tagStateManager;
            _opcTagConverter = opcTagConverter;
            this._logger = logger; 
		}
		/// <summary>
		/// Triggered when notifications are received from the OPC server
		/// </summary>
		/// <param name="subscription"></param>
		/// <param name="e"></param>
		/// 
		public void Subscription_NotificationMessageReceived(Subscription subscription, NotificationMessageReceivedEventArgs e)
		{
            _logger.Write(string.Format("received - seq: {0}", e.NotificationMessage.SequenceNumber));
		}
		/// <summary>
		/// When a subscription is recycled or keep-alive is done this event hander will be triggered. Logging has to be implemented
		/// for diagnostics
		/// </summary>
		/// <param name="subscription">Current subscription</param>
		/// <param name="e">Use SubscriptionConnectionStatus.CurrentStatus and SubscriptionConnectionStatus.OldStatus to write meaningful log</param>
		public void Subscription_StatusChanged(Subscription subscription, SubscriptionStatusChangedEventArgs e)
		{
			//log this
		}
		public void Subscription_DataChanged(Subscription subscription, DataChangedEventArgs e)
		{
			foreach(DataChange change in e.DataChanges)
			{
				string timeStamp = ((change.Value.SourceTimestamp != DateTime.MinValue) ? change.Value.SourceTimestamp : change.Value.ServerTimestamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
				string typeInfo = change.Value.WrappedValue.TypeInfo.ToString();
				string wrappedValue = change.Value.WrappedValue.ToString();
				//string tagName = change.MonitoredItem.NodeId.Identifier.ToString();
                string tagName = change.MonitoredItem.NodeId.ToString();
                WriteLog(string.Format("{{tagName: \"{0}\", timeStamp: \"{1}\", typeInfo: \"{2}\", wrappedValue: {3}}}", tagName, timeStamp, typeInfo, wrappedValue));
                //write the changed state to the state manager. 
                OpcTag opcGeneratedTag = new OpcTag() { Id = tagName, TagType = typeInfo, TagQuality = "0", TagValue = wrappedValue, Timestamp = DateTime.Parse(timeStamp) };
                Tag tag = _opcTagConverter.Convert(opcGeneratedTag);
                _tagStateManager.UpdateTag<string>(tag.Name, tag.GetTimestamp(), wrappedValue, tag.Other);								
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="subscription"></param>
		/// <param name="e"></param>
		public void Subscription_NewEvents(Subscription subscription, NewEventsEventArgs e)
		{
            //1. convert the event to exchnage message
            //2. write the message to IMessageExchange<Msg>
		}

        public void WriteLog(string msg)
        {
            if (this._logger != null)
            {
                this._logger.Write(msg);
            }
        }
	}
}
