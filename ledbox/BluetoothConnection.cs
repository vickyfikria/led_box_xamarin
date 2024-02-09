using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ledbox.Resources;
using Xamarin.Forms;
using Acr.UserDialogs;

namespace ledbox
{
    public class BluetoothConnection: ConnectionInterface, IDisposable
    {

        

        IBluetooth bluetooth;
        int mode;
        INavigation Navigation;


        public BluetoothConnection(INavigation navigation, string mac)
        {

            this.Navigation = navigation;
        }

        public async void Connect(Action<bool> isconnected)
        {
            bluetooth = DependencyService.Get<IBluetooth>();

            List<BluetoothItem> devices = bluetooth.getDevices();

            //verifica se nello stack delle modal c'è già la connectionview
            foreach (Page p in Navigation.ModalStack)
                if (p is BluetoothConnectionView)
                    return;

            BluetoothConnectionView cv = new BluetoothConnectionView(devices);

            
            await this.Navigation.PushAsync(cv);

        }


        public void _connect(BluetoothItem bluetoothSelected, Action<bool> isconnected)
        {

            if (bluetooth == null)
                return;

            //Device.BeginInvokeOnMainThread(() =>
            //{
                bluetooth.Connect(bluetoothSelected, (status) =>
                {
                    if (status)
                    {
                        //DependencyService.Get<IMessage>().LongAlert(AppResources.success_connection);
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().LongAlert(AppResources.error_connection);
                        App.conn = null;
                    }

                    isconnected(status);
                    /*
                    if (status)
                        foreach (Page page in Navigation.NavigationStack)
                        {
                            if (page.GetType() == typeof(BluetoothConnectionView))
                                Navigation.PopAsync(false);
                        }
                    */



                });
            //});
        }


        public bool ConnectToLedbox(string ip = "")
        {

            return false;
        }

        public void DisconnectToLedbox()
        {
            try
            {
                if (App.conn != null && App.conn.isConnected())
                    App.conn.SendMessage(App.api.createDisconnectMessage());
            }
            catch
            {

            }

            if (bluetooth != null)
            {
                

                    Device.BeginInvokeOnMainThread(() => {
                        try
                        {
                            bluetooth.Disconnect();
                        }
                        catch (Exception e)
                        {
                            Console.Write("ERROR DISCONNECT BLUETOOTH " + e.ToString());
                        }
                    });
                
                        MessagingCenter.Send<APILedbox, string>(App.api, "stop_all", "");
                 
                
            }
        }

        public bool isConnected()
        {
            if(bluetooth!=null)

                return bluetooth.isConnected();

            return false;
        }

        public void sendFile(string filePath, bool isExist, bool forceUpload=false)
        {

            if (!bluetooth.sendFile(filePath, isExist, forceUpload))
            {
                MessagingCenter.Send<ConnectionInterface, APILedbox.file>(this, "uploaded", new APILedbox.file() { filepath = filePath });
                //return;
            }
            else
            {
                Console.WriteLine("Upload finish");
            }




            
        }

        public void SendMessage(string message)
        {
            if (bluetooth != null)
            {
                bluetooth.SendMessage(message);
                MessagingCenter.Send<APILedbox>(App.api, "message_send");
            }
        }


        public void startUploadFile(string customMessageApiUpload, Action<string> isFinish, string type, bool forceUpload = false)
        {
            startUploadFile("", "", "", (value) => { isFinish(value); }, type, forceUpload, customMessageApiUpload);

        }


        public void startUploadFile(string filename, string filePath, string alias, Action<string> isFinish, string type,bool forceUpload=false,string customMessageApiUpload = "")
        {
            if (customMessageApiUpload=="" && (filePath == null || filePath == ""))
            {
                isFinish("");
                return;
            }

            if (customMessageApiUpload == "")
                customMessageApiUpload = App.api.createFileUploadMessage(filename, filePath, alias, type, forceUpload);


            MessagingCenter.Unsubscribe<ConnectionInterface, APILedbox.file>(this, "uploaded");
            SendMessage(customMessageApiUpload);

            MessagingCenter.Subscribe<ConnectionInterface, APILedbox.file>(this, "uploaded", ((value, file) => {

                Console.WriteLine("file uploaded " + file.filepath);
                isFinish(file.filepath);
                MessagingCenter.Unsubscribe<ConnectionInterface, APILedbox.file>(this, "uploaded");

            }));
        }




        public void setAddress(string address)
        {
           
        }

        public string getAddress()
        {
            return "";
        }

        public void setModeListener(int mode = 0)
        {
            this.mode = mode;
        }

        public void Dispose()
        {
            bluetooth = null;
        }


        public string getType()
        {
            return "bluetooth";
        }

        public bool checkPermission(int action)
        {
            throw new NotImplementedException();
        }
    }
}
