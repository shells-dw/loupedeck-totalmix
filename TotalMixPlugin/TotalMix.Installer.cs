// installer class basically creating the default settings if they should not exist (usually during the very first start)

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.IO;
    using System.Reflection;

    using Newtonsoft.Json.Linq;

    public partial class TotalMixPlugin
    {
        readonly String[] expectedSettings = { "host", "port", "sendPort", "backgroundPort", "backgroundSendPort", "mirroringEnabled", "skipChecks", "enableLogging" };
        public override Boolean Install()
        {
            // Here we ensure the plugin data directory is there.
            var _this = new TotalMixPlugin();
            var pluginDataDirectory = _this.GetPluginDataDirectory();
            if (!IoHelpers.EnsureDirectoryExists(pluginDataDirectory))
            {
                _this.Log.Info($"{DateTime.Now} - Installer: pluginDataDirectory doesn't exist");
                return false;
            }

            // Now we put a template configuration file
            var filePath = System.IO.Path.Combine(pluginDataDirectory, System.IO.Path.Combine(pluginDataDirectory, "settings.json"));
            if (File.Exists(filePath))
            {
                _this.Log.Info($"{DateTime.Now} - Installer: File exists");
                var json = File.ReadAllText(filePath);
                var jsonObj = JObject.Parse(json);
                foreach (var setting in this.expectedSettings)
                {
                    if (!jsonObj.ContainsKey(setting))
                    {
                        String value = "";
                        switch (setting)
                        {
                            case "host":
                                value = "127.0.0.1";
                                break;
                            case "port":
                                value = "7001";
                                break;
                            case "sendPort":
                                value = "9001";
                                break;
                            case "backgroundPort":
                                value = "7002";
                                break;
                            case "backgroundSendPort":
                                value = "9002";
                                break;
                            case "mirroringEnabled":
                                value = "true";
                                break;
                            case "skipChecks":
                                value = "false";
                                break;
                        }
                        jsonObj.Add(setting, value);
                        File.WriteAllText(filePath, jsonObj.ToString());
                    }
                }
            }
            else
            {
                ResourceReader.CreateFileFromResource("Loupedeck.TotalMixPlugin.settings.json", filePath);
                _this.Log.Info($"{DateTime.Now} - Installer: config file does not yet exist, creating default file");
            }
            return true;
        }
        public class ResourceReader
        {
            // to read the file as a Stream
            public static Stream GetResourceStream(String resourceName)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
                return resourceStream;
            }

            // to save the resource to a file
            public static void CreateFileFromResource(String resourceName, String path)
            {
                Stream resourceStream = GetResourceStream(resourceName);
                if (resourceStream != null)
                {
                    using (Stream input = resourceStream)
                    {
                        using (Stream output = File.Create(path))
                        {
                            input.CopyTo(output);
                        }
                    }
                }
            }
        }
    }
}