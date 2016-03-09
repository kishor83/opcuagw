using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IotGateway.PlugIn.EventHub
{
	public class EventHubRepository
	{
		private EventHubConnectionPool _eventHubConnectionPool;
		//number of messaging factories; note that each messaging factory holds a physical TCP connection
		private static int _numMessagingFactories = 1;
		//private static List<EventHubClient> _ehcPool;
		//number of Event Hub clients per factory to be populated into the _ehcPool
		private static int _numEventHubClients = 1;
		//ServiceBus connection string - in future replace this with a device specific SAS key
		private static string _connectionString;
		//ServiceBus namespace
		private static string _serviceBusName;
		//Event Hub name
		private static string _hubname;
		private static bool _poolInitialized = false;

		/// <summary>
		/// SendEventHubMessage sends a single message to Event Hub.Uses Dictionary input to minimize external dependencies
		/// </summary>
		/// <param name="input"></param>
		public async void SendEventHubMessage(Dictionary<string, string> input)
		{
			string message = null;
			string partitionid = null;
			try
			{
				//TODO create enumerations
				message = input["EH_MESSAGE"];
				partitionid = input["EH_PARTITION_ID"];
			}
			catch (KeyNotFoundException knfe)
			{
				throw new ApplicationException("SendEventHubMessage: dictionary values are missing", knfe);
			}

			EventHubClient ec = _eventHubConnectionPool.GetEventHubClient();
			if (ec != null)
			{
				EventData eventData = new EventData(Encoding.UTF8.GetBytes(message)) { PartitionKey = partitionid };
				await ec.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
			}
			else
			{
				throw new ApplicationException("The MessageFactory pool is empty");
			}
		}
		public async void SendEventHubMessage(EventData msg)
		{
			EventHubClient ec = _eventHubConnectionPool.GetEventHubClient();
			if (ec != null)
			{
				await ec.SendAsync(msg);
			}
			else
			{
				throw new ApplicationException("The MessageFactory pool is empty");
			}
		}
		public Task<object> RecycleConnectionPool(Dictionary<string, object> input)
		{
			_eventHubConnectionPool.ResetPool();
			return Task.FromResult<object>(true);
		}

		public Task<object> InitPool(Dictionary<string, string> input)
		{
			if (_poolInitialized)
			{
				//NOP - do nothing
				return Task.FromResult<object>(string.Format("The pool is already initialied"));
			}
			try
			{
				_connectionString = input["EventHubConnectionString"];
				_serviceBusName = input["ServiceBusNameSpace"];
				_hubname = input["EventHubName"];
			}
			catch (KeyNotFoundException knfe)
			{
				throw new ApplicationException("InitPool: dictionary values are missing", knfe);
			}
			finally
			{
				//reset the state of the connection pool to known state
				_eventHubConnectionPool = null; 
			}

			_eventHubConnectionPool = new EventHubConnectionPool(_serviceBusName, _connectionString, _hubname, _numMessagingFactories, _numEventHubClients);
			_eventHubConnectionPool.InitPool();
			return Task.FromResult<object>(string.Format("The pool is initialied to: factories: {0}, clients/factory: {1}", _numMessagingFactories, _numEventHubClients));
		}

		public void ClosePool()
		{
			try
			{
				_eventHubConnectionPool.ClosePool();
			}
			finally
			{
				_eventHubConnectionPool = null;
			}

		}
	}
}
