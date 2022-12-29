// helper functions

namespace Loupedeck.TotalMixPlugin.Actions
{
    using System;
    using System.IO;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Linq;

    internal class HelperFunctions
    {
        // reads if logging was requested either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public String GetLoggingRequested(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
            var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
            var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
            _this.Log.Info($"{DateTime.Now} - TotalMix: enableLogging: {configFileSettings.enableLogging.Value}");
            return configFileSettings.enableLogging.Value;
        }
        // reads the interface Ip either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public String GetDeviceIp(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDeviceIp: {configDeviceIp}");
            return configDeviceIp;
        }

        // reads the interface port (to send to) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDevicePort(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDevicePort: {configDevicePort}");
            return configDevicePort;
        }

        // reads the interface send port (the port to listen for responses from the device) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDeviceSendPort(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDeviceSendPort: {configDeviceSendPort}");
            return configDeviceSendPort;
        }

        public Int32 GetDeviceBackgroundPort(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDeviceBackgroundPort: {configDeviceBackgroundPort}");
            return configDeviceBackgroundPort;
        }

        // reads the interface send port (the port to listen for responses from the device) either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public Int32 GetDeviceBackgroundSendPort(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDeviceBackgroundSendPort: {configDeviceBackgroundSendPort}");
            return configDeviceBackgroundSendPort;
        }

        // reads if mirroring of TotalMix is requested or not; reads either from the Global variable if it exists there - or from the local config file during startup and puts it in the Global variable so it's updated during every start of the Loupedeck if the local config file is changed - then returns it to the caller
        public String GetDeviceMirroringRequested(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
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
            _this.Log.Info($"{DateTime.Now} - TotalMix: configDeviceMirroring: {configDeviceMirroring}");
            return configDeviceMirroring;
        }
        // option to skip availability checks of TotalMix
        public String GetSkipDeviceChecks(String PluginDataDirectory)
        {
            var _this = new TotalMixPlugin();
            try
            {
                String configSkipChecks;
                if (Globals.skipChecks == null)
                {
                    var json = File.ReadAllText(Path.Combine(PluginDataDirectory, "settings.json"));
                    var configFileSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(json);
                    configSkipChecks = configFileSettings.skipChecks.Value;
                    Globals.skipChecks = configSkipChecks;
                }
                else
                {
                    configSkipChecks = Globals.skipChecks;
                }
                _this.Log.Info($"{DateTime.Now} - TotalMix: configSkipChecks: {configSkipChecks}");
                return configSkipChecks;
            }
            catch (Exception ex)
            {
                _this.Log.Error($"{DateTime.Now} - TotalMix: configSkipChecks: Exception {ex.Message}");
                return "false";
            }
        }
        public void GetChannelCount()
        {
            var _this = new TotalMixPlugin();
            try
            {
                Globals.channelCount = Globals.bankSettings["Input"].Where(d => d.Key.Contains("/1/pan")).ToDictionary(d => d.Key, d => d.Value).Count;
                _this.Log.Info($"{DateTime.Now} - TotalMix: Globals.channelCount: {Globals.channelCount}");
            }
            catch (Exception ex)
            {
                Globals.channelCount = 16;
                _this.Log.Info($"{DateTime.Now} - TotalMix: Globals.channelCount: {Globals.channelCount} | Exception: {ex.Message}");
            }
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
            var _this = new TotalMixPlugin();
            var checkDevice = new CheckDevice();
            Task.Run(() => checkDevice.Listen(Globals.interfaceBackgroundSendPort)).Wait();
            _this.Log.Info($"{DateTime.Now} - TotalMix: CheckForTotalMix: {Globals.deviceState}");
            return Globals.deviceState;
        }
    }
}
