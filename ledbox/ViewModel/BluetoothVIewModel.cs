using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Acr.UserDialogs;
using ledbox.Resources;
using MvvmHelpers;
using Xamarin.Forms;

namespace ledbox.ViewModels
{
    public class BluetoothVIewModel: BaseViewModel,INotifyPropertyChanged
    {
        
        public ObservableCollection<BluetoothItem> Items { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        public bool message_no_device { get
            {
                if (Items != null)
                {
                    return Items.Count > 0 ? false : true;
                }
                else
                    return true;
            }
        }

        IProgressDialog loading;

        public BluetoothVIewModel()
        {
        

            DependencyService.Get<IBluetooth>().InitializeBluetooth();

            MessagingCenter.Subscribe<ConnectionInterface, int>(App.conn, "BluetoothState", ((s, state) =>
            {
                if (state == 12)
                { //se il bluetooth è on
                    reloadList(DependencyService.Get<IBluetooth>().getDevices());

                }
            }));


            MessagingCenter.Subscribe<ConnectionInterface, List<BluetoothItem>>(App.conn, "BluetoothDeviceFound", ((s, devices) =>
            {
                
                loading.Hide();
                reloadList(devices);

            }));

            MessagingCenter.Subscribe<ConnectionInterface>(App.conn, "BluetoothStartDiscovery", ((s) =>
            {
                //DependencyService.Get<IMessage>().ShortAlert(AppResources.bluetooth_alert);

                loading = UserDialogs.Instance.Loading(AppResources.searching,()=> { }, AppResources.cancel);
            }));



        }


        public void  reloadList(List<BluetoothItem> bluetoothItems)
        {
            if(Items==null)
                Items = new ObservableCollection<BluetoothItem>();
            foreach (BluetoothItem bluetoothItem in bluetoothItems)
            {

                if (bluetoothItem.name != null)
                {
                    if (bluetoothItem.name.Contains("Litescore") || bluetoothItem.name.Contains("ledbox"))
                    {
                        //verifica che il device non è presente già nell'elenco
                        if (!verifyDevice(bluetoothItem))
                            Items.Add(bluetoothItem);

                    }
                }
                
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Items"));
                PropertyChanged(this, new PropertyChangedEventArgs("message_no_device"));
            }

        }


        /// <summary>
        /// Verifica se un dispositivo bluetooth è già contenuto nella lista degli Items
        /// </summary>
        /// <param name="bluetoothItem"></param>
        /// <returns></returns>
        bool verifyDevice(BluetoothItem bluetoothItem)
        {
            foreach (BluetoothItem item in Items)
            {
                if (item.address == bluetoothItem.address)
                    return true;
            }

            return false;

        }

    }
}
