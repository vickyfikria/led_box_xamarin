using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
using ledbox.Resources;
using Acr.UserDialogs;

namespace ledbox
{
    public partial class MatchScoreView : ContentPage
    {

        ObservableCollection<StoreItem> interfaces { get; set; }

        private bool oneclick = false; //necessario per impedire la doppia apertura dell'interface


        public MatchScoreView()
        {
            InitializeComponent();

            interfaces = new ObservableCollection<StoreItem>();

            copyleboxjs();
            getListInterfaces();



        }



        void copyleboxjs()
        {

            //verifica se nella cartella delle interfacce è presente il file ledbox.js
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.DIRECTORY_INTERFACES);

            //verifica che la cartella sia presente
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            //if (!File.Exists(directory + "/ledbox.js"))
            DependencyService.Get<IDirectory>().copyFile("ledbox.js", directory + "/ledbox.js");
            DependencyService.Get<IDirectory>().copyFile("functions.js", directory + "/functions.js");

        }


        void getListInterfaces()
        {
            interfaces.Clear();

            App.interfaceFiles=helper.updateListStore(App.DIRECTORY_INTERFACES,"interface");

            foreach (StoreItem interface_obj in App.interfaceFiles)
                interfaces.Add(interface_obj);

            lstView.ItemsSource = interfaces;

            lblEmpty.IsVisible = interfaces.Count > 0 ? false:true ;
 
        }

        async void Disinstall_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as MenuItem;
            var msvm = button.BindingContext as StoreItem;

            var answer = await DisplayAlert(AppResources.disinstallation, AppResources.confirm_disinstallation, AppResources.yes, AppResources.no);
            if (answer)
            {

                msvm.disinstall();
                getListInterfaces();
            }

        }

        async void Bt_Store_Clicked(object sender, System.EventArgs e)
        {
           
            if (!App.isInternetConnection(true))
            {
                return;
            }

            StoreView sv = new StoreView(webservice.INTERFACE_CATEGORY);
            sv.Disappearing += (s, obj) =>
            {
                getListInterfaces();
            };


            await Navigation.PushAsync(sv);

        }

        void lstView_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

            if (oneclick)
                return;

            

            oneclick = true;

            StoreItem m = (StoreItem)e.Item;

            //TODO per test di interfaccia
            //testInterfaceView(m);
            //return;

            if (m.access != "" && m.access != null)
            {
                if (m.access != App.role)
                {
                    App.DisplayAlert(AppResources.access_denied);
                    return;
                }
            }

            
            if (App.conn != null)
            {
                if (!App.conn.isConnected())
                {
                    App.DisplayAlert(AppResources.connecting_before_ledbox);
                    oneclick = false;
                    return;
                }
                else
                {
                    var loading = UserDialogs.Instance.Loading(AppResources.loading, () => { }, AppResources.cancel);

                    if (m.zipfile != "")
                    {
                        App.conn.startUploadFile(m.zipfile, m.zipfile_path, "", (onFinish) =>
                        {
                            if (onFinish != "")
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    openInterfaceView(m);
                                });
                                loading.Hide();
                            }


                        }, APILedbox.FILETYPE_INTERFACE);
                    }
                    else
                    {
                        openInterfaceView(m);
                        loading.Hide();
                    }

                }
            }
            else
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                oneclick = false;
            }

            return;
        }

        /// <summary>
        /// Usata solo ai fini di debug (esclude il controllo di connessione LEDbox)
        /// </summary>
        /// <param name="m"></param>
        async void testInterfaceView(StoreItem m)
        {
            await Navigation.PushAsync(new InterfaceView(m,true));
            oneclick = false;
        }


        async void openInterfaceView(StoreItem m)
        {

            if (!File.Exists(m.local_file))
            {
                App.DisplayAlert("File dell'interfaccia non presente. Reinstallare l'interfaccia");
                return;
            }

            if (m.permission != "") //verifica i permessi solo se sono stati impostati
            {
                if (App.role == "guest")
                    if (m.permission == StoreItem.PERMISSION_ONLY_ADMIN)
                    {
                        App.DisplayAlert(AppResources.interface_for_only_admin);
                        return;
                    }
            }

            if(m.allow_connection !="" && m.allow_connection!="all") //verifica se c'è una restrizione nella tipologia di connessione
            {
                if (App.conn.getType() != m.allow_connection)
                {
                    if (m.allow_connection == App.CONNECTION_LAN && App.conn.getType() != App.CONNECTION_LAN)
                    {
                        App.DisplayAlert(AppResources.interface_for_only_wifi_connection);
                        return;
                    }
                    if (m.allow_connection == App.CONNECTION_BLUETOOTH && App.conn.getType() != App.CONNECTION_BLUETOOTH)
                    {
                        App.DisplayAlert(AppResources.interface_for_only_bluetooth_connection);
                        return;
                    }
                }
            }


            await Navigation.PushAsync(new InterfaceView(m));
            oneclick = false;
        }
    }
}
