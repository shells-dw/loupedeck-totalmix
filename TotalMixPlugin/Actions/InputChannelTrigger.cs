// dial action for Output channels

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    using Loupedeck.TotalMixPlugin.Actions;


    public class InputChannelTrigger : PluginDynamicCommand
    {
        // assign variables
        private TotalMixPlugin _plugin;
        private readonly Boolean[] _mute = new Boolean[17];
        readonly String bus = "Input";
        private Boolean currentState;
        private Boolean setToState;
        String action;

        // build the action
        public InputChannelTrigger() : base("Input Channel Buttons", "Options for Input Channels", "Input Channels")
        {
            this.MakeProfileAction("tree");
        }
        protected override PluginProfileActionData GetProfileActionData()
        {
            var tree = new PluginProfileActionTree("Select channel and action to trigger");
            tree.AddLevel("Channel");
            tree.AddLevel("Action");

            // there are 16 channels available (in my RME Interface and any I could get my hands on anyway), so creating 16 Channels with the respective options to select from
            for (var i = 1; i < 17; i++)
            {
                var node = tree.Root.AddNode($"Channel {i}");
                node.SetPropertyValue("Channel", "Channel {i}");
                node.AddItem($"mute|{i}", "Mute");
                node.AddItem($"solo|{i}", "Solo");
                node.AddItem($"phantom|{i}", "Phantom Power");
                node.AddItem($"phase|{i}", "Phase", "left phase on stereo channels");
                node.AddItem($"phaseRight|{i}", "Phase Right", "(on stereo channels only)");
                node.AddItem($"instrument|{i}", "Instrument", "Enables Hi-Z input on channels supporting that");
                node.AddItem($"pad|{i}", "Pad");
                node.AddItem($"msProc|{i}", "Mid/Side Processing");
            }
            return tree;
        }

        protected override bool OnLoad()
        {
            _plugin = base.Plugin as TotalMixPlugin;
            if (_plugin is null)
                return false;

            _plugin.UpdatedInputSetting += (sender, e) => this.ActionImageChanged(e.Address);
            return base.OnLoad();
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
                        Task.Run(() => HelperFunctions.SendOscCommand($"/1/bus{this.bus}", 1)).GetAwaiter().GetResult();
                        Task.Run(() => HelperFunctions.SendOscCommand($"/1/{this.action}/1/{channel}", this.setToState ? 1 : 0)).GetAwaiter().GetResult();
                    }
                    // everything else is handled on a sole per-channel-basis, meaning there is no global address, every channel needs to be addressed
                    // but don't think you could just go ahead and be like "change setting on channel x", nah-ah. without being able to know which channel is currently active, channels have to be switched one by one
                    // hence I'm making sure during initialization that channel 1 is the channel it always sits on. e.g.: channel 4 phase should be changed means running "/2/track+" 3 times (coming from channel 1), then the value is to be set (hoping we're on the right channel), then track- has to be run 3 times again to leave it at channel 1 for the next change.
                    // gotta love it.
                    else
                    {
                        Task.Run(() => HelperFunctions.SendOscCommand($"/1/bus{this.bus}", 1)).GetAwaiter().GetResult();

                        for (var c = 1; c < Int32.Parse(channel); c++)
                        {
                            Task.Run(() => HelperFunctions.SendOscCommand($"/2/track+", 1)).GetAwaiter().GetResult();
                        }
                        Task.Run(() => HelperFunctions.SendOscCommand($"/2/{this.action}", 1)).GetAwaiter().GetResult();
                        for (var c = 1; c < Int32.Parse(channel); c++)
                        {
                            Task.Run(() => HelperFunctions.SendOscCommand($"/2/track-", 1)).GetAwaiter().GetResult();
                        }
                    }
                    this.ActionImageChanged(actionParameter); // Notify the Loupedeck service that the actionImage has changed.
                }
                catch
                {
                    //
                }
            }
        }

        // update command image
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
        {
            if (this.Plugin.PluginStatus.Status.ToString() != "Normal")
            {
                using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    //drawing a black full-size rectangle to overlay the default graphic (TODO: figure out if that's maybe something that is done nicer than this)
                    bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.White);
                    bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.White);

                    // draw icons for different cases
                    bitmapBuilder.DrawText("⚠️", x: 45, y: 10, width: 70, height: 70, BitmapColor.Black, fontSize: 60);
                    bitmapBuilder.DrawText("Error", x: 5, y: 50, width: 70, height: 40, fontSize: 20, color: BitmapColor.Black);
                    return bitmapBuilder.ToImage();
                }
            }
            // splitting channel and action
            var action = actionParameter.Split("|")[0];
            var channel = actionParameter.Split("|")[1];
            String trackname = "";

            // handling these special as these have different active and inactive images. Getting current value, setting if not present
            if (action == "mute" || action == "solo" || action == "phantom")
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
                }
                catch
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
                        case "phantom":
                            bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("phantomOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("phantomOff80.png")));
                            bitmapBuilder.DrawText(trackname, x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                            break;
                        default:
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawRectangle(0, 40, 80, 40, BitmapColor.Black);
                            bitmapBuilder.FillRectangle(0, 40, 80, 40, BitmapColor.Black);
                            bitmapBuilder.DrawText($"{action}\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White);
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
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"Inst. (Hi-Z)\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "pad":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"Pad\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        case "msProc":
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"M/S Proc\n{trackname}", x: 10, y: 50, width: 60, height: 20, BitmapColor.White, fontSize: 13);
                            break;
                        default:
                            bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                            bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                            bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                            bitmapBuilder.DrawText($"{action}\n{trackname}", x: 5, y: 40, width: 70, height: 40, fontSize: 13, color: BitmapColor.White);
                            break;
                    }
                    return bitmapBuilder.ToImage();
                }

            }
        }

    }
}
