namespace Loupedeck.TotalMixPlugin.Models.Events
{
    using System;
    public class TotalMixUpdatedSetting : EventArgs
    {
        public string Address { get; }

        public TotalMixUpdatedSetting(string address)
        {
            this.Address = address;
        }
    }
}
