namespace Loupedeck.TotalMixPlugin
{
    using System;
    using Loupedeck.TotalMixPlugin.Actions;


    public class LoadSnapshots : PluginDynamicCommand
    {
        // Assign variables

        // build action
        public LoadSnapshots() : base(displayName: "Snapshots", description: "Load Snapshots", groupName: "Snapshots")
        {
            this.MakeProfileAction("list;Select snapshot to load with this button: ");

            // TotalMix supports only 8 snapshots (afaik), so create 8 options for the list
            for (var i = 1; i < 9; i++)
            {
                // weirdly Snapshots are in reverse order in Totalmix, so making that happen...
                this.AddParameter($"snapshots/{9-i}/1", $"Snapshot {i}", groupName: "Snapshots");
            }
        }
        // Button is pressed
        protected override void RunCommand(String actionParameter)
        {
            // send command to load selected snapshot
            HelperFunctions.SendOscCommand($"/3/{actionParameter}", 1);
        }

        // update command image (nothing to update here per se, but that's called to draw whatever is shown on the Loupedeck)
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
                    bitmapBuilder.DrawText("Error", x: 10, y: 10, width: 30, height: 30, fontSize: 14, color: BitmapColor.Black);
                    return bitmapBuilder.ToImage();
                }
            }
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                var test = actionParameter.Substring(10, 1);
                bitmapBuilder.DrawText($"Snapshot {9 - Int32.Parse(actionParameter.Substring(10, 1))}", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);

                return bitmapBuilder.ToImage();
            }
        }

    }
}
