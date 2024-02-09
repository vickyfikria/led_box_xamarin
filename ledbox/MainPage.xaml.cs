using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Net;
using System.IO;
using ledbox.Resources;
using System.Threading;
using Acr.UserDialogs;

namespace ledbox
{
    public partial class MainPage : ContentPage
    {
        bool isLoginOpen = false; //definisce se la finestra di login è stata già aperta
       
        public MainPage()
        {
            InitializeComponent();
            changeTitle();

            //carica il file progetto
            App.storage.readFile();

            //cambia l'alias
            changeAlias();

            
            //attiva il listener per gli alert
            MessagingCenter.Subscribe<MainPage,string>(this, "alert",((obj,message) => {
                DisplayAlert(AppResources.warning, message, AppResources.ok);
            }));

            updateDashboard();

            

        }

        /// <summary>
        /// Imposta la tipologia di connessione da instaurare con il LEDBox (Wifi o Bluetooth)
        /// </summary>
        void setConnection()
        {

          

            switch (Preferences.Get("type_connection", "Wi-Fi"))
            {
                case "Wi-Fi":
                    App.conn = new WifiConnection(Navigation, Preferences.Get("ledbox_ip", ""));
                    break;
                case "Bluetooth":
                    App.conn = new BluetoothConnection(Navigation, Preferences.Get("ledbox_bluetooth", ""));
                    break;
            }
           

            App.sbvm.changeIcon();
            

        }

        async void Bt_Setting_Clicked(object sender, System.EventArgs e)
        {
            string action = await DisplayActionSheet(AppResources.preferences, AppResources.cancel, null, AppResources.change_language, AppResources.check_last_ledbox_version,AppResources.open_store, AppResources.download_manual, AppResources.credits);

            if (action == AppResources.change_language) openSelectLanguage();
            if (action == AppResources.credits) openCredits();
            if (action == AppResources.check_last_ledbox_version) check_last_ledbox_version();
            if (action == AppResources.open_store) OpenStore();

            if (action == AppResources.download_manual) DownloadManual();
        }

        private void DownloadManual()
        {
            Device.OpenUri(new Uri("http://ledbox.tech4sport.com/ledbox_manual_" + App.lang + ".pdf"));

        }


        /// <summary>
        /// Apre la schermata per la selezione della lingua
        /// </summary>
        async void openSelectLanguage()
        {
            
            string action = await DisplayActionSheet(AppResources.select_language, AppResources.cancel, null, "Italiano", "English", "Deutsche", "Magyar", "Hrvatski", "Русский");

            if (action != AppResources.cancel)
            {
                switch (action)
                {                                            
                    case "Italiano":
                        Preferences.Set("language", "it-IT");
                        break;
                    case "English":
                        Preferences.Set("language", "en");
                        break;
                    case "Deutsche":
                        Preferences.Set("language", "de");
                        break;
                    case "Magyar":
                        Preferences.Set("language", "hu");
                        break;
                    case "Hrvatski":
                        Preferences.Set("language", "hr");
                        break;
                    case "Русский":
                        Preferences.Set("language", "ru");
                        break;
                }

                if (App.conn != null) {                

                    App.DisplayAlert(AppResources.disconnect_for_change_language);
                    App.conn.DisconnectToLedbox();
                }                    

                App.setLanguageApp();
                Translator.Instance.Invalidate();
                App.sport.reloadLanguage();
                //this.ToolbarItems.Clear();


                
               // Application.Current.MainPage = new MainPage();
                App.ReloadApp(new MainPage());

                //App.main.InitializeComponent();
                App.sbvm.notifyChanges();
                


            }     

        }

        /// <summary>
        /// Apre la schermata per la selezione della tipologia di connessione con il LEDBox
        /// </summary>
        async void openSelectConnection()
        {
            if(App.conn!=null)
                App.conn.DisconnectToLedbox();


            if (Device.RuntimePlatform == Device.iOS)
                App.conn = new WifiConnection(Navigation, Preferences.Get("ledbox_ip", ""));
            else
            {
                string action = await DisplayActionSheet(AppResources.select_connection, AppResources.cancel, null, "Wi-Fi", "Bluetooth");
                switch (action)
                {
                    case "Wi-Fi":
                        Preferences.Set("type_connection", "wifi");
                        break;
                    case "Bluetooth":
                        Preferences.Set("type_connection", "bluetooth");
                        break;

                }
            }

            setConnection();
        }

        /// <summary>
        /// verifica e scarica l'ultimo aggiornamento LEDbox
        /// </summary>
        async void check_last_ledbox_version()
        {
            if (!App.isInternetConnection(true))
            {
               
                return;
            }
            float current_version = Preferences.Get("last_update_ledbox_version", 0f);

            webservice.updateLedbox update = await App.webservice.GetLastUpdateLedbox();

            if (current_version < update.version)
            {
                App.DownloadLastUpdateLedbox();

            }
            else
            {

                //verifica se il file è presente sul device o meno
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                if (!File.Exists(Path.Combine(directory, update.file)))
                {
                    App.DownloadLastUpdateLedbox(true);
                }
                else
                {
                    App.DisplayAlert(AppResources.you_have_last_ledbox_upgrade + " (" + current_version.ToString() + ")");

                }
            }


        }

        Thread closeInfoMessage = new Thread(() => {
            Thread.Sleep(5000);
            App.conn.SendMessage(App.api.createLayoutMessage("waiting"));
        });

        void open_layout_parameters()
        {
            if (App.conn == null || !App.conn.isConnected())
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
            else
            {
                App.conn.SendMessage(App.api.createShowInfoMessage());
                closeInfoMessage.Start();



            }
        }





        void Bt_changeAlias(object sender, System.EventArgs e)
        {
            changeAlias(true);
        }


        /// <summary>
        /// Cambia il nome utente
        /// </summary>
        /// <param name="force"></param>
        public void changeAlias(bool force=false)
        {

            if (isLoginOpen) //evita che possa essere aperta più volte la stessa window
                return;


            if (App.alias == null || App.alias == "" || force)
            {
                isLoginOpen = true;

               

                LoginView login = new LoginView();
                login.Disappearing+= ((s,o) =>
                {
                    isLoginOpen = false;

                    changeTitle();
                    //inizializza i parametri di default
                    if (!Preferences.Get("init_"+App.sport.name, false))
                    {
                        loadDefaultContent();
                        Preferences.Set("init_"+App.sport.name, true);

                    }

                    //crea le cartelle
                    string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.sport.name);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.sport.name,App.DIRECTORY_INTERFACES);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.sport.name, App.DIRECTORY_PRACTICES);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.sport.name, App.DIRECTORY_PRACTICE_PRESET);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.sport.name, App.DIRECTORY_PLUGINS);

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);


                    copyleboxjs();

                });
                Navigation.PushModalAsync(login);
            }


        }


        /// <summary>
        /// Apre la finestra delle preferenze del LEDbox
        /// </summary>
        public async void openPreferences()
        {

            

            if (App.conn==null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
                return;
            }
            else
            {
                await Navigation.PushAsync(new SettingView());
            }
            
        }


        /// <summary>
        /// Apre la finestra dei credits
        /// </summary>
        public async void openCredits()
        {
            
                await Navigation.PushAsync(new CreditsView());
            

        }
        async void Bt_Playlist_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new PlaylistView());
        }

        async void Bt_MatchScore_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new MatchScoreView());

        }        

        async void Bt_Practice_Clicked(object sender, System.EventArgs e)
        {

            //TODO modificato per consentire il recupero delle practice dal LEDBox 14/12/2019s
            //await Navigation.PushAsync(new PracticeTabbedView(AppResources.practice, Color.DarkSeaGreen, 0));

            await Navigation.PushAsync(new PracticeView(AppResources.practice, Color.DarkSeaGreen, 0));



        }

        async void Bt_WeightGym_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new PracticeView(AppResources.weight_gym, Color.DarkSeaGreen, 1));



        }

        async void Bt_Text_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new CustomListView());



        }

        /// <summary>
        /// Scarica dallo store tutti gli item di default da installare sull'App
        /// </summary>
        public async void loadDefaultContent()
        {
            //scarica la lista delle interfacce
            List<StoreItem> storeItems = await App.webservice.GetStorePreInstall(App.sport.name);

            if (storeItems != null)
            {
                //DependencyService.Get<IMessage>().ShortAlert(AppResources.downloading_preinstall);

                //trova le interfacce che hanno la flag preinstall
                foreach (StoreItem storeItem in storeItems)
                {
                    if (storeItem.preinstall)
                    {
                        storeItem.download_and_install((finish) =>
                        {
                       
                        });
                    }
                }
            }
        }

        
        /// <summary>
        /// Cambia il titolo della finestra
        /// </summary>
        void changeTitle()
        {
            this.Title = "LEDbox " + App.sport;
        }

        /// <summary>
        /// Apre la schermata dello store online per i plugin
        /// </summary>
        async void OpenStore()
        {

            if (!App.isInternetConnection(true))
            {
                return;
            }

            StoreView sv = new StoreView(webservice.PLUGIN_CATEGORY);
            sv.Disappearing += (s, obj) =>
            {
                //aggiorna la dashboard
                updateDashboard();
            };


            await Navigation.PushAsync(sv);

        }

        /// <summary>
        /// Aggiorna i pulsanti della dashboard
        /// </summary>
        void updateDashboard()
        {
            //verifica la presenza di plugin
            App.pluginFiles = helper.updateListStore(App.DIRECTORY_PLUGINS,"plugin");
            listPlugins.Children.Clear();
            if (App.pluginFiles.Count() > 0)
            {
                foreach (StoreItem plugin in App.pluginFiles)
                {
                    Button button = new Button();
                    button.Style = new Style(typeof(Button)) { BaseResourceKey = "btnPluginDashboard" };
                    button.Text = plugin.name;
                    button.Clicked += Bt_open_plugin;
                    listPlugins.Children.Add(button);

                }

            }

        }


        async void Bt_open_plugin(object sender, System.EventArgs e)
        {
            copyleboxjs();

            Button btn = (Button)sender;
            //cerca il plugin
            foreach (StoreItem plugin in App.pluginFiles)
            {
                if (plugin.name == btn.Text)
                {
                    if (App.conn == null || !App.conn.isConnected())
                    {
                        App.DisplayAlert(AppResources.connecting_before_ledbox);
                        return;
                    }

                    var loading = UserDialogs.Instance.Loading(AppResources.loading, () => { }, AppResources.cancel);

                    if (plugin.zipfile != "")
                    {
                        App.conn.startUploadFile(plugin.zipfile, plugin.zipfile_path, "", (onFinish) =>
                        {
                            if (onFinish != "")
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    openPluginView(plugin);
                                });
                                loading.Hide();
                            }


                        }, APILedbox.FILETYPE_INTERFACE);
                    }
                    else
                    {
                        openPluginView(plugin);
                        loading.Hide();
                    }
                   
                }
            }
        }

        async void openPluginView(StoreItem m)
        {

            if (m.permission != "") //verifica i permessi solo se sono stati impostati
            {
                if (App.role == "guest")
                    if (m.permission == StoreItem.PERMISSION_ONLY_ADMIN)
                    {
                        App.DisplayAlert(AppResources.interface_for_only_admin);
                        return;
                    }
            }

            if (m.allow_connection != "" && m.allow_connection != "all") //verifica se c'è una restrizione nella tipologia di connessione
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
            
        }


        void copyleboxjs()
        {

            //verifica se nella cartella delle interfacce è presente il file ledbox.js
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.DIRECTORY_PLUGINS);

            //verifica che la cartella sia presente
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            //if (!File.Exists(directory + "/ledbox.js"))
            DependencyService.Get<IDirectory>().copyFile("ledbox.js", directory + "/ledbox.js");
            DependencyService.Get<IDirectory>().copyFile("functions.js", directory + "/functions.js");

        }

    }
}
