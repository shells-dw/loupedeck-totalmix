// define Global variables

namespace Loupedeck.TotalMixPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public static class Globals
    {
        // holds all page 1 responses for all 3 busses, once filled
        public static Dictionary<String, Dictionary<String, String>> bankSettings = new Dictionary<String, Dictionary<String, String>>();
        public static Dictionary<String, Dictionary<String, String>> recentUpdates = new Dictionary<String, Dictionary<String, String>>();

        // last known device state
        public static Int32 deviceState;

        // holds the current bus (which might or might not be something that survives the first polish)
        public static String currentBus;

        // Globals for the device connection data, filled at start, read from then on
        public static String interfaceIp;
        public static Int32 interfacePort;
        public static Int32 interfaceSendPort;
        public static Int32 interfaceBackgroundPort;
        public static Int32 interfaceBackgroundSendPort;
        public static String mirroringRequested;
    }
}
