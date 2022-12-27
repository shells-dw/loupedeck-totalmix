// handling for Playback Channels

namespace Loupedeck.TotalMixPlugin
{
    using System;


    public class PlaybackChannelTrigger : PluginDynamicCommand
    {
        // assign variables
        private TotalMixPlugin _plugin;
        private readonly Boolean[] _mute = new Boolean[17];
        readonly String bus = "Playback";
        private Boolean currentState;
        private Boolean setToState;
        String action;

        // build the action
        public PlaybackChannelTrigger() : base("Playback Channel Buttons", "Options for Playback Channels", "Playback Channels")
        {
            this.MakeProfileAction("tree");
        }
        protected override PluginProfileActionData GetProfileActionData()
        {
            //build a tree
            var tree = new PluginProfileActionTree("Select channel and action to trigger");
            tree.AddLevel("Channel");
            tree.AddLevel("Action");

            // create channels as per how many channels were detected
            for (var i = 1; i < Globals.channelCount+1; i++)
            {
                var node = tree.Root.AddNode($"Channel {i}");
                node.SetPropertyValue("Channel", "Channel {i}");
                node.AddItem($"mute|{i}", "Mute");
                node.AddItem($"solo|{i}", "Solo");
                node.AddItem($"phase|{i}", "Phase", "left phase on stereo channels");
                node.AddItem($"phaseRight|{i}", "Phase Right", "(on stereo channels only)");
                node.AddItem($"pad|{i}", "Pad");
                node.AddItem($"msProc|{i}", "Mid/Side Processing");
                node.AddItem($"lowcutEnable|{i}", "Enable Lowcut");
                node.AddItem($"eqEnable|{i}", "Enable EQ");
                node.AddItem($"compexpEnable|{i}", "Enable Compressor/Extender");
                node.AddItem($"alevEnable|{i}", "Enable Autolevel");
            }
            return tree;
        }

        // button is pressed
        protected override void RunCommand(String actionParameter)
        {
            // check action parameter is actually set
            if (actionParameter.Contains("|"))
            {
                // splitting channel and action
                this.action = actionParameter.Split("|")[0];
                var channel = actionParameter.Split("|")[1];

                // make toggle
                Int32.TryParse(actionParameter, out var i);
                this._mute[i] = !this._mute[i];

                try
                {
                    // handling these actions differently because these have sort-of-global addresses
                    if (this.action == "mute" || this.action == "solo" || this.action == "phantom")
                    {
                        // catch current setting from Global variable, flipping it, updating it in the Global variable and sending it to the device (making sure we're on the right bus)
                        Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{this.action}/1/{channel}", out var value);
                        this.currentState = Convert.ToBoolean(Int32.Parse(value));
                        this.setToState = !this.currentState;
                        Int32 setToStateInt = Convert.ToInt32(this.setToState);
                        Globals.bankSettings[$"{this.bus}"][$"/1/{this.action}/1/{channel}"] = setToStateInt.ToString();

                        Sender.Send($"/1/bus{this.bus}", 1, Globals.interfaceIp, Globals.interfacePort);
                        Sender.Send($"/1/{this.action}/1/{channel}", this.setToState ? 1 : 0, Globals.interfaceIp, Globals.interfacePort);
                    }

                    // everything else is handled on a sole per-channel-basis, meaning there is no global address, every channel needs to be addressed individually
                    else
                    {
                        Sender.Send($"/1/bus{this.bus}", 1, Globals.interfaceIp, Globals.interfacePort);
                        Sender.Send($"/setBankStart", Int32.Parse(channel) - 1, Globals.interfaceIp, Globals.interfacePort);
                        Sender.Send($"/2/{this.action}", 1, Globals.interfaceIp, Globals.interfacePort);
                        Sender.Send($"/setBankStart", 0, Globals.interfaceIp, Globals.interfacePort);
                    }
                    // notify the Loupedeck there was an update
                    this.ActionImageChanged(actionParameter);
                }
                catch
                {
                    //
                }

            }
        }

        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as TotalMixPlugin;
            if (this._plugin is null)
                return false;

            this._plugin.UpdatedPlaybackSetting += (sender, e) => this.ActionImageChanged(e.Address);
            return base.OnLoad();
        }

        // update command image
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this.Plugin.PluginStatus.Status.ToString() != "Normal")
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    //drawing a black full-size rectangle to overlay the default graphic (TODO: figure out if that's maybe something that is done nicer than this)
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);

                    // no connection // error
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerRed80.png")));
                    bitmapBuilder.DrawText("No Connection", x: 5, y: 50, width: 70, height: 20, fontSize: 10, color: BitmapColor.White);
                    return bitmapBuilder.ToImage();
                }
            }
            // splitting channel and action
            var action = actionParameter.Split("|")[0];
            var channel = actionParameter.Split("|")[1];
            String trackname = "";

            // handling these special as these have different active and inactive images. Getting current value, setting if not present
            if (action == "mute" || action == "solo")
            {
                try
                {
                    Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/{action}/1/{channel}", out var value);
                    if (value != null)
                    {
                        this.currentState = Convert.ToBoolean(Int32.Parse(value));
                    }
                    else
                    {
                        this.currentState = false;
                    }

                    //get the trackname (what RME calls the channel name set in TotalMix)
                    Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/trackname{channel}", out trackname);
                } catch
                {
                    action = "\t❌";
                    trackname = "\t❌";
                }

                // drawing the actual icon
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    //drawing a black full-size rectangle to overlay the default graphic (TODO: figure out if that's maybe something that is done nicer than this)
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);

                    // draw icons for different cases
                    switch (action)
                    {
                        case "mute":
                            bitmapBuilder.DrawText(trackname, x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                            bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteOff80.png")));
                            break;
                        case "solo":
                            bitmapBuilder.DrawText(trackname, x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                            bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("soloOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("soloOff80.png")));
                            break;
                        default:
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawRectangle(0, 40, 80, 40, BitmapColor.Black);
                            bitmapBuilder.FillRectangle(0, 40, 80, 40, BitmapColor.Black);
                            bitmapBuilder.DrawText($"{action} {channel}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White);
                            break;

                    }
                    return bitmapBuilder.ToImage();
                }
            }

            // if the action wasn't having active/inactive images
            else
            {
                //get the trackname (what RME calls the channel name set in TotalMix)
                try
                {
                    Globals.bankSettings[$"{this.bus}"].TryGetValue($"/1/trackname{channel}", out trackname);
                }
                catch
                {
                    action = "\t❌";
                    trackname = "\t❌";
                }
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    //drawing a black full-size rectangle to overlay the default graphic (TODO: figure out if that's maybe something that is done nicer than this)
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);

                    // draw icons for different cases
                    switch (action)
                    {
                        case "phase":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("phaseOff80.png")));
                            bitmapBuilder.DrawText($"{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "phaseRight":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("phaseRightOff80.png")));
                            bitmapBuilder.DrawText($"{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "instrument":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("instrumentOff80.png")));
                            bitmapBuilder.DrawText($"{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "pad":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("padOff80.png")));
                            bitmapBuilder.DrawText($"{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "msProc":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("msProcOff80.png")));
                            bitmapBuilder.DrawText($"{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "lowcutEnable":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"Lowcut\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "eqEnable":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"EQ\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "compexpEnable":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"Comp/Exp\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "alevEnable":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"Autolevel\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        default:
                            bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                            bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"{action} {trackname}", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                            break;
                    }
                    return bitmapBuilder.ToImage();
                }

            }
        }

    }
}
