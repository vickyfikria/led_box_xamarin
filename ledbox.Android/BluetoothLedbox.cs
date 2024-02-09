using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using ledbox.Droid;
using Xamarin.Forms;
using Java.Util;
using Java.IO;
using System.Threading;
using System.Net.Sockets;
using Acr.UserDialogs;
using Android.Content.PM;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android;

[assembly: Dependency(typeof(BluetoothLedbox))]

namespace ledbox.Droid
{
    public class BluetoothLedbox:IBluetooth
    {

        public static BluetoothAdapter bluetoothAdapter = null;
        private const int REQUEST_ENABLE_BT = 2;

        public BluetoothSocket mySocket;
 
        BluetoothDevice currentBluetoothDevice;
        ICollection<BluetoothDevice> bluetoothDevices;
        Thread listen;
        System.Threading.Timer _checkConnection;
       
        
 
        public void InitializeBluetooth()  // Initialises bluetooth adapter settings
        {

            // Get local Bluetooth adapter
            //bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            // If the adapter is null, then Bluetooth is not supported
            if (bluetoothAdapter == null)
            {
                
                DependencyService.Get<IMessage>().LongAlert("Bluetooth is not available");
                return;
            }

            // If bluetooth is not enabled, ask to enable it
            if (!bluetoothAdapter.IsEnabled)
            {
                var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);

                var activity = (MainActivity)Forms.Context;

                activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);

               




            }

            ActivityCompat.RequestPermissions((MainActivity)Forms.Context, new String[] { Manifest.Permission.AccessCoarseLocation }, 1);


        }





        public void Connect(BluetoothItem bluetoothDevice, Action<bool> isconnected)
        {

            if (isConnected())
            {
                isconnected(true);
                return;

            }


            //verifica se devi effettuare il pairing prima
            if (!bluetoothDevice.paired)
            {
                //cerca tra i dispositivi ricercati
                foreach (BluetoothDevice bluetooth in MainActivity.last_bluetooth_searched)
                {
                    if (bluetooth.Address == bluetoothDevice.address)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DependencyService.Get<IMessage>().ShortAlert("Paring...");
                        });
                        bluetooth.CreateBond();

                        return;
                    }
                }

            }


            //riaggiorna l'elenco dei device paired
            getDevices();


            UUID SerialPortServiceClass_UUID = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");

            if (bluetoothDevices != null && bluetoothDevices.Count > 0) { 
                //cerca il device
                foreach (Android.Bluetooth.BluetoothDevice bluetooth in bluetoothDevices)
                {
                    if (bluetooth.Address == bluetoothDevice.address)
                    {
                        currentBluetoothDevice = bluetooth;


                        try
                        {
                            mySocket = currentBluetoothDevice.CreateRfcommSocketToServiceRecord(SerialPortServiceClass_UUID);
                        }
                        catch (Java.IO.IOException e)
                        {
                            System.Console.WriteLine("Create Socket Failed " + e.ToString());
                        }


                        if (mySocket != null)
                        {
                            try
                            {

                                // connects and will catch an exception - simplest, but does not necessarily spin up a new thread. Signature of method is async Task ConnectAsync
                                
                                mySocket.Connect();
                                if (mySocket.IsConnected)
                                {
                                    isconnected(true);

                                    //avvia il listenre per i messaggi che arrivano
                                    listen = new Thread(onMessageReceived);
                                    listen.IsBackground = true;
                                    listen.Start();

                                    //avvia il controllo della connessione (verifica ogni due secondi)
                                    _checkConnection = new System.Threading.Timer(checkConnection, null, 0, 2000);

                                    //invia il messaggio di inizializzazione al LEDbox
                                    SendMessage(App.api.createInitMessage(App.alias));


                                }
                            }
                            catch (Java.IO.IOException ex)
                            {
                                System.Console.WriteLine("ERROR Bluetooth Socket Connection Failed " + ex.Message);
                                isconnected(false);


                            }




                        }
                    }
                }
            }
            

                
        }


        public bool sendFile(string filePath, bool isExist, bool forceUpload = false)
        {
            if (isExist && !forceUpload)
            {
                return false;
            }

            UUID SerialPortServiceClass_UUID = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FC");
            BluetoothSocket client = currentBluetoothDevice.CreateRfcommSocketToServiceRecord(SerialPortServiceClass_UUID);
            client.Connect();


            var loading = UserDialogs.Instance.Progress("Upload", () => { }, "Cancel");

            

            byte[] SendingBuffer = null;
            
            int BufferSize = 1024;
            FileStream Fs=null;
            try
            {

               

               // netstream = client.OutputStream;

                Fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,FileShare.None);
                int NoOfPackets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(Fs.Length) / Convert.ToDouble(BufferSize)));

                int TotalLength = (int)Fs.Length, CurrentPacketLength;
                for (int i = 0; i < NoOfPackets; i++)
                {
                    if (TotalLength > BufferSize)
                    {
                        CurrentPacketLength = BufferSize;
                        TotalLength = TotalLength - CurrentPacketLength;
                    }
                    else
                        CurrentPacketLength = TotalLength;

                    SendingBuffer = new byte[CurrentPacketLength];
                    Fs.Read(SendingBuffer, 0, CurrentPacketLength);

                    client.OutputStream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                    float percent = (1-(float)TotalLength / (float)Fs.Length) *100;

                    loading.PercentComplete = (int)percent;



                }
                //Navigation.PopModalAsync();

                //OnUploadFinish();
                loading.Hide();
                Fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write("Error Send File Bluetooth " + ex.ToString());
                if(Fs!=null)
                    Fs.Close();
                loading.Hide();
                return false;
            }
            finally
            {
                client.OutputStream.Close();
                client.Close();
            
            }
        }


        /// <summary>
        /// Messaggi ricevuti dal socket
        /// </summary>
        void onMessageReceived()
        {
            
            
            while (mySocket!=null)
            {
                int bytesRec = 0;
                //int cbytes = 0;
                byte[] bytes = new byte[2048];
                try
                {
                    bytesRec = mySocket.InputStream.Read(bytes, bytesRec, 2048 - bytesRec);

                    //bytesRec = mBTInputStream.Read(bytes, bytesRec, 2048- bytesRec);

                    string message = helper.UnCompressString(bytes);
                    App.api.processMessage(message); //processa il messaggio

                    mySocket.InputStream.Flush();

                }
                catch(Exception ex)
                {
                    System.Console.WriteLine("connection broken "+ex.Message);
                    Disconnect();
                    //continue;
                }

            }
        }


        /// <summary>
        /// Get list of devices present into smartphone
        /// </summary>
        /// <returns></returns>
        public List<BluetoothItem> getDevices()
        {

           List<BluetoothItem> bluetoothDevicesResult = new List<BluetoothItem>();

           bluetoothDevices = bluetoothAdapter.BondedDevices;
           if(bluetoothDevices!=null)
                foreach(Android.Bluetooth.BluetoothDevice bluetooth in bluetoothDevices)
                {
                    //ottieni l'elenco dei servizi Bluetooth del dispositivo
                    ParcelUuid[] uuids = bluetooth.GetUuids();

                    if(uuids != null) //se non ci sono servizi non aggiungerlo alla lista
                        bluetoothDevicesResult.Add(new BluetoothItem()
                        {
                            UUID = bluetooth.GetUuids().First<ParcelUuid>().ToString(),
                            name=bluetooth.Name,
                            address=bluetooth.Address,
                            paired = true
                        });

                }
            // Get connected devices
            return bluetoothDevicesResult;
            
        }




        public void startDiscovery()
        {

            // If bluetooth is not enabled, ask to enable it
            if (!bluetoothAdapter.IsEnabled)
            {
                var enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);

                var activity = (MainActivity)Forms.Context;

                activity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
                return;
            }




           if (bluetoothAdapter.IsDiscovering)
                bluetoothAdapter.CancelDiscovery();

            
            bluetoothAdapter.StartDiscovery();
        }



        /// <summary>
        /// Send message to socket Bluetooth
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            try
            {
                System.Console.WriteLine("SEND " + message);
                byte[] array = helper.CompressString(message);

                mySocket.OutputStream.Write(array, 0, array.Length);
                mySocket.OutputStream.Flush();
                //mBTOutputStream.Write(array, 0, array.Length);
            }
            catch
            {
                Disconnect();
                

            }
        }

        

        public bool isConnected()
        {
            
            if (bluetoothAdapter != null)
                if(bluetoothAdapter.IsEnabled)
                    if(mySocket!=null)
                        return mySocket.IsConnected;
            return false;
        }


        public void Disconnect()
        {
            try
            {
                if (_checkConnection != null)
                {
                    _checkConnection.Change(Timeout.Infinite, Timeout.Infinite);
                    var waiter = new ManualResetEvent(false);
                    _checkConnection.Dispose(waiter);
                    _checkConnection = null;
                }

                /*if (listen != null)
                {
                    //listen.Abort();
                    //listen = null;
                }*/
                if (bluetoothAdapter != null)
                    if (mySocket != null)
                    {
                        resetConnection();
                    }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("Error Disconnect Bluetooth " + e.ToString());
                resetConnection();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<APILedbox>(App.api, "disconnect");
            });
            
        }

        private void resetConnection()
        {

            if (mySocket.InputStream != null)
            {
                mySocket.InputStream.Dispose();
                try { mySocket.InputStream.Close(); } catch (Exception e) { System.Diagnostics.Debug.Write("RESET CONNECTION InputStream ERROR : " + e.ToString()); }
                
            }

            if (mySocket.OutputStream != null)
            {
                mySocket.OutputStream.Dispose();
                try { mySocket.OutputStream.Close(); } catch (Exception e) { System.Diagnostics.Debug.Write("RESET CONNECTION OutputStream ERROR : " + e.ToString()); }
                
            }

            
            if (mySocket != null)
            {
                try {
                    mySocket.Close();
                    mySocket.Dispose();
                } catch (Exception e) { System.Diagnostics.Debug.Write("RESET CONNECTION Socket ERROR : " + e.ToString()); }


                mySocket = null;
            }

            currentBluetoothDevice = null;
        }

        void checkConnection(object info)
        {

            if (!isConnected())
            {
                Disconnect();
            }

        }


    }
}
