// helper functions

namespace Loupedeck.TotalMixPlugin.Actions
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    internal class HelperFunctions
    {
        // reads the interface Ip either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public String GetDeviceIp(String PluginDataDirectory)
        {
            String configDeviceIp;
            if (Globals.interfaceIp == null)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDeviceIp = configFileSettings.host.Value;
                Globals.interfaceIp = configDeviceIp;
            }
            else
            {
                configDeviceIp = Globals.interfaceIp;
            }
            return configDeviceIp;
        }

        // reads the interface port (to send to) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDevicePort(String PluginDataDirectory)
        {
            Int32 configDevicePort;
            if (Globals.interfacePort == 0)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDevicePort = Int32.Parse(configFileSettings.port.Value);
                Globals.interfacePort = configDevicePort;
            }
            else
            {
                configDevicePort = Globals.interfacePort;
            }
            return configDevicePort;
        }

        // reads the interface send port (the port to listen for responses from the device) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDeviceSendPort(String PluginDataDirectory)
        {
            Int32 configDeviceSendPort;
            if (Globals.interfaceSendPort == 0)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDeviceSendPort = Int32.Parse(configFileSettings.sendPort.Value);
                Globals.interfaceSendPort = configDeviceSendPort;
            }
            else
            {
                configDeviceSendPort = Globals.interfacePort;
            }
            return configDeviceSendPort;
        }

        public Int32 GetDeviceBackgroundPort(String PluginDataDirectory)
        {
            Int32 configDeviceBackgroundPort;
            if (Globals.interfaceBackgroundPort == 0)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDeviceBackgroundPort = Int32.Parse(configFileSettings.backgroundPort.Value);
                Globals.interfaceBackgroundPort = configDeviceBackgroundPort;
            }
            else
            {
                configDeviceBackgroundPort = Globals.interfaceBackgroundPort;
            }
            return configDeviceBackgroundPort;
        }

        // reads the interface send port (the port to listen for responses from the device) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDeviceBackgroundSendPort(String PluginDataDirectory)
        {
            Int32 configDeviceBackgroundSendPort;
            if (Globals.interfaceBackgroundSendPort == 0)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDeviceBackgroundSendPort = Int32.Parse(configFileSettings.backgroundSendPort.Value);
                Globals.interfaceBackgroundSendPort = configDeviceBackgroundSendPort;
            }
            else
            {
                configDeviceBackgroundSendPort = Globals.interfaceBackgroundSendPort;
            }
            return configDeviceBackgroundSendPort;
        }

        // reads if mirroring of TotalMix is requested or not; reads either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public String GetDeviceMirroringRequested(String PluginDataDirectory)
        {
            String configDeviceMirroring;
            if (Globals.mirroringRequested == null)
            {
                var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                configDeviceMirroring = configFileSettings.mirroringEnabled.Value;
                Globals.mirroringRequested = configDeviceMirroring;
            }
            else
            {
                configDeviceMirroring = Globals.mirroringRequested;
            }
            return configDeviceMirroring;
        }
        // here the magic happens (imagine the sparkles and rainbows yourself) - kidding, just taking the address and value from the call, combining it with the Global variables and send that all to the interface
        public static void SendOscCommand(String name, Single value)
        {
            Sender.Send(name, value, Globals.interfaceIp, Globals.interfacePort);
        }

        // needed that for something... hmm
        public class SelectableEnumItem
        {
            public String Key { get; set; }
            public String Value { get; set; }
        }

        // setting up a List for the contents off the configfile
        public List<SelectableEnumItem> GetConfigFile(String PluginDataDirectory, String configFile)
        {
            var json = File.ReadAllText(Path.Combine(PluginDataDirectory, configFile));
            var configFileContents = JsonConvert.DeserializeObject<dynamic>(json).ToObject<List<SelectableEnumItem>>();
            return configFileContents;
        }

        public Int32 CheckForTotalMix()
        {
            var checkDevice = new CheckDevice();
            Task.Run(() => checkDevice.Listen(Globals.interfaceBackgroundSendPort)).Wait();
            return Globals.deviceState;
        }
    }
}
