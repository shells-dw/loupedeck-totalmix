// dial action for master Volume

namespace Loupedeck.TotalMixPlugin
{
    using System;

    public class MasterVolumeAdjustments : PluginDynamicAdjustment
    {
        // assign variables
        private TotalMixPlugin _plugin;
        private Single valueFloat;

        // build the action
        public MasterVolumeAdjustments()
            : base(hasReset: false) => this.AddParameter("mastervolume", "Master Volume", "Master Channel");
        protected override Boolean OnLoad()
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
            try
            {
                Globals.bankSettings[$"Input"].TryGetValue($"/1/mastervolume", out var value);
                this.valueFloat = Convert.ToSingle(value);
                Single tmp = (Single)turn / (Single)115;
                this.valueFloat += tmp;
                this.valueFloat = (Single)Math.Max(0.0, (Single)Math.Min(1.0, this.valueFloat));
                Globals.bankSettings["Input"]["/1/mastervolume"] = this.valueFloat.ToString();
                // make it so
                Sender.Send($"/1/mastervolume", this.valueFloat, Globals.interfaceIp, Globals.interfacePort);
                this.AdjustmentValueChanged(actionParameter); // Notify the Loupedeck service that the adjustment value has changed.
            } catch
            {
                //
            }
        }

        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter)
        {
            try
            {
                Globals.bankSettings["Input"].TryGetValue("/1/mastervolumeVal", out var outputValue);
                if (Globals.loggingEnabled) Tracer.Trace($"TotalMix: Dial _Bus: Input (MasterVolume) _ Address: /1/mastervolumeVal _ Value:  {outputValue}");
                return outputValue;
            }
            catch (Exception ex)
            {
                if (Globals.loggingEnabled) Tracer.Warning($"TotalMix: Dial _ Bus: Input (MasterVolume) _ Address: /1/mastervolumeVal _ Error: {ex.Message}");
                return "\t❌";
            }
        }

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
            // draw friendly name and channel name next to the dial
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.DrawText($"Master Volume", x: 10, y: 10, width: 30, height: 30, fontSize: 14, color: BitmapColor.White);

                    return bitmapBuilder.ToImage();
                }
        }
    }
}

