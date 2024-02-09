using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Gms.Tasks;
using Android.Locations;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using ledbox.Droid;
using ledbox.Resources;
using Plugin.Permissions;
using Xamarin.Essentials;
using Xamarin.Forms;


[assembly: Dependency(typeof(AndroidPermission))]


namespace ledbox.Droid
{
    class AndroidPermission : IPermissions
    {

        public const int REQUEST_LOCATION_SERVICE = 200;

        public const int ACTION_INIT_APP = 1;
        public const int ACTION_CONNECT = 2;
        public const int ACTION_CONNECT_WIFI_DIRECT = 3;
        public const int ACTION_CHECK_INTERNET_WIFI_DIRECT = 4;
        public const int ACTION_CHECK_INTERNET = 5;
        public const int ACTION_STORAGE = 6;
        public const int ACTION_BLUETOOTH = 7;
        public const int ACTION_STORAGE_ANDROID13 = 8;



        public static Context context = Android.App.Application.Context;

        public bool checkPermission(int action)
        {
            Version version = DeviceInfo.Version;

            switch (action)
            {
                case ACTION_INIT_APP:
                case ACTION_CONNECT:
                    if (version.Major >= 10)
                    {
                        //verifica se sono abilitati i servizi di localizzazione (necessari per la lettura SSID WIfi e per il discovery Bluetooth)
                        LocationManager locationManager = (LocationManager)Forms.Context.GetSystemService(Context.LocationService);
                        if (locationManager.IsProviderEnabled(LocationManager.GpsProvider) == false)
                        {
                            turnOnGps();
                           
                            return false;


                        }

                    }



                    break;
                case ACTION_CONNECT_WIFI_DIRECT:
                    if (version.Major == 9 && version.Minor==0) //per Android 9 bisogna controllare che i dati mobile siano disattivi per instradare sulla Wlan
                    {
                        //verifica se i dati mobile sono attivi
                        bool mobile = DependencyService.Get<IIPAddressManager>().CheckDataMobile();

                        // DependencyService.Get<IWifi>().ForceWifiOverCellular(); //TODO da provare con android 9


                        if (mobile)
                        {
                            App.DisplayAlert(AppResources.disable_mobile_connection);

                            // Android.App.Application.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionWifiSettings).SetFlags(ActivityFlags.NewTask));
                            //TODO provare ad aprire la finestra di disconessione dati mobili



                            return false;
                        }
                    }
                    break;

                case ACTION_CHECK_INTERNET_WIFI_DIRECT:
                    //verifica se i dati mobili sono attivi
                  
                    if (version.Major == 9)
                    {
                        return MobileDataState();
                    }

                    if (version.Major == 10)
                    {
                        return false;
                    }

                    return false;


                    break;
                case ACTION_CHECK_INTERNET:
                    return isOnline();


                    break;
                case ACTION_STORAGE:

                   
                    if (
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.BluetoothConnect) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.BluetoothScan) != Android.Content.PM.Permission.Granted ||

                        //Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ManageExternalStorage) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadExternalStorage) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaImages) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaVideo) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaAudio) != Android.Content.PM.Permission.Granted
                        )
                    {
                        var activity = (MainActivity)Forms.Context;
                        activity.RequestPermissions(
                            new string[] {
                                //Android.Manifest.Permission.ManageExternalStorage,
                                Android.Manifest.Permission.ReadExternalStorage,
                                Android.Manifest.Permission.WriteExternalStorage,
                                Android.Manifest.Permission.ReadMediaImages,
                                Android.Manifest.Permission.ReadMediaVideo,
                                Android.Manifest.Permission.ReadMediaAudio,

                                Android.Manifest.Permission.BluetoothConnect,
                                Android.Manifest.Permission.BluetoothScan

                            }, 0);



                        return false;

                    }
                    return true;

                case ACTION_STORAGE_ANDROID13:


                    if (Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaImages) != Android.Content.PM.Permission.Granted || Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaVideo) != Android.Content.PM.Permission.Granted || Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.ReadMediaAudio) != Android.Content.PM.Permission.Granted)
                    {
                        var activity = (MainActivity)Forms.Context;
                        activity.RequestPermissions(new string[] { Android.Manifest.Permission.ReadMediaImages, Android.Manifest.Permission.ReadMediaVideo, Android.Manifest.Permission.ReadMediaAudio }, 0);

                        //activity.RequestPermissions(new string[] { Android.Manifest.Permission.ReadMediaImages },0);//, Android.Manifest.Permission.ReadMediaVideo, Android.Manifest.Permission.ReadMediaAudio }, 0);

                        return false;

                    }
                    return true;


                   
                case ACTION_BLUETOOTH:


                    if (
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.BluetoothConnect) != Android.Content.PM.Permission.Granted ||
                        Android.App.Application.Context.CheckSelfPermission(Android.Manifest.Permission.BluetoothScan) != Android.Content.PM.Permission.Granted

                        )
                    {
                        var activity = (MainActivity)Forms.Context;
                        activity.RequestPermissions(new string[] { Android.Manifest.Permission.BluetoothConnect, Android.Manifest.Permission.BluetoothScan }, 0);
                        return false;

                    }
                    return true;
                    
            }

            


            return true;
        }

        public bool isOnline()
        {
            Runtime runtime = Runtime.GetRuntime();
            try
            {
                Java.Lang.Process ipProcess = runtime.Exec("/system/bin/ping -W 1 -c 1 8.8.8.8");
                int exitValue = ipProcess.WaitFor();
                return (exitValue == 0);
            }
            catch (Java.Lang.Exception e) { e.PrintStackTrace(); }
           

            return false;
        }


        
      


        public bool MobileDataState()
        {
            try
            {
                TelephonyManager telephonyService = (TelephonyManager)context.GetSystemService(Context.TelephonyService);
                return telephonyService.DataEnabled;
            }
            catch (Java.Lang.Exception ex)
            { }

            return false;

        }


        public async void turnOnGps()
        {
            try
            {
                MainActivity activity = Forms.Context as MainActivity;

                GoogleApiClient googleApiClient = new GoogleApiClient.Builder(activity).AddApi(LocationServices.API).Build();
                googleApiClient.Connect();
                Android.Gms.Location.LocationRequest locationRequest = Android.Gms.Location.LocationRequest.Create();
                locationRequest.SetPriority(Android.Gms.Location.LocationRequest.PriorityHighAccuracy);
                locationRequest.SetInterval(10000);
                locationRequest.SetFastestInterval(10000 / 2);


                LocationSettingsRequest.Builder locationSettingsRequestBuilder = new LocationSettingsRequest.Builder().AddLocationRequest(locationRequest);
                locationSettingsRequestBuilder.SetAlwaysShow(true);
                locationSettingsRequestBuilder.SetNeedBle(true);
                LocationSettingsResult locationSettingsResult = await LocationServices.SettingsApi.CheckLocationSettingsAsync(googleApiClient, locationSettingsRequestBuilder.Build());

                if (locationSettingsResult.Status.StatusCode == LocationSettingsStatusCodes.ResolutionRequired)
                {
                    locationSettingsResult.Status.StartResolutionForResult(activity, REQUEST_LOCATION_SERVICE);
                }
            }
            catch (Java.Lang.Exception ex)
            {
                //GlobalVariables.SendExceptionReport(ex);
            }
        }
    }
}