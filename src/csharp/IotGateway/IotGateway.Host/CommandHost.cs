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
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeC;

namespace IotGateway.Host
{
    public class CommandHost:HostBase
    {

        public override void Initialize(Dictionary<string, object> settings)
        {
            string pluginNamespace = GetContainerNamespace();
            Debug.Assert(pluginNamespace != null);
            //load types specific to this host into TypeContainer
            TypeContainer tc = TypeContainer.Instance;
            IProcessor commandReceiver = tc.GetInstance<IReceiver>(pluginNamespace) as IProcessor;
            //configure reqired settings: "Logger", "CmdExecutor", "CmdConverter"
            //"IotHubConfigFile" is already part of the settings by this time
            settings["Logger"] = tc.GetInstance<ILogger>();
            settings["CmdExecutor"] = tc.GetInstance<ICommandExecutor<Command>>(pluginNamespace);
            settings["CmdConverter"] = tc.GetInstance <IConverter<string, Command>>(pluginNamespace);
            this._receivers.Add("CommandReceiver", commandReceiver);
            base.Initialize(settings);
        }
    }
}
