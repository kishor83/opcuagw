using System;
using System.Collections.Generic;
using System.Threading.Tasks;
//using BAGateway;
using IotGateway.Common;
using Microsoft.ServiceBus.Messaging;
using IotGateway.Core;

namespace IotGateway.PlugIn.EventHub
{
	public class EventHubMessageSender<TMsg>: IProcessor
	{
		private Dictionary<string, object> _settings;
        private IMessageExchnage<TMsg> _messageExchange;
        private IConverter<TMsg, object> _dataConverter; 
		private Dictionary<string, string> _eventHubConfig;
		private bool _stopCalled = false;
		public EventHubMessageSender()
		{

		}
        public EventHubMessageSender(IMessageExchnage<TMsg> messageExchnage)
		{
			_messageExchange = messageExchnage;
			_eventHubConfig = new Dictionary<string, string>();
		}
        public EventHubMessageSender(IMessageExchnage<TMsg> messageExchnage, IConverter<TMsg, object> dataConverter, Dictionary<string, object> settings)
		{
			_messageExchange = messageExchnage;
			_dataConverter = dataConverter;
			_eventHubConfig = new Dictionary<string, string>();
			_settings = settings;
		}

		#region IProcessor Members
		/// <summary>
		/// All the following fields are mandatory; the data type of all the fields are string type
		/// settings.ServiceBusNameSpace, settings.EventHubName, settings.EventHubConnectionString 
		/// </summary>
		/// <param name="settings"></param>
		public void Initialize(Dictionary<string, object> settings)
		{
			string[] fieldNames = new string[] { "EventHubConfigFile", "MessageExchange", "TagToEventDataConverter" };

			//validates missing fields and throws ValidationException in case of errors. Let us not catch it here so as to avoid
			//passing it through multiple layers
			FieldValidator.CheckMissingFields(settings, fieldNames);
            string eventHubConfigFile = settings.GetValueOrNull("EventHubConfigFile") as string;
            _eventHubConfig = ConfigUtility.ReadConfig<Dictionary<string, string>>(eventHubConfigFile);
            _messageExchange = settings.GetValueOrNull("MessageExchange") as IMessageExchnage<TMsg>;
            _dataConverter = settings.GetValueOrNull("TagToEventDataConverter") as IConverter<TMsg, object>;
        }

		public async void Start()
		{
				EventHubRepository eh = new EventHubRepository();
				await eh.InitPool(_eventHubConfig);
				while (true)
				{
					if (this._stopCalled)
					{
						break; 
					}
					EventData eventData = null;
					TMsg tag = default(TMsg); 
					if (this._messageExchange.IsEmpty())
					{
						await Task.Delay(2000);
					}
					else
					{
						tag = _messageExchange.Read();
						eventData = (EventData)_dataConverter.Convert(tag);

						if (eventData != null)
						{
							try
							{
								eh.SendEventHubMessage(eventData);
							}
							catch (ApplicationException exp)
							{
								throw new ApplicationException("EventHub.SendEventHubMessage failed", exp);
							}
						}
					}
				}
		}
		public void Stop()
		{
			this._stopCalled = true; 
		}


		public void Initialize()
		{
			Initialize(_settings);
		}
		//public void ConfigureSettings(Dictionary<string, object> settings)
		//{
		//	_settings = settings;
		//}
		#endregion
	}
}
