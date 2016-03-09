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
using IotGateway.Common;
using IotGateway.Core;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

namespace IotGateway.PlugIn.OpcUa
{
	public class OpcReceiver<TMsg>:IProcessor, IReceiver
	{
		private Dictionary<string, object> _settings; 
		private Dictionary<string, string> _opcServerConfig;
		private List<OpcNode> _opcNodeSubscriptionConfig;
        private TagStateManager _tagStateManager;
        private ILogger _logger;
        private IConverter<object, Tag> _opcTagConverter; 
		private Session _session;

		/// <summary>
		/// settings requires the following fields:
		/// settings.EventHubConfig = "EventHubConfig.json", 
		/// settings.OpcServerConfig = "OpcServerConfig.json",
		/// settings.OpcNodeSubscriptionConfig = "OpcNodeSubscriptionConfig.json"
		/// For the format of the json files, check the SDK documentation
		/// </summary>
		/// <param name="settings"></param>
		public void Initialize(Dictionary<string, object> settings)
		{
            //ConfigureSettings(settings);
			//add new entry to fieldNames if a mandatory field is sent through settings
			string[] fieldNames = new string[] { "OpcServerConfigFile", "OpcTagConfigFile", "Logger", "OpcTagConverter", "TagStateManager" };

			//validates missing fields and throws ValidationException in case of errors. Let us not catch it here so as to avoid
			//passing it through multiple layers
			FieldValidator.CheckMissingFields(settings, fieldNames);

            //convert json fields into configuraiton dictionaries
            string opcServerConfigFile = settings.GetValueOrNull<string, object>("OpcServerConfigFile") as string;
            _opcServerConfig = ConfigUtility.ReadConfig<Dictionary<string, string>>(opcServerConfigFile);
            string opcTagConfigFile = settings.GetValueOrNull<string, object>("OpcTagConfigFile") as string;
            _opcNodeSubscriptionConfig = ConfigUtility.ReadConfig<List<OpcNode>>(opcTagConfigFile);
            _tagStateManager = settings.GetValueOrNull<string, object>("TagStateManager") as TagStateManager;
            _opcTagConverter = settings.GetValueOrNull<string, object>("OpcTagConverter") as IConverter<object, Tag>;
            _logger = settings.GetValueOrNull<string, object>("Logger") as ILogger;
        }

		public void Start()
		{
			OpcClient opcClient = new OpcClient(_opcServerConfig["OpcServerUrl"]);
			_session = opcClient.Connect(true);
			Subscription subscription = opcClient.Subscribe(_session);
			OpcSubscriptionHandlers<TMsg> opcReceiver = new OpcSubscriptionHandlers<TMsg>(_tagStateManager, _opcTagConverter, _logger);
			subscription.DataChanged += opcReceiver.Subscription_DataChanged;
			subscription.NotificationMessageReceived += opcReceiver.Subscription_NotificationMessageReceived;
			subscription.StatusChanged += opcReceiver.Subscription_StatusChanged;
			subscription.NewEvents += opcReceiver.Subscription_NewEvents;
            //create MonitoredItem: 3rd argument is the namespace index that depends on the OPC Server. For now we know it is 2; 
            //change this to read it from the server 
            //each node id requires separate call to CreateMonitoredItem
            //pooling all the nodes into a List and calling CreateMonitoredItem in one go doesn't work

            foreach (OpcNode opcNode in _opcNodeSubscriptionConfig)
            {
                List<NodeId> nodeIdList = new List<NodeId>();
                //the following is the working version for BA Server
                //NodeId nodeId = new NodeId(UnifiedAutomation.UaBase.IdType.String, opcNode.TagName, opcNode.TagNamespaceIndex);
                NodeId nodeId;
                switch(opcNode.TagNameDataType)
                {
                    case "string":
                        nodeId = new NodeId(opcNode.TagName, opcNode.TagNamespaceIndex);
                        break;
                    case "numeric":
                        nodeId = new NodeId(uint.Parse(opcNode.TagName), opcNode.TagNamespaceIndex);
                        break;
                    default:
                        nodeId = new NodeId(opcNode.TagName, opcNode.TagNamespaceIndex);
                        break;
                }
                nodeIdList.Add(nodeId);
                opcClient.CreateMonitoredItem(subscription, nodeIdList);
            }
        }

		public void Stop()
		{
			//disconnect OPC session
			_session.Disconnect();

			//do additional cleanup here
		}

		#region IProcessor Members


		public void Initialize()
		{
			Initialize(_settings);
		}
		public void ConfigureSettings(Dictionary<string, object> settings)
		{
			_settings = settings;
		}
		#endregion
	}
}
