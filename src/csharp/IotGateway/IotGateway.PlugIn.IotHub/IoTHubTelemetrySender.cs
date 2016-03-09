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
using IotGateway.Core;
using IotGateway.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using DeviceMessage = Microsoft.Azure.Devices.Client.Message;
namespace IotGateway.PlugIn.IotHub
{

    public class IoTHubTelemetrySender<TMsg> : IProcessor, ISender where TMsg: Tag
    {
        private Dictionary<string, object> _settings;
        private TagStateManager _tagStateManager; 
        private IConverter<Tag, object> _dataConverter;
        private IotHubConfig _iotHubConfig;
        private DeviceClientPool _deviceClienPool; 
        private volatile bool _stopCalled = false;

        public void Initialize()
        {
            Initialize(_settings);
        }

        public void Initialize(Dictionary<string, object> settings)
        {
            string[] fieldNames = new string[] { "IotHubConfigFile", "TagStateManager", "TagToIotHubMessageConverter" };
            FieldValidator.CheckMissingFields(settings, fieldNames);
            string iotHubConfigFile = settings.GetValueOrNull("IotHubConfigFile") as string; 
            _iotHubConfig = ConfigUtility.ReadConfig<IotHubConfig>(iotHubConfigFile);
            _dataConverter = settings.GetValueOrNull("TagToIotHubMessageConverter") as IConverter<Tag, object>;
            _tagStateManager = settings.GetValueOrNull("TagStateManager") as TagStateManager;
            _deviceClienPool = new DeviceClientPool();
            _deviceClienPool.Initialize(_iotHubConfig.IotHubHostName, _iotHubConfig.TelemetryDevicePool);
        }

        public void Start()
        {
            List<Task> senderTaskList = new List<Task>();
            foreach (var tagGroup in _tagStateManager.TagGroups)
            {
                Action<object> tagSenderMethod = new Action<object>(SendTagGroupChanges);
                Task t = Task.Factory.StartNew(tagSenderMethod, tagGroup);
                senderTaskList.Add(t);
            }
            Task.WaitAll(senderTaskList.ToArray());
        }

        private async void SendTagGroupChanges(object tagGroup)
        {
            TagGroup tempTagGroup = tagGroup as TagGroup;
            if (tempTagGroup == null) return;
            int sendIntervalSeconds = tempTagGroup.Config.SendIntervalSeconds; 

            while (!this._stopCalled)
            {
                if (this._stopCalled)
                {
                    break;
                }
                DeviceMessage deviceMessage = null;

                //scan the tags to see if anything changed
                foreach(var tag in tempTagGroup.Tags)
                {
                    if (!tag.Value.IsChanged()) continue;
                    deviceMessage = (DeviceMessage)_dataConverter.Convert(tag.Value);
                    if (deviceMessage == null) continue;
                    try
                    {
                        DeviceClient deviceClient = _deviceClienPool.GetDeviceClient();
                        await deviceClient.SendEventAsync(deviceMessage);
                    }
                    catch (ApplicationException exp)
                    {
                        throw new ApplicationException("DeviceClient.SendEventAsync failed", exp);
                    }
                }
                await Task.Delay(tempTagGroup.Config.SendIntervalSeconds * 1000);
            }
        }

        public void Stop()
        {
            this._stopCalled = true; 
        }
    }
}
