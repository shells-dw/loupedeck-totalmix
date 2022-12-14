// dial action for Output channels

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Loupedeck.TotalMixPlugin.Actions;

    public class InputChannelAdjustments : PluginDynamicAdjustment
    {
        // assign variables
        private TotalMixPlugin _plugin;
        readonly String bus = "Input";
        String action;
        private Single valueFloat;

        // build the action
        public InputChannelAdjustments() : base(displayName: "Input Channel Dials", description: "Options for Input Channels", groupName: "Input Channel Dials", hasReset: false) => this.MakeProfileAction("tree");
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
                node.AddItem($"micgain|{i}", "Gain");
            }
            return tree;
        }
        protected override bool OnLoad()
        {
            this._plugin = base.Plugin as TotalMixPlugin;
            if (this._plugin is null)
            {
                return false;
            }

            this._plugin.UpdatedInputSetting += (sender, e) => this.AdjustmentValueChanged(e.Address);
            return base.OnLoad();
        }

        // This method is called when the dial associated to the plugin is rotated.
        protected override void ApplyAdjustment(String actionParameter, Int32 turn)
        {
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
                    if (this.action == "micgain")
                    {
                        Single gaintmp = (Single)turn / (Single)65;
                        this.valueFloat += gaintmp;
                        this.valueFloat = (Single)Math.Max(0.14, (Single)Math.Min(1.0, this.valueFloat));
                    }
                    // update Global Dict with new setting
                    Globals.bankSettings[$"{this.bus}"][$"/1/{this.action}{channel}"] = this.valueFloat.ToString();

                    // make it s0
                    Sender.Send($"/1/bus{this.bus}", 1, Globals.interfaceIp, Globals.interfacePort);
                    Sender.Send($"/1/{this.action}{channel}", this.valueFloat, Globals.interfaceIp, Globals.interfacePort);
                    this.ActionImageChanged(actionParameter); // Notify the Loupedeck service that the adjustment image has changed.
                    this.AdjustmentValueChanged(actionParameter); // Notify the Loupedeck service that the adjustment value has changed.
                }
                catch
                {
                    //
                }
            }
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter) => this.GetValue(actionParameter);

        private String GetValue(String actionParameter)
        {
            // checking actionParameter, splitting it, checking for Global Dict availability
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
                        catch
                        {
                            return "\t❌";
                        }
                    }

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
                        catch
                        {
                            return "\t❌";
                        }
                    }


                    // micgain uses simply 65 linear steps, that's easy ;)
                    /* however only on Mic inputs. Line inputs have only 18 steps, but there is no way to programmatically tell which is which (that I know of)
                    * also if the interface has 2 Mic and 2 Line inputs, these are channels 1 and 2 for Mic and channels 3 and 4 for line.
                    * your interface may have 8 and 0 or 4 and 4 and there is no way to know that programmatically (that I know of)
                    * also, just to make it more unpredictable, when channels 1 and 2 are set to Stereo, they both become channel 1 and channel 2 disappears
                    * all subsequent channels are then counted one channel less than before, meaning what has been channel 3 is now channel 2, channel 4 is now channel 3 and so on.
                    * Obviously the same happens when a channel is set to mono again, just the other way around. Suddenly channel 8 is channel 9.
                    * it's a right mess, this, hence I'm leaving it on 65, as people mostly don't mess around with line gain that much anyway. Set it once and roll with it,
                    * different story for mic gain though, so keeping it so it fits that best.
                    * */
                    if (action == "micgain")
                    {
                        try
                        {
                            Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{action}{channel}", out var outputValue);
                            var singleOutputValue = Convert.ToSingle(outputValue);
                            var floatRange = 1 - 0.15384616;
                            var intRange = 65 - 10;
                            var newValue = (singleOutputValue - 0.15384616) * intRange / floatRange + 10;
                            if (newValue < 10)
                            {
                                return outputValue = "0 dB";
                            }
                            outputValue = $"{Math.Round(newValue)} dB";
                            return outputValue;
                        }
                        catch
                        {
                            return "\t❌";
                        }
                    }
                    // if none of the above, see what's in the Global Dict and use that
                    else
                    {
                        Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{action}{channel}Val", out var outputValue);
                        return outputValue;
                    }
                } catch
                {

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
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.White);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.White);

                    // draw icons for different cases
                    bitmapBuilder.DrawText("⚠️", x: 25, y: 0, width: 40, height: 40, BitmapColor.Black, fontSize: 30);
                    bitmapBuilder.DrawText("Error", x: 10, y: 25, width: 30, height: 30, fontSize: 14, color: BitmapColor.Black);
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
                } catch {
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
                    case "micgain":
                        dispAction = "Gain";
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

