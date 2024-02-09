using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Essentials;
using Plugin.Connectivity;
using ledbox.Resources;
using Acr.UserDialogs;
using System.Threading.Tasks;

namespace ledbox
{

    public class WifiConnection:ConnectionInterface, IDisposable
    {

        const int ACTION_CONNECT = 1;



        string ipLedbox; //IP del LEDbox

        //bool isconnected = false; //stato della connessione attuale
        Socket socket; //socket corrente sul LEDbox
        //delegate void onEventUploadFinish();
        //event onEventUploadFinish OnUploadFinish;
        Thread listen;
        System.Threading.Timer _checkConnection;
        INavigation Navigation;
        int mode=0;
        IProgressDialog progress;

        Thread th_scan;


        public string getType()
        {
            return "lan";
        }


        public WifiConnection(INavigation navigation,string ip)
        {

            setAddress(ip);
            this.Navigation = navigation;

        }

        /// <summary>
        /// Restituisce lo stato di connessione
        /// </summary>
        /// <returns><c>true</c>, if connected was ised, <c>false</c> otherwise.</returns>
        public bool isConnected()
        {
            try
            {
                if (socket == null)
                    return false;

                if (((socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0)))
                    return false;

                if (!socket.Connected)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;


        }




        public async void Connect(Action<bool> isconnected)
        {


            if (isConnected())
            {
                isconnected(true);
                return;

            }




            //verifica i permessi
            if (!DependencyService.Get<IPermissions>().checkPermission(2))
            {
                return;
            }

            

            if (!DependencyService.Get<IIPAddressManager>().CheckWifiConnection())

            //verifica che ci sia una connessione wifi
            {
                await App.main.DisplayAlert(AppResources.warning, AppResources.connect_to_wifi, AppResources.ok);
                return;
            }


            //verifica che la connessione wifi corrente non sia di un ledbox in direct mode
            string current_ssid = DependencyService.Get<IIPAddressManager>().CurrentWifiConnection();
            
            if (current_ssid.Contains("ledbox_"))
            {

                //verifica i permessi
                if (!DependencyService.Get<IPermissions>().checkPermission(3))
                {
                    return;
                }


                /*
                Version version = DeviceInfo.Version;

                if (version.Major == 9) //per Android 9 bisogna controllare che i dati mobile siano disattivi per instradare sulla Wlan
                {
                    //verifica se i dati mobile sono attivi
                    bool mobile = DependencyService.Get<IIPAddressManager>().CheckDataMobile();

                   // DependencyService.Get<IWifi>().ForceWifiOverCellular(); //TODO da provare con android 9

                    
                    if (mobile)
                    {
                        App.DisplayAlert(AppResources.disable_mobile_connection);

                        // Android.App.Application.Context.StartActivity(new Android.Content.Intent(Android.Provider.Settings.ActionWifiSettings).SetFlags(ActivityFlags.NewTask));
                        //TODO provare ad aprire la finestra di disconessione dati mobili

                       
                        
                        return;
                    }
                }*/

                await _try_to_connect("172.24.1.1");
            }
                
            else
            {

                if (Preferences.Get("ledbox_ip","") != "")
                {
                    bool pResult = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                    {
                        OkText = AppResources.yes,
                        CancelText = AppResources.no,
                        Message = AppResources.connect_last_ledbox + "\n\nSN:" + Preferences.Get("ledbox_deviceName", "") + " IP:" + Preferences.Get("ledbox_ip", "")

                    });

                    if (pResult)
                    {
                        //prova con i valori prememorizzati
                        if (!await _try_to_connect(Preferences.Get("ledbox_ip", "")))
                        {
                              _scan_and_connect();
                        }
                    }
                    else
                    {
                        _scan_and_connect();
                    }
                }
                else
                {
                    _scan_and_connect();
                }



                /*

                //prova con i valori prememorizzati
                if (!await _try_to_connect(Preferences.Get("ledbox_ip", "")))
                {
                    //prova la scansione di rete
                    string ipFounded = await scanNetwork();
                    await _try_to_connect(ipFounded,true);

                    bool pResult = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
                    {
                        OkText = AppResources.yes,
                        CancelText = AppResources.no,
                        Message = "Ricercare altri ledbox nella rete?"

                    });

                    if (pResult)
                    {
                        //ConnectToLedbox(ip);
                    }
                    

                }*/
              
            }
           
        }


        /// <summary>
        /// Cerca i LEDbox nella rete
        /// </summary>
        async void _scan_and_connect(int startFrom=1)
        {
            //prova la scansione di rete
            string ipFounded = await scanNetwork(startFrom);

            string current_ip = DependencyService.Get<IIPAddressManager>().GetIPAddress();

            string default_ledbox_ip = current_ip;


            if (ipFounded == "")
            {
                PromptResult pResult1 = await UserDialogs.Instance.PromptAsync(new PromptConfig
                {
                    InputType = InputType.Name,
                    OkText = AppResources.ok,
                    CancelText = AppResources.cancel,
                    Title = AppResources.ledbox_not_found + "\n" + AppResources.enter_ip_ledbox,
                    Text = default_ledbox_ip

                });

                if (pResult1.Ok)
                {
                    ipFounded = pResult1.Text;
                }

            }


            if (ipFounded == "")
                return;
            //prendi l'ultmo numero dell'ip
            int lastByteIp = int.Parse(ipFounded.Split('.')[3]);

            await _try_to_connect(ipFounded, true);

            bool pResult = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig
            {
                OkText = AppResources.yes,
                CancelText = AppResources.no,
                Message = AppResources.search_other_ledbox
            });

            if (pResult)
            {
                //disconnettiti da quello corrente
                if(App.conn!=null)
                    App.conn.DisconnectToLedbox();

                _scan_and_connect(lastByteIp + 1);
            }
        }



        async Task<bool> _try_to_connect(string ip,bool force=false)
        {

            //DependencyService.Get<IMessage>().ShortAlert(AppResources.connecting_ledbox + " " + ip);


            if (ip == "" && !force)
                return false;



            if (ip == "")
            {

                while (!isConnected())
                {

                    PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
                    {
                        InputType = InputType.Name,
                        OkText = AppResources.ok,
                        CancelText = AppResources.cancel,
                        Title = AppResources.ledbox_not_found + "\n" + AppResources.enter_ip_ledbox,
                        Text = "172.24.1.1"

                    });

                    if (pResult.Ok)
                    {
                        ip = pResult.Text;
                        ConnectToLedbox(ip);
                    }
                    else
                    {
                        return false;
                    }

                }


            }
            


            if (ConnectToLedbox(ip))
            {
                //DependencyService.Get<IMessage>().LongAlert(AppResources.success_connection);
                Preferences.Set("ledbox_ip", ip);
                return true;
            }
            else
            {
                DependencyService.Get<IMessage>().LongAlert(AppResources.error_connection+" "+ip);
                Preferences.Set("ledbox_ip", "");
                return false;
            }
        }

        /// <summary>
        /// Connects to ledbox.
        /// </summary>
        /// <returns><c>true</c>, if to ledbox was connected, <c>false</c> otherwise.</returns>
        public bool ConnectToLedbox(string ip="")
        {

            if (ip != "")
                ipLedbox = ip;

            if (ipLedbox == "")
            {
                return false;
            }




            IPAddress host = IPAddress.Parse(ipLedbox);
            int port = 8889;

            try
            {
                socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                IAsyncResult result = socket.BeginConnect(host, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(2000, true);

                if (socket.Connected)
                {
                    Preferences.Set("ledbox_ip", ipLedbox);
                    Console.WriteLine("Connection established");
                    try
                    {
                     
                        //avvia il listenre per i messaggi che arrivano
                        listen = new Thread(onMessageReceived);
                        listen.Name = "Wifi Listener";
                        listen.Start();

                        //avvia il controllo della connessione (verifica ogni due secondi)
                        _checkConnection = new System.Threading.Timer(checkConnection, null, 0, 2000);
                        

                        //invia il messaggio di inizializzazione al LEDbox
                        SendMessage(App.api.createInitMessage(App.alias));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }

                    //socket.EndConnect(result);
                    return isConnected(); //ulteriore test
                }
                else
                {
                    // NOTE, MUST CLOSE THE SOCKET

                    socket.Close();
                    Console.WriteLine("Connection not established");
                    return false;
                }

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disconnects to ledbox.
        /// </summary>
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

            if (_checkConnection != null)
            {
                _checkConnection.Change(Timeout.Infinite, Timeout.Infinite);
                var waiter = new ManualResetEvent(false);
                _checkConnection.Dispose(waiter);
                _checkConnection = null;
            }


            try
            {
                if (listen != null)
                    listen.Abort();
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Thread Listen Abort");

                Thread.ResetAbort();
            }


            if (socket!=null)
                socket.Close();

            socket = null;
            
            try
            {
                MessagingCenter.Send<APILedbox>(App.api, "disconnect");
                MessagingCenter.Send<APILedbox, string>(App.api, "stop_all", "");
            }
            catch (Exception e)
            {
                Console.Write("Error to disconnect LEDbox " + e.ToString());
            }

        }

        /// <summary>
        /// Messaggi ricevuti dal socket
        /// </summary>
        void onMessageReceived()
        {
            byte[] bytes = new byte[2048];
            while (true)
            {
                try
                {
                    
                    int bytesRec = socket.Receive(bytes);
                    //string message = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    string message = helper.UnCompressString(bytes);
                    App.api.processMessage(message); //processa il messaggio
                }
                catch
                {
                    Console.WriteLine("connection broken");
                    continue;
                }

            }
        }



        /// <summary>
        /// Invia un messaggio al LEDbox
        /// </summary>
        /// <param name="message">Message.</param>
        public void SendMessage(string message)
        {
            if (!isConnected())
            {
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
                return;
            }

            try
            {
                Byte[] data = helper.CompressString(message);

                //Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                socket.Send(data);
                Console.WriteLine("MESSAGE SEND: " + message);
                MessagingCenter.Send<APILedbox>(App.api, "message_send");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore durante l'invio del messaggio " +ex.Message);
                DisconnectToLedbox();
                if(!ConnectToLedbox())
                    App.DisplayAlert(AppResources.error_connection_to_ledbox); 
            }
        }






        public void startUploadFile(string customMessageApiUpload, Action<string> isFinish, string type, bool forceUpload = false)
        {
            startUploadFile("", "", "", (value) => { isFinish(value); }, type, forceUpload, customMessageApiUpload);

        }



            /// <summary>
            /// Avvia il processo di upload di un file sul LEDbox
            /// </summary>
            /// <param name="filename">Filename.</param>
            /// <param name="filePath">File path.</param>
            public void startUploadFile(string filename,string filePath,string alias,Action <string> isFinish,string type,bool forceUpload=false,string customMessageApiUpload="")
        {

            //escludi se il file non è stato definito oppure non è specificato un messaggio di upload custom
            if (customMessageApiUpload == "" && (filePath==null || filePath == ""))
            {
                isFinish("");
                return;
            }

            //se non è specificato un messaggio API di upload custom
            if (customMessageApiUpload == "")
                customMessageApiUpload = App.api.createFileUploadMessage(filename, filePath, alias, type, forceUpload);

            MessagingCenter.Unsubscribe<ConnectionInterface, APILedbox.file>(this, "uploaded");
            SendMessage(customMessageApiUpload);

            MessagingCenter.Subscribe<ConnectionInterface, APILedbox.file>(this, "uploaded",((value,file)=> {

                Console.WriteLine("file uploaded "+file.filepath);
                isFinish(file.filepath);
                MessagingCenter.Unsubscribe<ConnectionInterface, APILedbox.file>(this, "uploaded");

            }));


        }


        /// <summary>
        /// Invia un file sul LEDbox
        /// </summary>
        /// <param name="filePath">File path.</param>
        public void sendFile(string filePath,bool isExist,bool forceUpload=false)
        {
            if (isExist && !forceUpload)
            {
                MessagingCenter.Send<ConnectionInterface, APILedbox.file>(this, "uploaded",new APILedbox.file() { filepath = filePath });
                return;
            }

            var loading = UserDialogs.Instance.Progress("Upload",()=>{ },AppResources.cancel);


            TcpClient client = new TcpClient(ipLedbox, 12345);
            byte[] SendingBuffer = null;
            NetworkStream netstream = null;
            int BufferSize = 1024;
            try
            {

                Console.Write("Connected to the Server...\n");

                netstream = client.GetStream();

                FileStream Fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
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
                    netstream.Write(SendingBuffer, 0, (int)SendingBuffer.Length);

                    float percent = (1 - (float)TotalLength / (float)Fs.Length) * 100;

                    loading.PercentComplete = (int)percent;

                }
                //Navigation.PopModalAsync();
                Console.WriteLine("Upload finish");
                loading.Hide();
                //OnUploadFinish();
                Fs.Close();
            }
            catch (Exception ex)
            {
                loading.Hide();
                Console.WriteLine(ex.Message);
            }
            finally
            {
                netstream.Close();
                client.Close();
            }
        }


        public void setModeListener(int mode = 0)
        {
            this.mode = mode;
        }

        void checkConnection(object info)
        {
           
            if (!isConnected())
            {
                DisconnectToLedbox();
            }

        }

        public void setAddress(string address)
        {
            ipLedbox = address;
        }


        public string getAddress()
        {
            return ipLedbox;
        }

        public void Dispose()
        {
            
        }


        async Task<string> scanNetwork(int startFrom=1)
        {

            bool stopTask = false;

            progress=UserDialogs.Instance.Progress(AppResources.ledbox_search,()=> {
                stopTask = true;
            }, AppResources.cancel);

            string ipFounded= await Task.Run(() => { 
                //prendi l'ip corrente della wifi

                string current_ip = DependencyService.Get<IIPAddressManager>().GetIPAddress();


                Console.WriteLine("Current IP :" + current_ip);

                if (current_ip == "")
                {
                    return "";
                    
                }
                

                string[] ipNumber = current_ip.Split('.');
                string ipBase = ipNumber[0] + "." + ipNumber[1] + "." + ipNumber[2] + ".";
                for (int i = startFrom; i < 255; i++)
                {
                    if (stopTask)
                    {
                        progress.Hide();
                        return "";

                    }


                    string ip = ipBase + i.ToString();
                    try
                    {
                        int p = (int)((100f / 255f) * i);
                    
                        progress.PercentComplete = p;
                        progress.Title = AppResources.ledbox_search + "\n" + ip;

                        Console.WriteLine(ip);
                        bool v = IsPortOpen(ip, 8889, TimeSpan.FromSeconds(0.5));  // TIMEOUT WIFI
                        if (v)
                        {
                            progress.Hide();
                            return ip;
                        }
                    }
                    catch
                    {
                        continue;
                    }

                    
                }
                progress.Hide();
                return "";
            });

            return ipFounded;

        }


         bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                if (s.ConnectAsync(host, port).Wait(200))
                {
                    s.Disconnect(false);
                    
                    return true;
                }
                else
                {
                    return false;
                }
                /*
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                    if (client.Connected)
                    {
                        client.EndConnect(result);
                        return true;

                    }
                    else
                    {
                        return false;
                    }
                   /* if (!success)
                    {
                        return false;
                    }*/

                   
                //}

            }
            catch
            {
                return false;
            }
            return true;
        }




        public bool checkPermission(int action)
        {

            Version version = DeviceInfo.Version;

            switch (action)
            {
                case ACTION_CONNECT:
                    
                    switch (version.Major)
                    {
                        case 9:
                            //





                            break;
                    }
                    
                    break;
            }


            
            if (version.Major>= 9)
            {

            }


            return true;
        }
    }


}


