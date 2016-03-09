using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IotGateway.PlugIn.EventHub
{
	public class MessagingFacotryPoolItem
	{
		private MessagingFactory _messagingFactory;
		List<EventHubClient> _eventHubClientPool;
		public MessagingFacotryPoolItem(string _sbConnectionString, string eventHubName, int clientPoolSize)
		{
			_messagingFactory =  MessagingFactory.CreateFromConnectionString(_sbConnectionString);
			_eventHubClientPool = new List<EventHubClient>(clientPoolSize);

			for (int i = 0; i < clientPoolSize; i++)
			{
				_eventHubClientPool.Add(_messagingFactory.CreateEventHubClient(eventHubName));
			}
		}
		//return a random client to sp
		public EventHubClient GetEventHubClient(int clientIndex)
		{
			if (_eventHubClientPool == null) return null;

			if (clientIndex < 0 || clientIndex > (_eventHubClientPool.Count - 1))
			{
				return null;
			}
			else
			{
				return _eventHubClientPool[clientIndex];
			}
		}

		public EventHubClient GetRandomEventHubClient()
		{
			if (_eventHubClientPool == null) return null;
			if (_eventHubClientPool.Count == 1) return _eventHubClientPool[0];
			int ehcPoolIx = new Random().Next(this._eventHubClientPool.Count);
			return this._eventHubClientPool[ehcPoolIx];

		}
		public MessagingFactory MessagingFactory { get { return this._messagingFactory; } }
		public void CloseAllConnections()
		{

			try
			{
				if (_eventHubClientPool != null)
				{
					foreach (EventHubClient ehc in _eventHubClientPool)
					{
						ehc.Close();
					}
				}
				if (_messagingFactory != null)
					_messagingFactory.Close();
			}
			catch
			{
				//do some logging here
			}
			_messagingFactory = null;
			_eventHubClientPool = null;
		}
	}
}
