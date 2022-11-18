// OSC Listener

namespace Loupedeck.TotalMixPlugin
{
    using Rug.Osc;
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    internal class Listener
    {
        // assign variables
        static OscReceiver receiver;
        static Thread thread;



        // main listener Task
        static async public Task Listen(String bus, String address, Single value)
        {
            Globals.currentBus = bus;

            // Create the receiver
            receiver = new OscReceiver(Globals.interfaceBackgroundSendPort);

            // Create a thread to do the listening
            thread = new Thread(new ParameterizedThreadStart(ListenLoop));

            // Connect the receiver
            receiver.Connect();

            // Start the listen thread
            thread.Start(bus);
            Task.Run(() => Sender.Send(address, value, Globals.interfaceIp, Globals.interfaceBackgroundPort)).Wait();

            Thread.CurrentThread.Join(1000);
            // close the Reciver 
            receiver.Close();
            return;
        }

        static void ListenLoop(object bus)
        {
            try
            {
                while (receiver.State != OscSocketState.Closed)
                {
                    // making sure to add the key if it doesn't exist already (we need it anyway and don't like exceptions)
                    if (!Globals.bankSettings.ContainsKey($"{bus}"))
                    {
                        Globals.bankSettings.Add($"{bus}", new Dictionary<String, String>());
                    }
                    if (!Globals.recentUpdates.ContainsKey($"{bus}"))
                    {
                        Globals.recentUpdates.Add($"{bus}", new Dictionary<String, String>());
                    }
                    // if we are in a state to recieve
                    if (receiver.State == OscSocketState.Connected)
                    {
                        // get the next message 
                        // this will block until one arrives or the socket is closed
                        OscPacket packet = receiver.Receive();
                        
                        // we're expecting only bundles at this time, so define the received packet as bundle
                        OscBundle bundle = packet as OscBundle;

                        // abort condition RegEx - the device sends a heartbeat of sorts ("/") every second
                        String snapRegEx = @"^\/$";
                        var r = new Regex(snapRegEx, RegexOptions.IgnoreCase);
                        if (bundle != null)
                        {
                            // match for abort condition
                            Match m = r.Match(((Rug.Osc.OscMessage)bundle[0]).Address.ToString());

                            // add every received bundle to the Global Dict
                            for (var i = 0; i < bundle.Count; i++)
                            {
                                if (Globals.bankSettings[$"{bus}"].ContainsKey(((Rug.Osc.OscMessage)bundle[i]).Address))
                                {
                                    Match theTriggerMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), "^\\/1\\/(mute|solo|phantom|pan|micgain)(\\/1\\/)?(\\d)$");
                                    if (theTriggerMatch.Success == true)
                                    {
                                        Globals.bankSettings[$"{bus}"].TryGetValue($"/1/{theTriggerMatch.Groups[1].Value}{theTriggerMatch.Groups[2].Value}{theTriggerMatch.Groups[3].Value}", out var value);
                                        if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                        {
                                            Globals.recentUpdates[$"{bus}"][theTriggerMatch.Groups[1].Value] = theTriggerMatch.Groups[3].Value;
                                        }
                                    }
                                    Match theAdjustmentMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), "^\\/1\\/(volume|mastervolume)(\\d)?Val$");
                                    if (theAdjustmentMatch.Success == true)
                                    {
                                            Globals.bankSettings[$"{bus}"].TryGetValue($"/1/{theAdjustmentMatch.Groups[1].Value}{theAdjustmentMatch.Groups[2].Value}Val", out var value);
                                            if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                            {
                                                Globals.recentUpdates[$"{bus}"][theAdjustmentMatch.Groups[1].Value] = theAdjustmentMatch.Groups[2].Value;
                                            }
                                    }
                                    Globals.bankSettings[$"{bus}"].Remove(((Rug.Osc.OscMessage)bundle[i]).Address);
                                }
                                Globals.bankSettings[$"{bus}"].Add(((Rug.Osc.OscMessage)bundle[i]).Address, ((Rug.Osc.OscMessage)bundle[i])[0].ToString());
                            }

                            // matched
                            if (m.Success)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // TODO
            catch
            {
                // if the socket was connected when this happens
                // then do something useful with it
                if (receiver.State == OscSocketState.Connected)
                {
                    // something useful
                }
            }
        }
    }
}
