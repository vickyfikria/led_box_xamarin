using System;
using ledbox.Droid;
using System.Net;
using Xamarin.Forms;
using Android.Net.Wifi;
using Android.App;
using Android.Text.Format;
using System.Collections.Generic;
using Android.Telephony;
using Xamarin.Essentials;
using Android.Net;
using Android.Content;
using Android.Locations;
using ledbox.Resources;

//[assembly: Dependency(typeof(ledbox.Droid.Android.DependencyServices.IPAddressManager))]

[assembly: Dependency(typeof(IPAddressManager))]

namespace ledbox.Droid
{
    class IPAddressManager : IIPAddressManager
    {
        private const int REQUEST_ENABLE_GPS = 2;

        [Obsolete]
        public string GetIPAddress()
        {
            Android.Content.Context context = Android.App.Application.Context;
            try
            {
                WifiManager wm = (WifiManager)context.GetSystemService(Android.Content.Context.WifiService);

                
                return Formatter.FormatIpAddress(wm.ConnectionInfo.IpAddress);
            }
            catch
            {
                return "";
            }
        }

        public bool CheckWifiConnection()
        {


           


            Android.Content.Context context = Android.App.Application.Context;
            try { 


          

                WifiManager wm = (WifiManager)context.GetSystemService(Android.Content.Context.WifiService);
                Version version = DeviceInfo.Version;
                if (version.Major >= 10)
                {
                    return wm.IsWifiEnabled;
                }
                else
                {
                    return wm.ConnectionInfo.NetworkId > -1 ? true : false;
                }

                
            }
            catch
            {
                return false;
            }
        }



       



        public string CurrentWifiConnection()
        {

            

            
            


            if (Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
            {
                var activity = (MainActivity)Forms.Context;
                activity.RequestPermissions(new string[] { Android.Manifest.Permission.AccessFineLocation, Android.Manifest.Permission.AccessCoarseLocation }, 0);

            }

            


            string ssid = "";
            WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            WifiInfo info = wifiManager.ConnectionInfo;
            int networkId = info.NetworkId;

              

            Version version = DeviceInfo.Version;
            if (version.Major >= 10)
            {
                return info.SSID;
            }



            IList<WifiConfiguration> netConfList = wifiManager.ConfiguredNetworks;

            if (netConfList != null)
            {
                foreach (WifiConfiguration wificonf in netConfList)
                {
                    if (wificonf.NetworkId == networkId)
                    {
                        ssid = wificonf.Ssid;
                        break;
                    }
                }               
            }
            return ssid;
        }



        public bool CheckDataMobile()
        {
            TelephonyManager tm = (TelephonyManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.TelephonyService);
            return tm.DataEnabled;

        }

        [Obsolete]
        public string CurrentWifiConnectionOld()
        {
            Android.Content.Context context = Android.App.Application.Context;
            try
            {
                WifiManager wm = (WifiManager)context.GetSystemService(Android.Content.Context.WifiService);


              


                return wm.ConnectionInfo.SSID;
            }
            catch
            {
                return "";
            }
        }

    }
}