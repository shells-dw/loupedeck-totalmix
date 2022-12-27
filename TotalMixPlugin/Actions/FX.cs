namespace Loupedeck.TotalMixPlugin
{
    using System;


    public class FX : PluginDynamicCommand
    {
        // Assign variables
        private TotalMixPlugin _plugin;
        private Boolean currentState;
        private Boolean setToState;
        private Int32 setToStateInt;
        // build action
        public FX() : base()
        {
            this.AddParameter("globalMute", "Global Mute", groupName: "Global Functions");
            this.AddParameter("globalSolo", "Global Solo", groupName: "Global Functions");
            this.AddParameter("reverbEnable", "Enable Reverb", groupName: "Global Functions");
            this.AddParameter("echoEnable", "Enable Echo", groupName: "Global Functions");
        }

        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as TotalMixPlugin;
            if (this._plugin is null)
            {
                return false;
            }

            this._plugin.UpdatedGlobalSetting += (sender, e) => this.ActionImageChanged(e.Address);
            return base.OnLoad();
        }

        // Button is pressed
        protected override void RunCommand(String actionParameter)
        {
            try
            {
                Globals.bankSettings["global"].TryGetValue($"/3/{actionParameter}", out var value);
                this.currentState = Convert.ToBoolean(Int32.Parse(value));
                this.setToState = !this.currentState;
            }
            catch
            {
                this.setToState = true;
            }
            this.setToStateInt = Convert.ToInt32(this.setToState);

            // send to TotalMix
            Sender.Send($"/3/{actionParameter}", 1, Globals.interfaceIp, Globals.interfacePort);

            // update Global variable
            Globals.bankSettings["global"][$"/3/{actionParameter}"] = this.setToStateInt.ToString();

            // notify Loupedeck about the change
            this.ActionImageChanged(actionParameter);

        }

        // update command image (nothing to update here per se, but that's called to draw whatever is shown on the Loupedeck)
        protected override BitmapImage GetCommandImage(String actionParameter, PluginImageSize imageSize)
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
            } // getting the currentState from Global variable
            try
            {
                Globals.bankSettings["global"].TryGetValue($"/3/{actionParameter}", out var value);
                this.currentState = value != null && Convert.ToBoolean(Int32.Parse(value));
            }
            catch
            {

            }
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                switch (actionParameter)
                {
                    case "globalMute":
                        bitmapBuilder.DrawText("Global Mute", x: 0, y: 55, width: 80, height: 15, BitmapColor.White);
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteOff80.png")));
                        break;
                    case "globalSolo":
                        bitmapBuilder.DrawText("Global Solo", x: 0, y: 55, width: 80, height: 15, BitmapColor.White);
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("soloOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("soloOff80.png")));
                        break;
                    case "reverbEnable":
                        bitmapBuilder.DrawText("Reverb", x: 0, y: 55, width: 80, height: 15, BitmapColor.White);
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("reverbOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("reverbOff80.png")));
                        break;
                    case "echoEnable":
                        bitmapBuilder.DrawText("Echo", x: 0, y: 55, width: 80, height: 15, BitmapColor.White);
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("echoOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("echoOff80.png")));
                        break;
                    default:
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerGreen80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerRed80.png")));
                        bitmapBuilder.DrawRectangle(0, 40, 80, 40, BitmapColor.Black);
                        bitmapBuilder.FillRectangle(0, 40, 80, 40, BitmapColor.Black);
                        bitmapBuilder.DrawText(actionParameter, x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                }
                return bitmapBuilder.ToImage();
            }
        }
    }
}
