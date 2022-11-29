// main plugin logic

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Linq;
    using System.Threading;

    using Loupedeck.TotalMixPlugin.Actions;
    using Loupedeck.TotalMixPlugin.Models.Events;

    // This class contains the plugin-level logic of the Loupedeck plugin.

    public partial class TotalMixPlugin : Plugin
    {

        private readonly Thread _updateThread;
        public event EventHandler<TotalMixUpdatedSetting> UpdatedInputSetting;
        public event EventHandler<TotalMixUpdatedSetting> UpdatedOutputSetting;
        public event EventHandler<TotalMixUpdatedSetting> UpdatedPlaybackSetting;
        readonly Listener listener = new Listener();

        // Gets a value indicating whether this is an Universal plugin or an Application plugin.
        public override Boolean UsesApplicationApiOnly => true;

        // Gets a value indicating whether this is an API-only plugin.
        public override Boolean HasNoApplication => true;

        public TotalMixPlugin() => this._updateThread = new Thread(this.UpdateThread) { IsBackground = true };

        private void UpdateThread()
        {
            while (true)
            {
                try
                {
                    String[] busses = { "Input", "Output", "Playback" };
                    foreach (var bus in busses)
                    {
                        this.listener.Listen(bus, $"/1/bus{bus}", 1).Wait();

                        for (Int32 i = 0; i < Globals.recentUpdates[bus].Count; i++)
                        {
                            this.FireEvent(bus, i);
                        }
                    }
                }
                catch
                {
                    //
                }
                System.Threading.Thread.Sleep(50);
            }
        }

        private void FireEvent(String bus, Int32 index)
        {
            if (bus == "Input")
            {
                if (Globals.recentUpdates[bus].ElementAt(index).Key.Contains("main") || Globals.recentUpdates[bus].ElementAt(index).Key.Contains("global"))
                {
                    var TotalMixUpdatedEventArgsGlobal = new TotalMixUpdatedSetting($"{Globals.recentUpdates[bus].ElementAt(index).Key}");
                    UpdatedInputSetting?.Invoke(this, TotalMixUpdatedEventArgsGlobal);
                }
                else
                {
                    var TotalMixUpdatedEventArgs = new TotalMixUpdatedSetting($"{Globals.recentUpdates[bus].ElementAt(index).Key}|{Globals.recentUpdates[bus].ElementAt(index).Value}");
                    UpdatedInputSetting?.Invoke(this, TotalMixUpdatedEventArgs);
                }
            }
            if (bus == "Output")
            {
                var TotalMixUpdatedEventArgs = new TotalMixUpdatedSetting($"{Globals.recentUpdates[bus].ElementAt(index).Key}|{Globals.recentUpdates[bus].ElementAt(index).Value}");
                UpdatedOutputSetting?.Invoke(this, TotalMixUpdatedEventArgs);
            }
            if (bus == "Playback")
            {
                var TotalMixUpdatedEventArgs = new TotalMixUpdatedSetting($"{Globals.recentUpdates[bus].ElementAt(index).Key}|{Globals.recentUpdates[bus].ElementAt(index).Value}");
                UpdatedPlaybackSetting?.Invoke(this, TotalMixUpdatedEventArgs);
            }
            Globals.recentUpdates[bus].Remove(Globals.recentUpdates[bus].ElementAt(index).Key);
        }

        // This method is called when the plugin is loaded during the Loupedeck service start-up.
        public override void Load()
        {
            this.LoadPluginIcons();
            // calling Installer (which basically creates the default settings for the device connectivity if it shouldn't exist
            this.Install();
            // getting the pluginDataDirectory to fill the Global variables with the data from the settings file during startup, so that users can change them
            var helperFunction = new TotalMixPlugin();
            var pluginDataDirectory = helperFunction.GetPluginDataDirectory();
            var helper = new HelperFunctions();
            helper.GetDeviceIp(pluginDataDirectory);
            helper.GetDevicePort(pluginDataDirectory);
            helper.GetDeviceSendPort(pluginDataDirectory);
            helper.GetDeviceBackgroundPort(pluginDataDirectory);
            helper.GetDeviceBackgroundSendPort(pluginDataDirectory);
            helper.GetDeviceMirroringRequested(pluginDataDirectory);
            helper.GetSkipDeviceChecks(pluginDataDirectory);
            if (Globals.skipChecks == "false")
            {
                var state = helper.CheckForTotalMix();
                if (state == 2)
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected", null, null);
                }
                else
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "TotalMixFX doesn't seem to be running or OSC isn't setup correctly in TotalMixFX for this plugin to work", "https://github.com/shells-dw/loupedeck-totalmix#setup-for-osc", "See github page for instructions on how to set up TotalMix");
                }
            }
            // filling the Global Dict bankSettings initially
            this.listener.Listen("Input", "/1/busInput", 1).Wait();
            this.listener.Listen("Output", "/1/busOutput", 1).Wait();
            this.listener.Listen("Playback", "/1/busPlayback", 1).Wait();
            if (Globals.mirroringRequested == "true")
            {
                this._updateThread.Start();
            }

            // making sure, we're actually on channel 1 (as further actions rely on it and there is no easy way to figure out which channel is currently active during runtime)
            for (var c = 1; c < 16; c++)
            {
                HelperFunctions.SendOscCommand($"/2/track-", 1);
            }
        }
        // This method is called when the plugin is unloaded during the Loupedeck service shutdown.
        public override void Unload() => this._updateThread.Interrupt();

        // setting plugin icons
        private void LoadPluginIcons()
        {
            this.Info.Icon16x16 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("icon16.png"));
            this.Info.Icon32x32 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("icon32.png"));
            this.Info.Icon48x48 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("icon48.png"));
            this.Info.Icon256x256 = EmbeddedResources.ReadImage(EmbeddedResources.FindFile("icon256.png"));
        }
    }
}