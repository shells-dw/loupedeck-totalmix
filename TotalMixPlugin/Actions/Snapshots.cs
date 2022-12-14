namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Loupedeck.TotalMixPlugin.Actions;


    public class LoadSnapshots : PluginDynamicCommand
    {
        // Assign variables

        List<String> current = null;
        // build action
        public LoadSnapshots() : base(displayName: "Snapshots", description: "Load Snapshots", groupName: "Snapshots")
        {
            this.MakeProfileAction("list;Select snapshot to load with this button: ");

            GetCustomSnapshotNames();
            // TotalMix supports only 8 snapshots (afaik), so create 8 options for the list
            for (var i = 1; i < 9; i++)
            {
                // weirdly Snapshots are in reverse order in Totalmix, so making that happen...
            //    this.AddParameter($"snapshots/{9 - i}/1", $"Snapshot {i}", groupName: "Snapshots");
                this.AddParameter($"snapshots/{9 - i}/1", $" Snapshot {i}: {current[i-1]}", groupName: "Snapshots");
            }
        }
        // Button is pressed
        protected override void RunCommand(String actionParameter) =>
            // send command to load selected snapshot
            Sender.Send($"/3/{actionParameter}", 1, Globals.interfaceIp, Globals.interfacePort);

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

                if (GetCustomSnapshotNames())
                {
                    bitmapBuilder.DrawText(current[8 - Int32.Parse(actionParameter.Substring(10, 1))], x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                }
                else
                {
                    bitmapBuilder.DrawText($"Snapshot {9 - Int32.Parse(actionParameter.Substring(10, 1))}", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                }

                return bitmapBuilder.ToImage();
            }
        }

        private Boolean GetCustomSnapshotNames()
        {
            String totalMixDir;
            if (Helpers.IsMacintosh())
            {
                totalMixDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Library/Application Support/RME TotalMix FX";
            }
            else
            {
                totalMixDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\TotalMixFx";
            }
            if (Directory.Exists(totalMixDir))
            {
                FileInfo[] totalMixConfig = Directory.GetFiles(totalMixDir, "last.*.xml").Select(x => new FileInfo(x)).ToArray();
                var curentConfig = totalMixConfig.OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

                this.current = new List<String>();
                foreach (var line in File.ReadAllLines(curentConfig.ToString()))
                {
                    if (line.Contains("SnapshotName"))
                    {
                        Match snapshotNames = Regex.Match(line, "v\\=\\\"(.*\\b)");
                        if (snapshotNames.Success == true)
                        {
                            this.current.Add(snapshotNames.Groups[1].Value);
                        }
                    }
                    if (line.Contains("<Inputs>"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
