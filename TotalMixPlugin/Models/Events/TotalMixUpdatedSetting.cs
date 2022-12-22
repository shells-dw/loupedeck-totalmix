namespace Loupedeck.TotalMixPlugin.Models.Events
{
    using System;
    public class TotalMixUpdatedSetting : EventArgs
    {
        public String Address { get; }

        public TotalMixUpdatedSetting(String address) => this.Address = address;
    }
}
