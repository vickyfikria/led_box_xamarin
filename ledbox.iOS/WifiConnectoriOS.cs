using System;
using ledbox.iOS;
using NetworkExtension;
using Xamarin.Forms;


[assembly: Dependency(typeof(WifiConnectoriOS))]
namespace ledbox.iOS
{
    public class WifiConnectoriOS: IWifiConnector
    {





        public void ConnectToWifi(string ssid, string password) {
            var wifiManager = new NEHotspotConfigurationManager();
            var wifiConfig = new NEHotspotConfiguration(ssid, password, false);

            wifiManager.ApplyConfiguration(wifiConfig, (error) =>
            {
                if (error != null)
                {
                    Console.WriteLine($"Error while connecting to Wi-Fi network {ssid}: {error}");
                }
            });

        }
    }
}
