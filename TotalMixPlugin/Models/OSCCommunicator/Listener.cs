// OSC Listener

namespace Loupedeck.TotalMixPlugin
{
    using Rug.Osc;

    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    internal class Listener
    {
        // assign variables
        static OscReceiver _receiver;

        // main listener Task
        public Task Listen(String bus, String address, Single value)
        {
            Globals.currentBus = bus;

            // Create the _receiver
            _receiver = new OscReceiver(Globals.interfaceBackgroundSendPort);

            // Connect the _receiver
            _receiver.Connect();

            // Start the listen thread
            Sender.Send(address, value, Globals.interfaceIp, Globals.interfaceBackgroundPort).Wait();

            // close the Reciver 
            Task.Run(() => this.ListenLoop(bus)).Wait(5000);
            _receiver.Close();
            if (_receiver != null)
            {
                // dispose of the reciever
                _receiver.Dispose();
                _receiver = null;
            }
            return Task.CompletedTask;
        }

        private Task ListenLoop(Object bus)
        {
            try
            {
                while (_receiver != null && _receiver.State == OscSocketState.Connected)
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
                    if (_receiver != null && _receiver.State == OscSocketState.Connected)
                    {
                        // get the next message 
                        // this will block until one arrives or the socket is closed
                        while (_receiver.TryReceive(out OscPacket packet) == true)
                        {
                            // we're expecting only bundles at this time, so define the received packet as bundle
                            OscBundle bundle = packet as OscBundle;

                            if (bundle != null)
                            {
                                Match TotalMixHeartbeat = Regex.Match(((Rug.Osc.OscMessage)bundle[0]).Address.ToString(), @"^\/$|^\/3\/recordRecordStart");
                                if (TotalMixHeartbeat.Success == true)
                                {
                                    return Task.CompletedTask;
                                }
                                // add every received bundle to the Global Dict
                                for (var i = 0; i < bundle.Count; i++)
                                {
                                    Match uninterestingValues = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), "^.{3}(?>label|select)");
                                    if (uninterestingValues.Success == false)
                                    {
                                        if (Globals.bankSettings[$"{bus}"].ContainsKey(((Rug.Osc.OscMessage)bundle[i]).Address))
                                        {
                                            Match theTriggerMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), @"^\/1\/(mute|solo|phantom|pan|micgain)(\/1\/)?(\d{1,})$");
                                            if (theTriggerMatch.Success == true)
                                            {
                                                Globals.bankSettings[$"{bus}"].TryGetValue($"/1/{theTriggerMatch.Groups[1].Value}{theTriggerMatch.Groups[2].Value}{theTriggerMatch.Groups[3].Value}", out var value);
                                                if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                                {
                                                    Globals.recentUpdates[$"{bus}"][theTriggerMatch.Groups[1].Value] = theTriggerMatch.Groups[3].Value;
                                                }
                                                var abort = $"{theTriggerMatch.Groups[1].Value}{theTriggerMatch.Groups[3].Value}Val";
                                                if (abort == $"micgain{Globals.channelCount}Val")
                                                {
                                                    return Task.CompletedTask;
                                                }
                                            }
                                            Match theAdjustmentMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), @"^\/1\/(volume|mastervolume)(\d{1,})?Val$");
                                            if (theAdjustmentMatch.Success == true)
                                            {
                                                Globals.bankSettings[$"{bus}"].TryGetValue($"/1/{theAdjustmentMatch.Groups[1].Value}{theAdjustmentMatch.Groups[2].Value}Val", out var value);
                                                if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                                {
                                                    Globals.recentUpdates[$"{bus}"][theAdjustmentMatch.Groups[1].Value] = theAdjustmentMatch.Groups[2].Value;
                                                }
                                            }
                                            Match theMainMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), "^\\/1\\/(main.*)$");
                                            if (theMainMatch.Success == true)
                                            {
                                                Globals.bankSettings[$"{bus}"].TryGetValue($"/1/{theMainMatch.Groups[1].Value}", out var value);
                                                if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                                {
                                                    Globals.recentUpdates[$"{bus}"][theMainMatch.Groups[1].Value] = "";
                                                }
                                            }
                                            Match theGlobalMatch = Regex.Match(((Rug.Osc.OscMessage)bundle[i]).Address.ToString(), @"^\/3\/(global(?:Mute|Solo)|reverbEnable|echoEnable)$");
                                            if (theGlobalMatch.Success == true)
                                            {
                                                Globals.bankSettings[$"{bus}"].TryGetValue($"/3/{theGlobalMatch.Groups[1].Value}", out var value);
                                                if (value != ((Rug.Osc.OscMessage)bundle[i])[0].ToString())
                                                {
                                                    Globals.recentUpdates[$"{bus}"][theGlobalMatch.Groups[1].Value] = "";
                                                }
                                            }
                                            Globals.bankSettings[$"{bus}"].Remove(((Rug.Osc.OscMessage)bundle[i]).Address);
                                        }
                                        Globals.bankSettings[$"{bus}"].Add(((Rug.Osc.OscMessage)bundle[i]).Address, ((Rug.Osc.OscMessage)bundle[i])[0].ToString());
                                    }
                                }
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
                if (_receiver != null && _receiver.State == OscSocketState.Connected)
                {
                    // something useful

                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }
}
