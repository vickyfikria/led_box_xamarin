using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Essentials;
using ledbox.Resources;
using System.Net.NetworkInformation;

namespace ledbox
{
    public partial class ConnectionView : ContentPage
    {

        Thread th_connection;

        public ConnectionView(Action<bool> isconnected)
        {


            InitializeComponent();

            //esegui solo se la finestra si è aperta
            this.Appearing += (object sender, EventArgs e) => {

                // se è già connesso chiudi la finestra
                if (App.conn != null && App.conn.isConnected())
                {
                    isconnected(true);
                    return;
                }


                th_connection = new Thread(new System.Threading.ThreadStart(() => {
                    //effettua la connessione al LedBox
                    if (App.conn!=null && App.conn.ConnectToLedbox())

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lbl_message.Text = AppResources.ledbox_connected;                           
                            bt_ok.IsVisible = true;
                            bt_cancel.IsVisible = false;
                            isconnected(true);


                        });
                    else
                    {
                        string ip;
                        //verifica che la connessione wifi corrente non sia di un ledbox in direct mode
                        string current_ssid = DependencyService.Get<IIPAddressManager>().CurrentWifiConnection();

                        if (current_ssid.Contains("ledbox_"))
                            ip = "172.24.1.1";
                        else
                            ip = scanNetwork(); //prova a trovarlo

                        if (App.conn == null)
                        {
                            isconnected(false);
                            return;
                        }


                        if (ip != "")
                        {
                            if(App.conn.ConnectToLedbox(ip))
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    lbl_message.Text = AppResources.ledbox_connected;                                    
                                    bt_ok.IsVisible = true;
                                    bt_cancel.IsVisible = false;
                                    isconnected(true);
                                });



                            }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lbl_message.Text = AppResources.ledbox_not_found +"\n"+ AppResources.enter_ip_ledbox;                            
                                frame_input.IsVisible = true;
                                input.Text = Preferences.Get("ledbox_ip","");
                                bt_ok.IsVisible = true;
                                bt_cancel.IsVisible = false;

                            });
                            return;
                        }


                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lbl_message.Text = AppResources.error_connection;                      
                            bt_ok.IsVisible = false;
                            bt_cancel.IsVisible = true;
                            isconnected(false);
                        });
                    }
                }));
                th_connection.Start();

            };

        }

        async void Bt_OK_Clicked(object sender, System.EventArgs e)
        {
            if (frame_input.IsVisible)
            {
                App.conn.setAddress(input.Text);
                Preferences.Set("ledbox_ip", input.Text);
                App.conn.ConnectToLedbox(input.Text);
            }
            
            await Navigation.PopModalAsync();
        }

        async void Bt_Cancel_Clicked(object sender, System.EventArgs e)
        {
            if (th_connection != null)
                th_connection.Abort();

            await Navigation.PopModalAsync();
        }

        string scanNetwork()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lbl_message.Text = AppResources.ledbox_search;
            });
           

            //prendi l'ip corrente della wifi

            string current_ip = DependencyService.Get<IIPAddressManager>().GetIPAddress();
            /*
            try
            {
                foreach (IPAddress adress in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    current_ip = adress.ToString();
                    break;
                }
            }
            catch
            {
                current_ip = "";
            }*/

            Console.WriteLine("Current IP :" + current_ip);

            if (current_ip == "")
                return "";

            string[] ipNumber = current_ip.Split('.');
            string ipBase = ipNumber[0] + "." + ipNumber[1] + "." + ipNumber[2] + ".";
            for (int i = 1; i < 255; i++)
            {
                string ip = ipBase + i.ToString();
                try
                {

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        double p = ((100f / 255f) * i) / 100f;
                        progressbar.Progress = p;
                    });
                    Console.WriteLine(ip);
                    bool v = IsPortOpen(ip, 8889, TimeSpan.FromSeconds(1));  // TIMEOUT WIFI
                    if (v)
                        return ip;
                }
                catch
                {
                    continue;
                }
            }
            return "";
        }


        bool IsPortOpen(string host, int port, TimeSpan timeout)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success)
                    {
                        return false;
                    }

                    client.EndConnect(result);
                }

            }
            catch
            {
                return false;
            }
            return true;
        }
        
    }




}
