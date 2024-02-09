using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ledbox.iOS;
using Xamarin.Forms;

//[assembly: Dependency(typeof(ledbox.iOSUnified.iOS.DependencyServices.IPAddressManager))]
[assembly: Dependency(typeof(IPAddressManager))]

namespace ledbox.iOS
{
    class IPAddressManager : IIPAddressManager
    {
        public bool CheckDataMobile()
        {
            return false;
        }

        public bool CheckWifiConnection()
        {
            return true;
        }

        public string CurrentWifiConnection()
        {
            return "";
        }

        public string GetIPAddress()
        {
            String ipAddress = "";

            foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                    netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = addrInfo.Address.ToString();

                        }
                    }
                }
            }

            return ipAddress;
        }
    }
}