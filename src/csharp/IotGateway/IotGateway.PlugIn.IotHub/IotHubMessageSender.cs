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
using IotGateway.Common;
using IotGateway.Config;
using IotGateway.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;

namespace IotGateway.PlugIn.IotHub
{
    public class IotHubMessageSender<TExchangeMsg> : IProcessor, ISender
    {
        private Dictionary<string, object> _settings;
        private IotHubConfig _iotHubConfig;
        private IMessageExchange<TExchangeMsg> _alarmMessageExchange;
        private IConverter<TExchangeMsg, object> _toIotHubDataConverter;
        private ILogger _logger; 
        private DeviceClientPool _deviceClienPool;
        private bool _stopCalled = false;

        public void Initialize(Dictionary<string, object> settings)
        {
            string[] fieldNames = new string[] { "IotHubConfigFile", "MessageConverter.OpcUa", "MessageExchange.Messaging", "MessageConverter.IotHub", "Logger" };
            FieldValidator.CheckMissingFields(settings, fieldNames);
            string iotHubConfigFile = settings.GetValueOrNull("IotHubConfigFile") as string;
            _iotHubConfig = ConfigUtility.ReadConfig<IotHubConfig>(iotHubConfigFile);
            _toIotHubDataConverter = settings.GetValueOrNull("MessageConverter.IotHub") as IConverter<TExchangeMsg, object>;
            _alarmMessageExchange = settings.GetValueOrNull("MessageExchange.Messaging") as IMessageExchange<TExchangeMsg>;
            _logger = settings.GetValueOrNull("Logger") as ILogger;
            _deviceClienPool = new DeviceClientPool();
            _deviceClienPool.Initialize(_iotHubConfig.IotHubHostName, _iotHubConfig.AlarmDevicePool);
        }
        public async void Start()
        {
            while (!this._stopCalled)
            {
                if (this._stopCalled)
                {
                    break;
                }

                Message deviceMessage = null; 
                TExchangeMsg inputMsg = default(TExchangeMsg);
                if (this._alarmMessageExchange.IsEmpty())
                {
                    await Task.Delay(2000);
                }
                else
                {
                    inputMsg = _alarmMessageExchange.Read();

                    deviceMessage = (Message)_toIotHubDataConverter.Convert(inputMsg);

                    if (deviceMessage == null) continue;
                    try
                    {
                        DeviceClient deviceClient = _deviceClienPool.GetDeviceClient();
                        await deviceClient.SendEventAsync(deviceMessage);
                    }
                    catch (ApplicationException exp)
                    {
                        WriteLog(string.Format("EventHub.SendEventHubMessage failed with excepiton: Message: {0}, Stacktrace: {1}", exp.Message, exp.StackTrace));
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
        public void WriteLog(string msg)
        {
            if (_logger == null) return;
            _logger.Write(msg);
        }
    }
}
