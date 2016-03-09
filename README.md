#IotGateway Sample#
**[Note: this sample depends on OPC UA .NET client libraries from https://www.unified-automation.com/; these libraries need to be downloaded first before the IotGateway can be compiled. In future I plan to integrate with OPC Foundation's reference implementation libraries]**

* **Information about OPC UA can be found at https://opcfoundation.org/**
* **Information about Azure IoT and IoTHub can be found at https://azure.microsoft.com/en-us/services/iot-hub/ **

IoTGateway is a component framework for integrating IoT systems with Azure IoT Hub. The system uses a plugin model for writing receivers and senders. Out of the box, it supports OPC UA receiver and IoT Hub sender. The gateway is extensible through the plugin model; the plugins are hosted inside host containers which are derived from HostBase. All the plugins are registered into TypeContainer before the host containers are created and configured. The following are the list of projects and important abstractions in each of the projects: 

###IotGateway.Host.csproj###
This projects hosts the containers for each type of IoT messaging modality: telemetry, alarms, events, command and control. 

####TelmetryHost####
**TelemetryHost** implements logic for sending telemetry to IoT Hub; it hosts OPCReceiver (implements IReceiver marker and IProcessor) and IoTHubTelemetrySender (implements ISender and IProcessor) plugins. The TelemetryHost can be extended by replacing the receivers and senders. The host receives changes from the OPC UA object model and applies to in-memory state that is represented by a collection of tags organized into TagGroup objects based on sending frequency. The IoTHubTelemetrySender scans for changes every configurable number of seconds and sends asynchronously to IoT Hub. The sender uses a collection of devices as pool for scalability.  

####CommandHost####
**CommandHost** receives commands from IoT Hub (sent by a service-side process implemented as needed by the IoT application) and executes against OPC UA Server. Note that OpcUaCommandExecutor only writes the received command to the standard output. Work to be done to parse the command and apply it to the OPC UA object. 

####MessagingHost####
**MessagingHost** is not completely implemented; when done, the IReceiver implementation reads events and alarms from OPC UA or other IoT system and writes them to an implementation of IMessageExchange. The ISender implementation will read from the IMessageExchange and writes to IoT Hub. Multiple instances of MessagingHost containers can be implemented for instance to send alarms and events. 

###IotGateway.Core.csproj###
Contains core interfaces and some implementations. The interfaces will have to be separated into a project of its own and move implementations to IotGateway.Common. 

###IotGateway.Common.csproj###
Contains utility components used across all the projects.

###IotGateway.Config.csproj###
Contains configuration components for reading tag configuration, OPC server configuration and device pool information. 

###IotGateway.PlugIn.IotHub###
Contains all the plugins that depend on IoT Hub component library - Microsoft.Azure.Devices.Client library. The components include senders, receivers, and message converters. 

###IotGateway.PlugIn.OpcUa###
Contains all the plugins that depend on OPC UA which include the receiver, subscription handler, tag converter and command executor. 

###IotGateway.Typed.ConsoleRunner###
This test harness composes TypeContainer by directly referencing the plugins.

###IotGateway.Reflection.ConsoleRunner###
This test harness composes TypeContainer from XML file that is compatible with .NET reflection format. This XML file can be easily be generated by initially composing TypeContainer by direct references and then calling TypeContainer.GetRegistryAsXml();









 

