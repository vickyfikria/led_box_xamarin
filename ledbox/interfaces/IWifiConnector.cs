using System;
namespace ledbox
{

    public interface IWifiConnector
    {
        void ConnectToWifi(string ssid, string password);

    }
}
