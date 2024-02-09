using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using ledbox.ViewModels;
using Acr.UserDialogs;
using ledbox.Resources;

namespace ledbox
{
    public partial class BluetoothConnectionView : ContentPage
    {

       
        BluetoothVIewModel bvm;

        public BluetoothItem bluetoothSelected;

        List<BluetoothItem> bluetoothItems;

        public BluetoothConnectionView(List<BluetoothItem> bis)
        {


            bluetoothItems = bis;
            InitializeComponent();
            bvm = new BluetoothVIewModel();
            BindingContext = bvm;

            


        }

        protected override void OnAppearing()
        {
            if(bluetoothItems.Count>0)
                bvm.reloadList(bluetoothItems);
        }


        void connect()
        {
            var progress = UserDialogs.Instance.Loading(AppResources.connecting_ledbox,()=> { }, AppResources.cancel);

            //new System.Threading.Thread(() =>
            //{
            progress.Show();
            //}).Start();


            new System.Threading.Thread(() =>
            {
                BluetoothConnection bc = (BluetoothConnection)App.conn;
                try
                {
                    bc._connect(bluetoothSelected, (isconnected) =>
                    {
                        progress.Hide();
                        if (!isconnected)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                App.DisplayAlert(AppResources.error_connection_to_ledbox_bluetooth);
                            });

                        }
                    });
                }
                catch
                {
                    progress.Hide();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.DisplayAlert(AppResources.error_connection_to_ledbox_bluetooth);
                    });
                    Console.WriteLine("ERROR CONNECT");

                }
            }).Start();

            Navigation.PopAsync(false);

        }


        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            bluetoothSelected = e.Item as BluetoothItem;
            connect();
        }

        

        async void Bt_Cancel_Clicked(object sender, System.EventArgs e)
        {
            
            await Navigation.PopAsync();
        }

        async void Bt_Scan_Clicked(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => { 
                 UserDialogs.Instance.Alert(new AlertConfig
                {
                    Message = AppResources.warning_scan_bluetooth,
                    OkText = AppResources.ok,
                    OnAction = (() =>
                    {
                        IBluetooth bluetooth = DependencyService.Get<IBluetooth>();
                        bluetooth.startDiscovery();
                    })

                }) ;
            });

        }

    }
}
