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

namespace IotGateway.Host
{
    public class HostBase : IServiceHost
    {
        protected Dictionary<string, IProcessor> _senders;
        protected Dictionary<string, IProcessor> _receivers;
        protected Dictionary<string, object> _settings;
        protected string _pluginNamespace = "SERVICE_HOST";

        public HostBase()
        {
            _senders = new Dictionary<string, IProcessor>();
            _receivers = new Dictionary<string, IProcessor>();
        }
        public HostBase(Dictionary<string, object> settings)
        {
            _senders = new Dictionary<string, IProcessor>();
            _receivers = new Dictionary<string, IProcessor>();
            _settings = settings;
        }
        public HostBase(string pluginNamespace, Dictionary<string, object> settings)
        {
            Debug.Assert(pluginNamespace != null);
            _pluginNamespace = pluginNamespace; 
            _senders = new Dictionary<string, IProcessor>();
            _receivers = new Dictionary<string, IProcessor>();
            _settings = settings;
        }
        #region IServiceHost Members

        public virtual void AddReceiver(string name, IProcessor receiver)
        {
            _receivers.Add(name, receiver);
        }

        public virtual void AddSender(string name, IProcessor sender)
        {
            _senders.Add(name, sender);
        }
        #endregion


        #region IProcessor Members

        public virtual void Initialize(Dictionary<string, object> settings)
        {
            //for now no use for settings
            foreach (var sender in _senders.Values)
            {
                sender.Initialize(settings);
            }
            foreach (var receiver in _receivers.Values)
            {
                receiver.Initialize(settings);
            }

        }

        public virtual void Start()
        {
            foreach (var sender in _senders.Values)
            {
                sender.Start();
            }
            foreach (var receiver in _receivers.Values)
            {
                receiver.Start();
            }
        }

        public virtual void Stop()
        {
            foreach (var sender in _senders.Values)
            {
                sender.Stop();
            }
            foreach (var receiver in _receivers.Values)
            {
                receiver.Stop();
            }
        }


        public void Initialize()
        {
            Initialize(_settings);
        }

        public virtual string GetContainerNamespace()
        {
            return _pluginNamespace;
        }

        public void SetContainerNamespace(string pluginNamespace)
        {
            this._pluginNamespace = pluginNamespace;
        }

        #endregion
    }
}
