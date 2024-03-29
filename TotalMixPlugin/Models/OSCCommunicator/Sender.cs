﻿// OSC Sender

namespace Loupedeck.TotalMixPlugin
{
    using Rug.Osc;

    using System;
    using System.Net;
    using System.Threading.Tasks;
    internal class Sender
    {
        public static Task Send(String name, Single value, String ip, Int32 port)
        {
            // Binding address. Technically we can leave this as the any address, but it doesn't really matter.
            IPAddress LocalIP = IPAddress.Loopback;
            // Attempt to parse the input address. If we fail to parse, fall back to the loopback.
            if (ip != null && IPAddress.TryParse(ip, out var RemoteIP))
            {
                // If it's not the loopback/localhost address, then set the LocalIP to the any address
                if (!IPAddress.IsLoopback(RemoteIP))
                {
                    LocalIP = IPAddress.Any;
                }
            }
            else
            {
                // If tryparse ever fails, RemoteIP will be null, so remember to set a valid address here.
                RemoteIP = IPAddress.Loopback;
            }

            OscSender sender = null;
            try
            { sender = new OscSender(local: LocalIP, localPort: 0, remote: RemoteIP, remotePort: port); }
            catch (Exception ex)
            {
                sender.Dispose();
                sender = null;
            }
            finally { sender = new OscSender(local: LocalIP, localPort: 0, remote: RemoteIP, remotePort: port); }

            try
            {
                // connect to the socket 
                sender.Connect();
            }
            catch (Exception ex)
            {
                sender.Dispose();
                sender = null;
                Task.FromException(ex);
            }

            // Send a new message
            sender.Send(new OscMessage(name, value));

            sender.Close();
            return Task.CompletedTask;
        }
    }
}