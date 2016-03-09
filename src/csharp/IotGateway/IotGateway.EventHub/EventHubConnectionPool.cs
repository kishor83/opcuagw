using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IotGateway.PlugIn.EventHub
{
	//Enhance this for SAS key usage 
	public class EventHubConnectionPool
	{
		private List<MessagingFacotryPoolItem> _messageFactoryPool;
		//number of factory objects to be populated into _mfPool
		private int _numMessagingFactories;
		//number of Event Hub clients per factory to be populated into the _ehcPool
		private int _numEventHubClients;
		//service hub name space containing the EventHub
		private string _serviceBusName;
		//service bus namespace connection string used by the MessagingFactory
		private string _sbConnectionString;
		//name of hte event hub assocaited with client
		private string _eventHubName;
		//if this is true don't do any changes to the object state
		public bool _poolInitialized;

		public bool PoolInitialized { get { return _poolInitialized; } }
		public EventHubConnectionPool(string serviceBusName, string sbConnectionSTring, string eventHubName)
		{
			SetConnectiondefaults();
			this._serviceBusName = serviceBusName;
			this._sbConnectionString = sbConnectionSTring;
			this._eventHubName = eventHubName;
			this._numMessagingFactories = 1;
			this._numEventHubClients = 1;
			this._poolInitialized = false;
		}
		public EventHubConnectionPool(string serviceBusName, string sbConnectionSTring, string eventHubName, int mfPoolSzie, int ehPoolSize)
		{
			SetConnectiondefaults();
			this._serviceBusName = serviceBusName;
			this._sbConnectionString = sbConnectionSTring;
			this._eventHubName = eventHubName;
			this._numMessagingFactories = mfPoolSzie;
			this._numEventHubClients = ehPoolSize;
			this._poolInitialized = false;
		}

		private void SetConnectiondefaults()
		{
			//removes the output connection limit (by default .NET sets it to 2)                                                   
			ServicePointManager.DefaultConnectionLimit = 150;
			//turn off Nagle's algorithm as we will send small messages
			ServicePointManager.UseNagleAlgorithm = false;
		}

		public void InitPool()
		{
			_messageFactoryPool = new List<MessagingFacotryPoolItem>();
			try
			{
				for (int i = 0; i < _numMessagingFactories; i++)
				{
					MessagingFacotryPoolItem mf = new MessagingFacotryPoolItem(_sbConnectionString, _eventHubName, _numEventHubClients);
					_messageFactoryPool.Add(mf);
				}
				_poolInitialized = true;
			}
			catch (ApplicationException ex)
			{
				throw new ApplicationException("InitPool exception", ex);
			}

		}

		public void ResetPool()
		{
			ClosePool();
			InitPool();
		}
		public void ClosePool()
		{
			try
			{
				foreach (MessagingFacotryPoolItem mfp in _messageFactoryPool)
				{
					mfp.CloseAllConnections();
				}
			}
			catch
			{
				//Log this exception
			}

			_messageFactoryPool = null;
			_poolInitialized = false;
		}

		public EventHubClient GetEventHubClient()
		{
			if (_messageFactoryPool.Count > 0)
			{
				int mfpPoolIx = new Random().Next(_messageFactoryPool.Count);
				MessagingFacotryPoolItem mfpi = _messageFactoryPool[mfpPoolIx];
				return mfpi.GetRandomEventHubClient();
			}
			else return null;
		}

	}
}
