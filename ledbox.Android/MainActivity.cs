using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Plugin.CurrentActivity;
using Android.Content;
using Android.Bluetooth;
using System.Linq;
using Acr.UserDialogs;
using Xamarin.Forms.Platform.Android;
using System.IO;
using System.Collections.Generic;
using Xamarin.Forms;
using Android.Net;
using Xamarin.Essentials;
using Android.Locations;
using ledbox.Resources;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.Telephony;

namespace ledbox.Droid
{



    [Activity(Label = "LEDbox", Icon = "@drawable/icon", Theme = "@style/MyTheme.Base", MainLauncher = false, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

       

        /// <summary>
        /// Dispositivi bluetooth trovati nell'ultima volta che è stato avviata la ricerca dispositivi Bluetooth
        /// </summary>
        public static List<BluetoothDevice> last_bluetooth_searched;

        protected override void OnCreate(Bundle savedInstanceState)
        {


            UserDialogs.Init(this);
            

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

           

            base.OnCreate(savedInstanceState);


            

            


            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            
            //Xamarians.CropImage.Droid.CropImageServiceAndroid.Initialize(this);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            //TextView versionLabel = FindViewById<TextView>(Resource.Id.versionLabel);

            //versionLabel.Text = "Versione Test";



            LoadApplication(new App());

            //verifica i permessi
            DependencyService.Get<IPermissions>().checkPermission(AndroidPermission.ACTION_INIT_APP);

            //verifica i permessi bluetooth
            //DependencyService.Get<IPermissions>().checkPermission(AndroidPermission.ACTION_BLUETOOTH);

            //verifica i permessi storage
            DependencyService.Get<IPermissions>().checkPermission(AndroidPermission.ACTION_STORAGE);

            //verifica i permessi storage
            //DependencyService.Get<IPermissions>().checkPermission(AndroidPermission.ACTION_STORAGE_ANDROID13);


            BluetoothDeviceReceiver BluetoothReceiver = new BluetoothDeviceReceiver();
            RegisterReceiver(BluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionStateChanged));
            RegisterReceiver(BluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));
            RegisterReceiver(BluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryStarted));
            RegisterReceiver(BluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(BluetoothReceiver, new IntentFilter(BluetoothDevice.ActionBondStateChanged));






            BluetoothLedbox.bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
           


        }




       

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (data != null)
            {
                if (requestCode == AndroidPermission.REQUEST_LOCATION_SERVICE) { 
                    if (resultCode == Result.Canceled)
                    {
                        DependencyService.Get<IPermissions>().checkPermission(AndroidPermission.ACTION_INIT_APP);

                    }
                }
            }
        }


        /*
        protected override void OnResume()
        {
            BluetoothDeviceReceiver bluetoothDeviceReceiver = new BluetoothDeviceReceiver();

            IntentFilter filter = new IntentFilter(BluetoothDevice.ActionFound);

            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);
            


            RegisterReceiver(bluetoothDeviceReceiver, filter);
            base.OnResume();
        }
        */
            /*

            public override bool OnOptionsItemSelected(IMenuItem item)
            {
                // check if the current item id 
                // is equals to the back button id
                if (item.ItemId == 16908332) // xam forms nav bar back button id
                {
                    // retrieve the current xamarin 
                    // forms page instance
                    var currentpage = (IBackButton)Xamarin.Forms.Application.Current.
                         MainPage.Navigation.NavigationStack.LastOrDefault();

                    // check if the page has subscribed to the custom back button event
                    if (currentpage?.BackButtonEvent != null)
                    {
                        // invoke the Custom back button action
                        currentpage?.BackButtonEvent.Invoke();
                        // and disable the default back button action
                        return false;
                    }

                    // if its not subscribed then go ahead 
                    // with the default back button action
                    return base.OnOptionsItemSelected(item);
                }
                else
                {
                    // since its not the back button 
                    //click, pass the event to the base
                    return base.OnOptionsItemSelected(item);
                }
            }

            /*public override void OnBackPressed()
            {
                // this is really not necessary, but in Android user has both Nav bar back button 
                // and physical back button, so its safe to cover the both events

                var currentpage = (IBackButton)Xamarin.Forms.Application.Current.
                    MainPage.Navigation.NavigationStack.LastOrDefault();

                if (currentpage?.BackButtonEvent != null)
                {
                    currentpage?.BackButtonEvent.Invoke();
                }
                else
                {
                    base.OnBackPressed();
                }
            }*/


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    public class BluetoothDeviceReceiver : BroadcastReceiver
    {

        bool startDiscovery = false;

        /// <summary>
        /// Lista dei device bluetooth trovati con la ricerca dispositivi
        /// </summary>
        List<BluetoothItem> bluetoothItems;


        /// <summary>
        /// Device bluetooth corrente che sta facendo il paring
        /// </summary>
        BluetoothDevice deviceBonding;

        public override void OnReceive(Context context, Intent intent)
        {
            var action = intent.Action;


            if(action== BluetoothAdapter.ActionStateChanged)
            {
                int state = intent.GetIntExtra(BluetoothAdapter.ExtraState, BluetoothAdapter.Error);
                if(App.conn!=null)
                    Xamarin.Forms.MessagingCenter.Send<ConnectionInterface,int>(App.conn, "BluetoothState",state);


            }

            if (action == BluetoothAdapter.ActionDiscoveryStarted)
            {
                startDiscovery = true;
                Xamarin.Forms.MessagingCenter.Send<ConnectionInterface>(App.conn, "BluetoothStartDiscovery");
                bluetoothItems = new List<BluetoothItem>();
            }

            if (action == BluetoothDevice.ActionFound)
            {
                

                var newDevice = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (newDevice != null) { 
                        BluetoothItem bluetoothItem = new BluetoothItem();
                        bluetoothItem.name = newDevice.Name;
                        bluetoothItem.address = newDevice.Address;
                        bluetoothItem.UUID = "";
                        bluetoothItem.paired = false;
                        bluetoothItems.Add(bluetoothItem);
                      
                    if (MainActivity.last_bluetooth_searched == null)
                            MainActivity.last_bluetooth_searched = new List<BluetoothDevice>();
                        MainActivity.last_bluetooth_searched.Add(newDevice);
                }

                return;
            }

            if(action== BluetoothAdapter.ActionDiscoveryFinished)
            {
                if (!startDiscovery)
                    return;
                try
                {
                    Xamarin.Forms.MessagingCenter.Send<ConnectionInterface, List<BluetoothItem>>(App.conn, "BluetoothDeviceFound", bluetoothItems);
                }
                catch (Exception e)
                {
                    DependencyService.Get<IMessage>().LongAlert("Bluetooth is not available");
                }
                    startDiscovery = false;
            }

            if (action == BluetoothDevice.ActionBondStateChanged)
            {
                if (deviceBonding == null)
                    deviceBonding = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                if (deviceBonding.BondState == Bond.Bonded)
                {
                    
                    BluetoothItem bluetoothItem = new BluetoothItem();
                    bluetoothItem.name = deviceBonding.Name;
                    bluetoothItem.address = deviceBonding.Address;
                    bluetoothItem.UUID = "";
                    bluetoothItem.paired = true;

                    BluetoothConnection bc = (BluetoothConnection)App.conn;
                   
                    bc._connect(bluetoothItem, (isconnected) => {

                        

                    });
                    deviceBonding = null;
                }

                
            }


            /*
            // Get the device
            var device = (BluetoothDevice)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

            if (device.BondState != Bond.Bonded)
            {
                Console.WriteLine($"Found device with name: {device.Name} and MAC address: {device.Address}");
            }
            */
        }
    }


}