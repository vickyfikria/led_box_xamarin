using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.IO;
using System.Collections.Generic;
using ledbox.Resources;
using System.Globalization;
using Plugin.Multilingual;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ledbox
{
    public partial class App : Application
    {

        /// <summary>
        /// Versione LEDbox minima richiesta
        /// </summary>
        public const float MINUMUN_VERSION_LEDBOX = 0.50f;
        public static string role = "admin";

        /// <summary>
        /// Impone se l'app è in versione test (condivisione interna) oppure pubblica
        /// </summary>
        public static bool isTestingMode = true;

        public static string passwordTestingMode = "@ledbox2020";

        /// <summary>
        /// Connessione corrente con il LEDbox (Bluetooth o Wifi)
        /// </summary>
        public static ConnectionInterface conn;
        /// <summary>
        /// Gestore delle API con il LEDbox
        /// </summary>
        public static APILedbox api;
        /// <summary>
        /// Nome utente corrente
        /// </summary>
        public static string alias = "";
        /// <summary>
        /// Sport corrente
        /// </summary>
        public static sport sport = null;
        /// <summary>
        /// immagine di attesa LEDbox
        /// </summary>
        public static string image_cover = "";
        /// <summary>
        /// Elenco di tutti gli Sport disponibili
        /// </summary>
        public static IList<sport> sports;
        /// <summary>
        /// Nome del LEDbox corrente
        /// </summary>
        public static string deviceName;
        /// <summary>
        /// Versione del ledbox a cui si è connessi
        /// </summary>
        public static float current_ledbox_version = 0;
        /// <summary>
        /// Ultima versione del firmware ledbox scaricata
        /// </summary>
        public static float last_update_ledbox_version = 0;
        /// <summary>
        /// Layout corrente sul LEDbox
        /// </summary>
        public static string current_ledbox_layout = "";




        /// <summary>
        /// Lingua dell'app
        /// </summary>
        public static CultureInfo lang;
        /// <summary>
        /// Gestore dell'archivio dell'App (su filesystem)
        /// </summary>
        public static storage storage;
        /// <summary>
        /// configurazione correnti del LEDBox
        /// </summary>
        public static List<APILedbox.config> configsLedbox;

        /// <summary>
        /// Interfacce presenti sul device
        /// </summary>
        public static List<StoreItem> interfaceFiles;

        /// <summary>
        /// Plugin presenti sul device
        /// </summary>
        public static List<StoreItem> pluginFiles;

        /// <summary>
        /// Gestore delle comunicazioni verso il server online (LEDbox Management)
        /// </summary>
        public static webservice webservice;

        public static MainPage main;

        public static ledbox.ViewModel.StatusBarViewModel sbvm;
        public static ledbox.ViewModel.ActivityViewModel avm;

        public const string DIRECTORY_INTERFACES = "interfaces";
        public const string DIRECTORY_PRACTICES = "practices";
        public const string DIRECTORY_PLUGINS = "plugins";

        public const string DIRECTORY_PRACTICE_PRESET = "presets/practice";
        public const string CONNECTION_LAN = "lan";
        public const string CONNECTION_BLUETOOTH = "bluetooth";


        public App()
        {
            //imposta se l'app è in modalità test
            isTestingMode = Preferences.Get("testingMode", false);



            //imposta la lingua dell'app
            setLanguageApp();

            //inizializzo il webservice, l'api e lo storage locale
            webservice = new webservice();
            api = new APILedbox();
            storage = new storage();

            //inizializza i componenti
            InitializeComponent();

            //inizializza gli sports
            getSports(Preferences.Get("sport", ""));


            if (isInternetConnection()){

                //scarica gli ultimi aggiornamenti del ledbox
                DownloadLastUpdateLedbox();

                //notifica LEDbox Management se ci sono stati dei device con un upgrade
                NotifyLEDboxManagementUpgrade();

            }
           

            
            //inizializza le classi per la gestione della status bar e dell'activity
            sbvm = new ledbox.ViewModel.StatusBarViewModel();
            avm = new ledbox.ViewModel.ActivityViewModel();

            //Carica alias e immagine di attesa di LEDbox
            sbvm.aliasText = Preferences.Get("alias", "");
            if(App.alias!="" && sbvm.sport!=null)
                image_cover=Preferences.Get("imagecover_" + App.alias + "_" + sbvm.sport.name,"");


            BindingContext = sbvm;

            NavigationPage.SetTitleIconImageSource(this, "icon.png");

            /*
            App.main = new MainPage();

            sbvm.setNavigation(App.main.Navigation);

            NavigationPage.SetTitleIconImageSource(this, "icon.png");
            MainPage = new NavigationPage(App.main);
            */
            ReloadApp(new ledbox.MainPage());



        }


        public static  void ReloadApp(MainPage m)
        {
            App.main = m;

            sbvm.setNavigation(App.main.Navigation);

            //NavigationPage.SetTitleIconImageSource(App, "icon.png");
            Application.Current.MainPage = new NavigationPage(App.main);
        }

        
        public static void DisplayAlert(string message)
        {
            App.main.DisplayAlert(AppResources.warning, message, AppResources.ok);
        }

        void App_OnConnect()
        {
        }


        protected override void OnStart()
        {

            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }


        /// <summary>
        /// Imposta la lingua dell'app
        /// </summary>
        public static void setLanguageApp()
        {
            //Imposta la lingua di default come quella del dispositivo
            CrossMultilingual.Current.CurrentCultureInfo = CrossMultilingual.Current.DeviceCultureInfo;

            //se era stata già impostata una lingua esegui l'override
            if (Preferences.Get("language", "") != "")
                CrossMultilingual.Current.CurrentCultureInfo = new CultureInfo(Preferences.Get("language", "en"), false);

            //attribuisci la lingua al file risorse
            AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;

           
            //memorizza la lingua seleziona nella variabile globale
            App.lang = AppResources.Culture;


        }

        /// <summary>
        /// Convert second in format MM:SS
        /// </summary>
        /// <param name="second"></param>
        /// <returns></returns>
        public static string formatSecondToMinute(int second,string format="mm:ss")
        {

            TimeSpan t = TimeSpan.FromSeconds(second);
            DateTime dateTime = DateTime.Today.Add(t);
            return dateTime.ToString(format);

        }





        /// <summary>
        /// Set to LEDbox the settings
        /// </summary>
        /// <param name="config"></param>
        public static void setConfigLedbox(APILedbox.config config)
        {
            if (App.configsLedbox == null)
                App.configsLedbox = new List<APILedbox.config>();


            //trova il settaggio
            for (int i = 0; i < App.configsLedbox.Count; i++)
            {
                APILedbox.config c = App.configsLedbox[i];
                if (c.section == config.section && c.field == config.field)
                {
                    c.value = config.value;
                    return;

                }
            }

            App.configsLedbox.Add(config);

        }



        /// <summary>
        /// Get from LEDbox the current settings
        /// </summary>
        /// <param name="section"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static APILedbox.config getConfigLedbox(string section, string field)
        {
            APILedbox.config config = new APILedbox.config();

            if (App.configsLedbox == null)
                return config;


            //trova il settaggio
            for (int i = 0; i < App.configsLedbox.Count; i++)
            {
                APILedbox.config c = App.configsLedbox[i];
                if (c.section == section && c.field == field)
                {
                    return c;

                }
            }

            return config;

        }


        /// <summary>
        /// Get from local and online the list of sports and set the current sport
        /// </summary>
        void getSports(string current_sport)
        {

            sports = new List<sport>();
            sport volleyball = new sport();
            volleyball.name = "Volleyball";
            volleyball.languages = new List<sport_language>();
            volleyball.languages.Add(new sport_language() { language = "it-IT", value = "Pallavolo" });
            volleyball.languages.Add(new sport_language() { language = "en", value = "Volleyball" });
            sports.Add(volleyball);

            sport basketball = new sport();
            basketball.name = "Basketball";
            basketball.languages = new List<sport_language>();
            basketball.languages.Add(new sport_language() { language = "it-IT", value = "Pallacanestro" });
            basketball.languages.Add(new sport_language() { language = "en", value = "Basketball" });
            sports.Add(basketball);

            sport soccer = new sport();
            soccer.name = "Soccer";
            soccer.languages = new List<sport_language>();
            soccer.languages.Add(new sport_language() { language = "it-IT", value = "Calcio" });
            soccer.languages.Add(new sport_language() { language = "en", value = "Soccer" });
            sports.Add(soccer);

            sport tennis = new sport();
            tennis.name = "Tennis";
            tennis.languages = new List<sport_language>();
            tennis.languages.Add(new sport_language() { language = "it-IT", value = "Tennis" });
            tennis.languages.Add(new sport_language() { language = "en", value = "Tennis" });
            sports.Add(tennis);

            sport tabletennis = new sport();
            tabletennis.name = "Table-tennis";
            tabletennis.languages = new List<sport_language>();
            tabletennis.languages.Add(new sport_language() { language = "it-IT", value = "Tennis Tavolo" });
            tabletennis.languages.Add(new sport_language() { language = "en", value = "Table Tennis" });
            sports.Add(tabletennis);

           















            //copia il file json nella cartella personale
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string file_fullpath = directory + "/sports.json";

            bool file_exist = true;

            //verifica se il file non è stato già copiato
            if(!File.Exists(file_fullpath))
                file_exist=DependencyService.Get<IDirectory>().copyFile("sports.json", file_fullpath);

            if (file_exist)
            {
               


                //verifica che il file sia valido
                FileInfo fileInfo = new FileInfo(file_fullpath);
                if (fileInfo.Length == 0)
                {
                    file_exist = DependencyService.Get<IDirectory>().copyFile("sports.json", file_fullpath);
                }

                if (!file_exist)
                    return;

                //apri il file e deserializzalo
                StreamReader stream = new StreamReader(file_fullpath, true);
                string file_stream = stream.ReadToEnd();
                stream.Close();
                List<sport> stmp= helper.parseSportJson(file_stream);
                if (stmp != null)
                    sports = stmp;




            }

            if (sports == null)
            {
                DependencyService.Get<IMessage>().LongAlert(AppResources.error_download);
                return;
            }



            //imposta lo sport corrente
            foreach (sport s in sports)
            {
                if (s.name == current_sport)
                {
                    App.sport = s;
                    return;
                }
            }

            if (sports.Count > 0)
            {
                App.sport = sports[0];
            }

          
        }

        /// <summary>
        /// Scarica dal LEDbox Management l'ultima release del firmware del LEDbox.
        /// </summary>
        public async static void DownloadLastUpdateLedbox(bool force_download=false)
        {
            //verifica se vi è un versione scaricata dell'aggiornamento ledbox
            float current_version = Preferences.Get("last_update_ledbox_version", 0f);

            webservice.updateLedbox update = await webservice.GetLastUpdateLedbox();

            
            //controlla se l'aggiornamento è più recente
            if (current_version < update.version || force_download)
            {
                bool answer = true;
                if(!force_download)
                    answer = await App.main.DisplayAlert(AppResources.update_ledbox, AppResources.online_new_upgrade_ledbox+"\n"+AppResources.will_be_downloaded + " "+(Math.Round((double)(update.size/1000000),2)).ToString()+" Mb", AppResources.yes, AppResources.no);

                if (answer)
                {

                    webservice.DownloadLastUpdateLedbox(update.file, (file_downloaded) =>
                    {
                        if (file_downloaded != "")
                        {
                            //salva nelle preferenze l'ultimo aggiornamento con la versione
                            Preferences.Set("last_update_ledbox_version", update.version);
                            Preferences.Set("last_update_ledbox_file", file_downloaded);
                            App.DisplayAlert(AppResources.download_last_upgrade_completed);
                        
                        }
                    });
                }

                
            }

            
        }


        /// <summary>
        /// Upload last update to Ledbox
        /// </summary>
        public static async void RunUpdateLedbox(string pretitle="",bool exit_to_cancel=false)
        {
            bool answer = await main.DisplayAlert(AppResources.update_ledbox+" "+App.deviceName, pretitle + AppResources.run_update, AppResources.yes, AppResources.no);

            
            if (answer)
                {
                    string filePath = Preferences.Get("last_update_ledbox_file", "");

                    if (filePath != "")
                    {
                        App.conn.startUploadFile("update.zip", filePath, App.alias, (isFinish) =>
                        {

                            if (isFinish == null)
                            {
                                Device.BeginInvokeOnMainThread(() => {
                                    App.DisplayAlert(AppResources.error_upload);
                                    if (exit_to_cancel)
                                        App.conn.DisconnectToLedbox();
                                    
                                });
                                return;
                            }

                            //imposta le variabili per notificare LEDbox Management
                            Preferences.Set("serialnumber_device_upgraded", App.deviceName);
                            Preferences.Set("version_device_upgraded", Preferences.Get("last_update_ledbox_version", 0f));




                            Device.BeginInvokeOnMainThread(() => {
                                App.DisplayAlert(AppResources.ledbox_updated);
                                App.conn.DisconnectToLedbox();
                            });



                            App.conn.SendMessage(App.api.createRebootMessage());

                        }, "", true);
                    }
                    else
                    {
                        Preferences.Remove("last_update_ledbox_version");
                        Preferences.Remove("last_update_ledbox_file");
                    }






                }
                else
                {
                    if (exit_to_cancel)
                        App.conn.DisconnectToLedbox();

                }

           // }, AppResources.yes, AppResources.no);


            
            

        }


        /// <summary>
        /// Invia a LEDbox Management dell'avvenuto aggiornamento del device (solo se è disponibile una rete internet)
        /// </summary>
        public static async void NotifyLEDboxManagementUpgrade()
        {
            string serialnumber_device_upgraded = Preferences.Get("serialnumber_device_upgraded", "");
            float version_device_upgraded = Preferences.Get("version_device_upgraded", 0f );

            //verifica se c'è una notifica da effettuare
            if (serialnumber_device_upgraded != "")
            {
                if(await webservice.SetDeviceUpgraded(serialnumber_device_upgraded, version_device_upgraded.ToString()))
                {
                    //azzera i valori
                    Preferences.Set("serialnumber_device_upgraded", "");
                    Preferences.Set("version_device_upgraded", 0f);

                }


            }
        }



        /// <summary>
        /// Invia sul ledbox l'immagine di attesa
        /// </summary>
        public static void UploadImageCoverToLedbox(bool loadWaitingLayout=false)
        {
            if (App.conn != null) {
                if (App.conn.isConnected())
                    //invia sul LEDbox l'immagine di attesa del LEDbox solo se admin
                    if (App.role == "guest")
                        return;
                    if(App.image_cover!="")
                        App.conn.startUploadFile(App.api.createChangeWaitingMessage(App.image_cover), (isFinish) => {
                            if(loadWaitingLayout)
                                App.conn.SendMessage(App.api.createReloadLayoutMessage("waiting"));
                        }, "", true);
            }
        }


        /// <summary>
        /// Verifica se è presente una connessione internet o meno
        /// </summary>
        /// <returns></returns>
        public static bool isInternetConnection(bool onAlert=false)
        {

            bool result = false;
            result = DependencyService.Get<IPermissions>().checkPermission(5);


            if (onAlert)
            {
                if (!result)
                {
                    //verifica che la connessione wifi corrente non sia di un ledbox in direct mode
                    string current_ssid = DependencyService.Get<IIPAddressManager>().CurrentWifiConnection();

                    if (current_ssid.Contains("ledbox_"))
                        App.DisplayAlert(AppResources.no_internet_wifi_direct);

                    else
                        App.DisplayAlert(AppResources.no_internet);
                }
            }



            return result;

        }



        /// <summary>
        /// Funzioni da avviare una volta effettuata la connessione ad un LEDbox
        /// </summary>
        public static void OnAfterConnect()
        {
            bool isRunLedboxUpdate = false;

            App.last_update_ledbox_version = Preferences.Get("last_update_ledbox_version", 0f);


            if (App.current_ledbox_version < App.MINUMUN_VERSION_LEDBOX)
            {

                //verifica se hai già scaricato l'ultima versione online
                if (App.last_update_ledbox_version >= App.MINUMUN_VERSION_LEDBOX)
                {
                    isRunLedboxUpdate = true; //evita che si avviano due volte la procedura di aggiornamento del ledbox
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.RunUpdateLedbox(AppResources.no_ledbox_compatible + "\n", true);
                    });
                }
                else
                {

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.DisplayAlert(AppResources.no_ledbox_compatible);
                    });

                    App.conn.DisconnectToLedbox();
                    return;
                }

            }

            //Invia l'immagine di attesa al LEDbox
            App.UploadImageCoverToLedbox(App.current_ledbox_layout == "waiting" ? true : false);

            Device.BeginInvokeOnMainThread(() => { App.avm.DialogActivities(); });

            //verifica se il ledbox necessità di un aggiornamento

            if (App.current_ledbox_version < App.last_update_ledbox_version)
            {
                if (!isRunLedboxUpdate)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        App.RunUpdateLedbox();
                    });
            }

            isRunLedboxUpdate = false;
        }

    }
}

