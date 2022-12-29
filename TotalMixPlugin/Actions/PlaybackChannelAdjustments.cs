// dial action for Playback channels

namespace Loupedeck.TotalMixPlugin
{
    using System;

    public class PlaybackChannelAdjustments : PluginDynamicAdjustment
    {
        // assign variables
        private TotalMixPlugin _plugin;
        readonly String bus = "Playback";
        String action;
        private Single valueFloat;

        // build the action
        public PlaybackChannelAdjustments() : base(displayName: "Playback Channel Dials", description: "Options for Playback Channels", groupName: "Playback Channel Dials", hasReset: false) => this.MakeProfileAction("tree");
        protected override PluginProfileActionData GetProfileActionData()
        {
            var tree = new PluginProfileActionTree("Select channel and action to trigger");
            tree.AddLevel("Channel");
            tree.AddLevel("Action");

            // create channels as per how many channels were detected
            for (var i = 1; i < Globals.channelCount + 1; i++)
            {
                var node = tree.Root.AddNode($"Channel {i}");
                node.SetPropertyValue("Channel", "Channel {i}");
                node.AddItem($"volume|{i}", "Volume");
                node.AddItem($"pan|{i}", "Pan");
            }

            return tree;
        }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as TotalMixPlugin;
            if (this._plugin is null)
            {
                return false;
            }

            this._plugin.UpdatedPlaybackSetting += (sender, e) => this.ActionImageChanged(e.Address);
            return base.OnLoad();
        }

        // This method is called when the dial associated to the plugin is rotated.
        protected override void ApplyAdjustment(String actionParameter, Int32 turn)
        {
            this.Log.Info($"{DateTime.Now} - TotalMix: PlaybackChannelAdjustments _ RunCommand {actionParameter}");
            // check action parameter is actually set
            if (actionParameter.Contains("|"))
            {
                // splitting channel and action
                this.action = actionParameter.Split("|")[0];
                var channel = actionParameter.Split("|")[1];


                try
                {
                    Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{this.action}{channel}", out var value);
                    this.valueFloat = Convert.ToSingle(value);

                    // handling these actions individually due to different scales each turn is adding
                    if (this.action == "volume")
                    {
                        Single tmp = (Single)turn / (Single)115;
                        this.valueFloat += tmp;
                        this.valueFloat = (Single)Math.Max(0.0, (Single)Math.Min(1.0, this.valueFloat));
                    }
                    if (this.action == "pan")
                    {
                        Single tmp = (Single)turn / (Single)200;
                        this.valueFloat += tmp;
                        this.valueFloat = (Single)Math.Max(0.0, (Single)Math.Min(1.0, this.valueFloat));
                    }

                    // update Global Dict with new setting
                    Globals.bankSettings[$"{this.bus}"][$"/1/{this.action}{channel}"] = this.valueFloat.ToString();

                    // making sure we're on the right bus
                    Sender.Send($"/1/bus{this.bus}", 1, Globals.interfaceIp, Globals.interfacePort);

                    // make it so
                    Sender.Send($"/1/{this.action}{channel}", this.valueFloat, Globals.interfaceIp, Globals.interfacePort);
                    this.AdjustmentValueChanged(actionParameter); // Notify the Loupedeck service that the adjustment value has changed.
                }
                catch (Exception ex)
                {
                    this.Log.Error($"{DateTime.Now} - TotalMix: PlaybackChannelAdjustments _ Exception {ex}");
                }
            }

        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => this.GetValue(actionParameter);

        private String GetValue(String actionParameter)
        {
            if (actionParameter.Contains("|"))
            {
                var action = actionParameter.Split("|")[0];
                var channel = actionParameter.Split("|")[1];
                try
                {
                    // handling these differently due to different scales
                    if (action == "volume")
                    {
                        try
                        {
                            Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{action}{channel}Val", out var outputValue);
                            return outputValue;
                        }
                        catch (Exception ex)
                        {
                            this.Log.Warning($"{DateTime.Now} - TotalMix: Dial _ Bus: {this.bus} _ Address: /1/{action}{channel}Val _ Error: {ex.Message}");
                            return "\t❌";
                        }
                    }
                    // pan uses simply 200 linear steps, that's finally easy ;)
                    if (action == "pan")
                    {
                        try
                        {
                            Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{action}{channel}", out var outputValue);
                            var singleOutputValue = Convert.ToSingle(outputValue);
                            var floatRange = 0.5 - 0;
                            var intRange = 0 - 100;
                            var newValue = (singleOutputValue - 0) * intRange / floatRange + 100;
                            if (singleOutputValue == 0.5)
                            {
                                outputValue = "< o >";
                            }
                            if (singleOutputValue < 0.5)
                            {
                                outputValue = $"< {Math.Round(newValue)}";
                            }
                            if (singleOutputValue > 0.5)
                            {
                                outputValue = $"{Math.Round(newValue * -1)} >";
                            }
                            return outputValue;
                        }
                        catch (Exception ex)
                        {
                            this.Log.Warning($"{DateTime.Now} - TotalMix: Dial _ Bus: {this.bus} _ Address: /1/{action}{channel}Val _ Error: {ex.Message}");
                            return "\t❌";
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Log.Error($"{DateTime.Now} - TotalMix: Dial _ Bus: {this.bus} _ Address: /1/{action}{channel}Val _ Error: {ex.Message}");
                }
            }
            return "err";
        }

        // drawing what is actually shown on the device (overwriting defaults to make it more useful by including channel names and friendly descriptors)
        protected override BitmapImage GetAdjustmentImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this.Plugin.PluginStatus.Status.ToString() != "Normal")
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    //drawing a black full-size rectangle to overlay the default graphic (TODO: figure out if that's maybe something that is done nicer than this)
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);

                    // draw icons for different cases
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerRed80.png")));
                    bitmapBuilder.DrawText("No Connection", x: 5, y: 50, width: 70, height: 20, fontSize: 10, color: BitmapColor.White);
                    return bitmapBuilder.ToImage();
                }
            }
            // checking for then splitting actionParameter as before
            if (actionParameter != null)
            {
                var action = actionParameter.Split("|")[0];
                var channel = actionParameter.Split("|")[1];
                String dispAction;
                String trackname = "";

                // check if Global Dict is filled and if so, extract trackname aka channel name
                try
                {
                    Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/trackname{channel}", out trackname);
                } catch
                {
                    //
                }

                // friendly names instead of technical descriptors
                switch (action)
                {
                    case "volume":
                        dispAction = "Volume";
                        break;
                    case "pan":
                        dispAction = "Pan";
                        break;
                    default:
                        dispAction = action;
                        break;
                }

                // draw friendly name and channel name next to the dial
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.DrawText($"{dispAction} {trackname}", x: 10, y: 10, width: 30, height: 30, fontSize: 14, color: BitmapColor.White);

                    return bitmapBuilder.ToImage();
                }
            }

            // just to catch if the actionParameter should be empty, not sure if needed (TODO: figure that out)
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.DrawText($"unknown", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);

                return bitmapBuilder.ToImage();
            }
        }
    }
}

