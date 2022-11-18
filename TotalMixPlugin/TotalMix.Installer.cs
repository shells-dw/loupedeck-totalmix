// installer class basically creating the default settings if they should not exist (usually during the very first start)

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.IO;
    using Newtonsoft.Json;

    class Device
    {
        public String host;
        public String port;
        public String sendPort;
        public String backgroundPort;
        public String backgroundSendPort;
        public String mirroringEnabled;
    }
    public partial class TotalMixPlugin
    {
        public String ClientConfigurationFilePath => System.IO.Path.Combine(this.GetPluginDataDirectory(), "settings.json");

        public override Boolean Install()
        {
            // Here we ensure the plugin data directory is there.
            var pluginDataDirectory = this.GetPluginDataDirectory();
            if (!IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                Tracer.Error("Plugin data is not created. Cannot continue installation");
                return false;
            }

            // Now we put a template configuration file
            var filePath = System.IO.Path.Combine(pluginDataDirectory, this.ClientConfigurationFilePath);
            if (File.Exists(filePath))
            {
                return true;
            }
            using (var streamWriter = new System.IO.StreamWriter(filePath))
            {
                var device = new Device
                {
                    host = "127.0.0.1",
                    port = "7001",
                    sendPort = "9001",
                    backgroundPort = "7002",
                    backgroundSendPort = "9002",
                    mirroringEnabled = "true"
                };

                var output = JsonConvert.SerializeObject(device, Formatting.Indented);
                Device deserializedDevice = JsonConvert.DeserializeObject<Device>(output);
                var serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(streamWriter, deserializedDevice);
            }
            return true;
        }
    }
}