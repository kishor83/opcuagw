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
using IotGateway.Config;
using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotGateway.PlugIn.IotHub
{/// <summary>
/// Implements device client pool for scalable IotHub netowrk interaction
/// </summary>
    public class DeviceClientPool
    {
        private ConcurrentDictionary<int, DevicePoolConfigItem> _devices;
        private ConcurrentDictionary<string, DeviceClient> _deviceClientPool; 

        public void Initialize(string hostName, List<DevicePoolConfigItem> devices)
        {
            Initialize(hostName, TransportType.Amqp, devices);
        }
        public void Initialize(string hostName, TransportType transportType, List<DevicePoolConfigItem> devices)
        {
            LoadDevices(devices);
            _deviceClientPool = new ConcurrentDictionary<string, DeviceClient>();
            foreach (var device in _devices)
            {
                DeviceClient deviceClient = DeviceClient.Create(hostName, new DeviceAuthenticationWithRegistrySymmetricKey(device.Value.DeviceId, device.Value.SasKey), transportType);
                _deviceClientPool.TryAdd(device.Value.DeviceId, deviceClient);
            }
        }

        public ConcurrentDictionary<string, DeviceClient> DeviceClients
        {
            get
            {
                return _deviceClientPool;
            }
        }
        private void LoadDevices(List<DevicePoolConfigItem> devices)
        {
            _devices = new ConcurrentDictionary<int, DevicePoolConfigItem>();
            for(int i=0; i<devices.Count; i++)
            {
                _devices.TryAdd(i, devices[i]);
            }
        }
        public DeviceClient GetDeviceClient(string deviceId)
        {
            DeviceClient deviceClient = null;
            if (!_deviceClientPool.TryGetValue(deviceId, out deviceClient))
            {
                return null; 
            }
            return deviceClient;
        }
        public DeviceClient GetDeviceClient()
        {
            if (_deviceClientPool.Count > 0)
            {
                int poolIx = new Random().Next(_devices.Count);
                DevicePoolConfigItem tempDevice;
                if (!_devices.TryGetValue(poolIx, out tempDevice))
                {
                    throw new ApplicationException("Serious error with device list: index error");
                }
                DeviceClient deviceClient = GetDeviceClient(tempDevice.DeviceId);
                if (deviceClient == null)
                {
                    throw new ApplicationException("Serious error with DeviceConnectionPool: mismatch between device records and device clients");
                }
                return deviceClient;
            }
            else return null;
        }
    }
}
