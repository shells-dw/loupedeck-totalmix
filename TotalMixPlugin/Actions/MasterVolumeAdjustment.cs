// dial action for master Volume

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Runtime.Remoting.Channels;
    using System.Threading.Tasks;
    using Loupedeck.TotalMixPlugin.Actions;

    public class MasterVolumeAdjustments : PluginDynamicAdjustment
    {
        // assign variables
        private TotalMixPlugin _plugin;
        private Int32 _toggle = 0;
        private Single valueFloat;

        // build the action
        public MasterVolumeAdjustments()
            : base(hasReset: false)
        {
            this.AddParameter("mastervolume|", "Master Volume", "Master Channel");
        }
        protected override bool OnLoad()
        {
            _plugin = base.Plugin as TotalMixPlugin;
            if (_plugin is null)
                return false;

            _plugin.UpdatedInputSetting += (sender, e) => this.ActionImageChanged(e.Address);
            _plugin.UpdatedInputSetting += (sender, e) => this.AdjustmentValueChanged(e.Address);
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
                Task.Run(() => HelperFunctions.SendOscCommand($"/1/mastervolume", this.valueFloat));
                this.AdjustmentValueChanged(actionParameter); // Notify the Loupedeck service that the adjustment value has changed.
            } catch
            {
                //
            }
        }

        // dial is pressed - flipping between -infinity and 0dB
        protected override void RunCommand(String actionParameter)
        {
            if (this._toggle == 0)
            {

                Task.Run(() => HelperFunctions.SendOscCommand($"/1/mastervolume", 0));
                this._toggle++;
            }
            else
            {
                Task.Run(() => HelperFunctions.SendOscCommand($"/1/mastervolume", (Single)0.8172043));
                this._toggle = 0;
            }
            this.AdjustmentValueChanged(actionParameter); // Notify the Loupedeck service that the adjustment value has changed.
        }



        // Returns the adjustment value that is shown next to the dial.
        protected override String GetAdjustmentValue(String actionParameter)
        {
            try
            {
                Globals.bankSettings["Input"].TryGetValue("/1/mastervolumeVal", out var outputValue);
                return outputValue;
            }
            catch
            {
                return "\t❌";

            }
        }

        protected override BitmapImage GetAdjustmentImage(string actionParameter, PluginImageSize imageSize)
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
            // draw friendly name and channel name next to the dial
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
                {
                    bitmapBuilder.DrawText($"Master Volume", x: 10, y: 10, width: 30, height: 30, fontSize: 14, color: BitmapColor.White);

                    return bitmapBuilder.ToImage();
                }
        }
    }
}

