// handling for Main Channel

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class MainsTrigger : PluginDynamicCommand
    {
        // assign variables
        private TotalMixPlugin _plugin;
        private Boolean currentState;
        private Boolean setToState;
        private Int32 setToStateInt;
        [DllImport("user32.dll")]
        public static extern Boolean ShowWindowAsync(IntPtr hWnd, Int32 nCmdShow);
        private const Int32 SW_HIDE = 0;
        private const Int32 SW_RESTORE = 5;
        private IntPtr hWnd;
        private IntPtr hWndCache;
        private Int32 hWndId;
        delegate Boolean EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern Boolean EnumThreadWindows(Int32 dwThreadId, EnumThreadDelegate lpfn,
            IntPtr lParam);

        static IEnumerable<IntPtr> EnumerateProcessWindowHandles(Int32 processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                EnumThreadWindows(thread.Id,
                    (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }

            return handles;
        }

        // build the action
        public MainsTrigger() : base()
        {
            this.AddParameter("mainDim", "Dim", groupName: "Master Channel");
            this.AddParameter("mainSpeakerB", "Speaker B", groupName: "Master Channel");
            this.AddParameter("mainRecall", "Recall", groupName: "Master Channel");
            this.AddParameter("mainMuteFx", "Mute FX", groupName: "Master Channel");
            this.AddParameter("mainMono", "Mono", groupName: "Master Channel");
            this.AddParameter("mainExtIn", "External In", groupName: "Master Channel");
            this.AddParameter("mainTalkback", "Talkback", groupName: "Master Channel");
            this.AddParameter("trim", "Trim", groupName: "Master Channel");
            if (Helpers.IsWindows())
            { 
                this.AddParameter("showhideui", "Show / Hide TotalMixFX Window", groupName: "Master Channel");
            }

        }
        protected override Boolean OnLoad()
        {
            this._plugin = base.Plugin as TotalMixPlugin;
            if (this._plugin is null)
            {
                return false;
            }

            this._plugin.UpdatedInputSetting += (sender, e) => this.ActionImageChanged(e.Address);
            return base.OnLoad();
        }
        protected override Boolean ProcessTouchEvent(String actionParameter, DeviceTouchEvent touchEvent)
        {
            while (!touchEvent.IsTouchUp() && actionParameter == "mainTalkback")
            {
                Sender.Send($"/1/{actionParameter}", 1, Globals.interfaceIp, Globals.interfacePort);
            }
            return base.ProcessTouchEvent(actionParameter, touchEvent);
        }
        // button is pressed
        protected override void RunCommand(String actionParameter)
        {
            if (actionParameter == "showhideui")
            {
                Process[] p = Process.GetProcessesByName("TotalMixFX");
                this.hWnd = p[0].MainWindowHandle;
                IntPtr WindowHandle = EnumerateProcessWindowHandles(p[0].Id).First();
                if (this.hWndCache == IntPtr.Zero)
                {
                    this.hWndCache = WindowHandle;
                }
                this.hWndId = (Int32)p[0].Id;
                if (this.hWnd == (IntPtr)0)
                {
                    ShowWindowAsync(this.hWndCache, SW_RESTORE);
                }
                else
                {
                    ShowWindowAsync(this.hWnd, SW_HIDE);
                }
            }
            else
            {
                // try getting the current state - if there is none or the Global variable doesn't exist, we'll just set it to true *shrug*
                try
                {
                    Globals.bankSettings["Input"].TryGetValue($"/1/{actionParameter}", out var value);
                    this.currentState = Convert.ToBoolean(Int32.Parse(value));
                    this.setToState = !this.currentState;
                }
                catch
                {
                    this.setToState = true;
                }
                this.setToStateInt = Convert.ToInt32(this.setToState);

                // send to TotalMix
                Sender.Send($"/1/{actionParameter}", 1, Globals.interfaceIp, Globals.interfacePort);

                // update Global variable
                Globals.bankSettings[$"Input"][$"/1/{actionParameter}"] = this.setToStateInt.ToString();

                // notify Loupedeck about the change
                this.ActionImageChanged(actionParameter);
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
                    bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerRed80.png")));
                    bitmapBuilder.DrawText("No Connection", x: 5, y: 50, width: 70, height: 40, fontSize: 15, color: BitmapColor.Black);
                    return bitmapBuilder.ToImage();
                }
            }
            // getting the currentState from Global variable
            try
            {
                Globals.bankSettings["Input"].TryGetValue($"/1/{actionParameter}", out var value);
                this.currentState = value != null && Convert.ToBoolean(Int32.Parse(value));
            } catch
            {
            
            }
            // build the image
            using (var bitmapBuilder = new BitmapBuilder(imageSize))
            {
                bitmapBuilder.DrawRectangle(0, 0, 80, 80, BitmapColor.Black);
                bitmapBuilder.FillRectangle(0, 0, 80, 80, BitmapColor.Black);
                switch (actionParameter)
                {
                    case "mainDim":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("dimOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("dimOff80.png")));
                        bitmapBuilder.DrawText("Main Dim", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainSpeakerB":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("speakerBOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("speakerBOff80.png")));
                        bitmapBuilder.DrawText("Speaker B", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainRecall":
                        bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("recall80.png")));
                        bitmapBuilder.DrawText("Main Recall", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainMuteFx":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteFXOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("muteFXOff80.png")));
                        bitmapBuilder.DrawText("Main Mute FX", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainMono":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("monoOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("monoOff80.png")));
                        bitmapBuilder.DrawText("Main Mono", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainExtIn":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("extInOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("extInOff80.png")));
                        bitmapBuilder.DrawText("Ext. Input", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "trim":
                        bitmapBuilder.SetBackgroundImage(this.currentState ? EmbeddedResources.ReadImage(EmbeddedResources.FindFile("trimOn80.png")) : EmbeddedResources.ReadImage(EmbeddedResources.FindFile("trimOff80.png")));
                        bitmapBuilder.DrawText("Trim", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "mainTalkback":
                        bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("talkbackOff80.png")));
                        bitmapBuilder.DrawText("Main Talkback", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
                        break;
                    case "showhideui":
                        bitmapBuilder.SetBackgroundImage(EmbeddedResources.ReadImage(EmbeddedResources.FindFile("mixerNeutral80.png")));
                        bitmapBuilder.DrawText("Show/Hide UI", x: 5, y: 40, width: 70, height: 40, fontSize: 15, color: BitmapColor.White);
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