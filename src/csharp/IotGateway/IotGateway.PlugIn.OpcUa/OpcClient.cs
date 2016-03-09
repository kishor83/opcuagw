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
//DO NOT REDISTRIBUTsE UnifiedAutomation.UaBase.dll AND UnifiedAutomation.UaClient.dll
//This code should never be given out as this depends on Unified Automation Inc's libraries UnifiedAutomation.UaBase.dll
// UnifiedAutomation.UaClient.dll unless 
//Partners and customers should acquire their own Unified Automation Inc's license or create their own OPC Client
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedAutomation.UaBase;
using UnifiedAutomation.UaClient;

namespace IotGateway.PlugIn.OpcUa
{
	public class OpcClient
	{
		private string m_serverUrl;
		private ApplicationInstance m_application;

		public OpcClient(string serverUrl)
		{
			m_serverUrl = serverUrl;
			m_application = ApplicationInstance.Default;
		}

		public OpcClient(string serverUrl, ApplicationInstance application)
		{
			m_serverUrl = serverUrl;
			m_application = application; 
		}

        public Session Connect(EndpointDescription opcEndpoint)
        {
            if (opcEndpoint == null)
                return null;
            Session session = new Session(m_application);
            if (session.UserIdentity == null)
                session.UserIdentity = new UserIdentity();
            session.UserIdentity.IdentityType = UserIdentityType.Anonymous;
            session.Connect(opcEndpoint, RetryInitialConnect.Yes, session.DefaultRequestSettings);

            return session;
        }

		public Session Connect(bool discoverUrl=true)
		{
			Session session = new Session(m_application);
            if (session.UserIdentity == null)
                session.UserIdentity = new UserIdentity();
            session.UserIdentity.IdentityType = UserIdentityType.Anonymous;
            EndpointDescription desc = null;
            if (discoverUrl)
            {
                var endpoints = OpcDiscovery.GetEndpoints(m_serverUrl);

                foreach (var endpoint in endpoints)
                {
                    if (endpoint.EndpointUrl.Trim().ToLower() == m_serverUrl.Trim().ToLower()
                        && endpoint.SecurityMode == MessageSecurityMode.None)
                    {
                        desc = endpoint;
                        break;
                    }
                }
            }
            else
            {
                desc = new EndpointDescription(m_serverUrl);
            }
            if (desc != null)
            {
                session.Connect(desc, RetryInitialConnect.Yes, session.DefaultRequestSettings);
                //session.Connect(m_serverUrl, SecuritySelection.None);
            }
            else
            {
                throw new ArgumentException("Cant find a matching endpoint at: " + m_serverUrl);
            }
			return session;
        }

        public Session Connect(string userName, string password, bool discoverUrl=false)
        {
            Session session = new Session(m_application);
            session.UseDnsNameAndPortFromDiscoveryUrl = true;
            if (session.UserIdentity == null)
            {
                session.UserIdentity = new UserIdentity();
                session.UserIdentity.UserName = userName;
                session.UserIdentity.Password = password;
                session.UserIdentity.IdentityType = UserIdentityType.UserName;
            }

            EndpointDescription endPointDescription = null;
            if (discoverUrl)
            {
                var endpoints = OpcDiscovery.GetEndpoints(m_serverUrl);
                foreach (var endpoint in endpoints)
                {
                    //let's use clear text endpoint just for testing
                    if (endpoint.SecurityMode == MessageSecurityMode.None)
                    {
                        endPointDescription = endpoint;
                        break;
                    }
                }
            }
            else
            {
                endPointDescription = new EndpointDescription(m_serverUrl);
            }
            if (endPointDescription != null)
            {
                //session.Connect(endPointDescription, RetryInitialConnect.No, session.DefaultRequestSettings);
                session.Connect(m_serverUrl, SecuritySelection.None);
            }
            else
            {
                throw new ArgumentException("Cant find a matching endpoint at: " + m_serverUrl);
            }
            return session;
        }

		public Subscription Subscribe(Session session)
		{
			Subscription subscription = new Subscription(session);

			subscription.PublishingInterval = 1000;
			subscription.MaxKeepAliveTime = 10000;
			subscription.Lifetime = 60000;
			subscription.MaxNotificationsPerPublish = 0;
			subscription.Priority = 0;
			subscription.PublishingEnabled = true;

			// create subscription.
			subscription.Create();
			return subscription;
		}
		public List<ReferenceDescription> Browse(Session session, NodeId nodeId)
		{
			BrowseContext context = new BrowseContext(); 
			context.BrowseDirection = BrowseDirection.Forward;
			context.ReferenceTypeId = ReferenceTypeIds.HierarchicalReferences; 
			context.IncludeSubtypes = true; 
			context.MaxReferencesToReturn = 0;

			byte[] continuationPoint = null;
			List<ReferenceDescription> references = session.Browse(nodeId, context, new RequestSettings() { OperationTimeout = 10000 }, out continuationPoint);
			List<ReferenceDescription> ret = new List<ReferenceDescription>();
			foreach (ReferenceDescription reference in references)
			{
				ret.Add(reference);
			}
			// process any continuation points.    
			while (!ByteString.IsNull(continuationPoint))
			{
				references = session.BrowseNext(new RequestSettings() { OperationTimeout = 10000 }, ref continuationPoint);
				foreach (ReferenceDescription reference in references)
				{
					ret.Add(reference);
				}
			}
			return ret;
		}

		public Dictionary<string,string> GetNodeDetails(Session session, ReferenceDescription desc)
		{
			var nodesToRead = buildAttributeList(desc);
			List<DataValue> values= session.Read(nodesToRead, 0, TimestampsToReturn.Both, null);
			Dictionary<string, string> ret = new Dictionary<string, string>();
			for (int i = 0; i < nodesToRead.Count; i++ )
			{
				string attributeName = (string)nodesToRead[i].UserData;
				string attributeValue = values[i].WrappedValue.ToString();
				//string attributeStatus = values[i].StatusCode.ToString();
				ret.Add(attributeName, attributeValue);
			}
			return ret;
		}
		private ReadValueIdCollection buildAttributeList(ReferenceDescription refDescription)
		{
			// Build list of attributes to read.
			ReadValueIdCollection nodesToRead = new ReadValueIdCollection();

			// Add default attributes (for all nodeclasses)
			addAttribute((NodeId)refDescription.NodeId, Attributes.NodeId, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.NodeClass, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.BrowseName, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.DisplayName, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.Description, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.WriteMask, nodesToRead);
			addAttribute((NodeId)refDescription.NodeId, Attributes.UserWriteMask, nodesToRead);

			// Add nodeclass specific attributes
			switch (refDescription.NodeClass)
			{
				case NodeClass.Object:
					addAttribute((NodeId)refDescription.NodeId, Attributes.EventNotifier, nodesToRead);
					break;
				case NodeClass.Variable:
					addAttribute((NodeId)refDescription.NodeId, Attributes.Value, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.DataType, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.ValueRank, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.ArrayDimensions, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.AccessLevel, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.UserAccessLevel, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.MinimumSamplingInterval, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.Historizing, nodesToRead);
					break;
				case NodeClass.Method:
					addAttribute((NodeId)refDescription.NodeId, Attributes.Executable, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.UserExecutable, nodesToRead);
					break;
				case NodeClass.ObjectType:
					addAttribute((NodeId)refDescription.NodeId, Attributes.IsAbstract, nodesToRead);
					break;
				case NodeClass.VariableType:
					addAttribute((NodeId)refDescription.NodeId, Attributes.Value, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.DataType, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.ValueRank, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.ArrayDimensions, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.IsAbstract, nodesToRead);
					break;
				case NodeClass.ReferenceType:
					addAttribute((NodeId)refDescription.NodeId, Attributes.IsAbstract, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.Symmetric, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.InverseName, nodesToRead);
					break;
				case NodeClass.DataType:
					addAttribute((NodeId)refDescription.NodeId, Attributes.IsAbstract, nodesToRead);
					break;
				case NodeClass.View:
					addAttribute((NodeId)refDescription.NodeId, Attributes.ContainsNoLoops, nodesToRead);
					addAttribute((NodeId)refDescription.NodeId, Attributes.EventNotifier, nodesToRead);
					break;
				default:
					break;
			}
			return nodesToRead;
		}
		private void addAttribute(NodeId node, uint attributeId, ReadValueIdCollection nodesToRead)
		{
			// Get NodeId from tree node .
			ReadValueId attributeToRead = new ReadValueId();
			attributeToRead.NodeId = node;
			attributeToRead.AttributeId = attributeId;
			attributeToRead.UserData = Attributes.GetDisplayText(attributeId);
			nodesToRead.Add(attributeToRead);
		}
		public List<StatusCode> CreateMonitoredItem(Subscription subscription, List<NodeId> nodeIdList)
		{
			List<MonitoredItem> monitoredItems = new List<MonitoredItem>();
			DataMonitoredItem monitoredItem = new DataMonitoredItem(nodeIdList[0], Attributes.Value);
			monitoredItem.MonitoringMode = MonitoringMode.Reporting;
			monitoredItem.SamplingInterval = 1;
			monitoredItem.QueueSize = 1;
			monitoredItem.DiscardOldest = true;
			monitoredItem.DeadbandType = DeadbandType.None;
			monitoredItem.Deadband = 0.0; 
			monitoredItems.Add(monitoredItem);
			// create monitored items.
			List<StatusCode> results = subscription.CreateMonitoredItems(
				monitoredItems,
				new RequestSettings() { OperationTimeout = 10000 });

			return results;
		}
	}
}
