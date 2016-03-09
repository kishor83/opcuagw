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
using IotGateway.Config;
using Microsoft.Azure.Devices.Client;

namespace IotGateway.PlugIn.IotHub
{
    /// <summary>
    /// Executes 
    /// </summary>
    /// <typeparam name="TCmd"></typeparam>
    public class IotHubCommandReceiver<TCmd> : IProcessor, IReceiver
    {
        private DeviceClientPool _deviceClientPool;
        private Dictionary<string, object> _settings;
        private IotHubConfig _iotHubConfig;
        private IConverter<string, TCmd> _cmdConverter;
        private ICommandExecutor<TCmd> _cmdExecutor;
        private ILogger _logger; 

        private volatile bool _stopCalled = false;
        public void Initialize()
        {
            Initialize(_settings);
        }

        public void Initialize(Dictionary<string, object> settings)
        {
            //ConfigureSettings(settings);
            string[] fieldNames = new string[] { "IotHubConfigFile", "Logger", "CmdExecutor", "CmdConverter" };
            FieldValidator.CheckMissingFields(settings, fieldNames);

            string iotHubConfigFile = settings.GetValueOrNull("IotHubConfigFile") as string;
            _iotHubConfig = ConfigUtility.ReadConfig<IotHubConfig>(iotHubConfigFile);
            _cmdConverter = settings.GetValueOrNull("CmdConverter") as IConverter<string, TCmd>;
            _cmdExecutor = settings.GetValueOrNull("CmdExecutor") as ICommandExecutor<TCmd>;
            _logger = settings.GetValueOrNull("Logger") as ILogger;

            _deviceClientPool = new DeviceClientPool();
            _deviceClientPool.Initialize(_iotHubConfig.IotHubHostName, _iotHubConfig.CommandDevicePool);
        }

        public void Start()
        {

            List<Task> senderTaskList = new List<Task>();
            foreach (var deviceClient in _deviceClientPool.DeviceClients)
            {
                {
                    Action<object> commandReceiver = new Action<object>(CommandReceiver);
                    Task t = Task.Factory.StartNew(commandReceiver, deviceClient.Value);
                    senderTaskList.Add(t);
                }
            }
            Task.WaitAll(senderTaskList.ToArray());
        }
        private async void CommandReceiver(object obj)
        {
            if (obj == null) return; 
            DeviceClient deviceClient = obj as DeviceClient;
            while (!this._stopCalled)
            {
                if (this._stopCalled)
                {
                    break;
                }

                Message receivedCommand = await deviceClient.ReceiveAsync();
                //filter any stray commands that are old due to system errors 
                if (receivedCommand != null)
                {
                    string cmdText = Encoding.ASCII.GetString(receivedCommand.GetBytes());
                    WriteLog(cmdText);
                    TCmd cmd = default(TCmd);
                    if (_cmdConverter == null) continue;
                    cmd = _cmdConverter.Convert(cmdText);
                    if (_cmdExecutor == null) continue;
                    if (_cmdExecutor.Execute(cmd))
                    {
                        await deviceClient.CompleteAsync(receivedCommand);
                    }
                }
            }
        }
        void WriteLog(string msg)
        {
            if (_logger == null) return; 
            _logger.Write(msg);
        }
        public void Stop()
        {
            this._stopCalled = true;
        }
    }
}
