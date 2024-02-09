using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using ledbox.Resources;
using Acr.UserDialogs;
using System.Threading;

namespace ledbox.ViewModel
{
    public class StatusBarViewModel:INotifyPropertyChanged
    {
        string _labelDeviceName = "";
        string _labelRole = "";
        string _labelBtConnection = AppResources.connect;
        string _iconConnection = "ic_disconnect.png";
        bool _show_message = false;
        

        string _message_button="";
        private INavigation Navigation;


        public ICommand OpenActivitiesCommand { get; private set; }
        public ICommand ConnectCommand { get; private set; }
        public ICommand FunctionLedboxCommand { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public string aliasText { get { return App.alias; } set { App.alias = value; if (PropertyChanged != null)  PropertyChanged(this, new PropertyChangedEventArgs("aliasText")); } }
        public sport sport { get { return App.sport; } set { App.sport = value; if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs("sport")); PropertyChanged(this, new PropertyChangedEventArgs("TitleApp")); } } }
        

        public string iconConnection { get { return _iconConnection; } set { _iconConnection = value; } }

        public string labelDeviceName { get { return _labelDeviceName; } set { _labelDeviceName = value; } }
        public string labelRole { get { if (_labelRole != "admin") return _labelRole; else return ""; } set { _labelRole = value; } }

        public string labelBtConnection { get { return _labelBtConnection; } set { _labelBtConnection = value; } }

        public string deviceName { get { return App.deviceName; } }
        public bool show_message { get { return _show_message; } set { _show_message = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("show_message")); } }
        public string message_button { get { return _message_button; } set { _message_button = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("message_button")); } }


        public bool ledboxPanelVisible { get { return _labelDeviceName != "" ? true : false; } }

        public bool isTestingMode { get { return App.isTestingMode; } }


        private bool _status_received = false;
        private bool _status_send = false;

        public bool status_received { get { return _status_received; } set { _status_received = value; } }
        public bool status_send { get { return _status_send; } set { _status_send = value; } }


        public StatusBarViewModel()
        {

            OpenActivitiesCommand = new Command(OpenActivities);
            ConnectCommand = new Command(ConnectLedbox);
            FunctionLedboxCommand = new Command(FunctionLedbox);
            MessagingCenter.Subscribe<APILedbox, APILedbox.current_setting>(this, "connected", (sender, arg) =>
            {
                if (App.conn != null && App.conn.isConnected())
                {
                    labelDeviceName = App.deviceName;
                    labelRole = App.role;
                    labelBtConnection = AppResources.disconnect;
                    notifyChanges();
                  
                }

            });

            MessagingCenter.Subscribe<APILedbox>(this, "disconnect", (arg) =>
            {
                labelDeviceName = "";
                labelRole = "";
                App.deviceName = "";
                labelBtConnection = AppResources.connect;
                App.avm.ClearActivities();
                notifyChanges();
                


                //chiudi la schermata di interfaccia (se aperta)
                foreach (Page page in Navigation.NavigationStack)
                {
                    if (page.GetType() == typeof(InterfaceView))
                        page.Navigation.PopAsync();
                }

                App.conn.Dispose();
                //App.conn= null;

            });

            MessagingCenter.Subscribe<APILedbox>(this, "message_received", (arg) =>
            {
                status_received = true;
                PropertyChanged(this, new PropertyChangedEventArgs("status_received"));

                new Thread(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    status_received = false;
                    if(PropertyChanged!=null)
                        PropertyChanged(this, new PropertyChangedEventArgs("status_received"));
                }).Start();
                

            });

            MessagingCenter.Subscribe<APILedbox>(this, "message_send", (arg) =>
            {
                status_send = true;
                PropertyChanged(this, new PropertyChangedEventArgs("status_send"));
                new Thread(() =>
                {
                    System.Threading.Thread.Sleep(1000);
                    status_send = false;
                    PropertyChanged(this, new PropertyChangedEventArgs("status_send"));
                }).Start();

                

            });
        }

        public void setShowMessage(bool status)
        {
            show_message = status;
            if(PropertyChanged!=null)
                PropertyChanged(this, new PropertyChangedEventArgs("show_message"));

        }

        public void setNavigation(INavigation navigation)
        {
            this.Navigation = navigation;
        }

        void ConnectLedbox()
        {

            if (App.conn != null && App.conn.isConnected())
            {
                DisconnectLedbox();
            }
            else
                openSelectConnection((oncomplete) => {
                    if (oncomplete)
                        App.conn.Connect((bool obj) => { changeIcon(); });
                });

        }

        async void FunctionLedbox()
        {

            string[] menu;

            if(App.role=="guest")
                menu=new string[2] { AppResources.configure_ledbox, AppResources.open_layout_parameters };
            else
                menu = new string[3] { AppResources.configure_ledbox, AppResources.open_layout_waiting, AppResources.open_layout_parameters };



            var action = await App.main.DisplayActionSheet(AppResources.settings_ledbox, AppResources.cancel, null,menu );
                          
            if (action == AppResources.configure_ledbox) openPreferences();
            if (action == AppResources.open_layout_waiting) open_layout_waiting();
            if (action == AppResources.open_layout_parameters) open_layout_parameters();

        }

        async void DisconnectLedbox()
        {
            var answer = await App.main.DisplayAlert(AppResources.disconnect, AppResources.sure_disconnect, AppResources.yes, AppResources.no);
            if(answer)
                App.conn.DisconnectToLedbox();
        }


        public void OpenActivities()
        {
            foreach(Page page in Navigation.NavigationStack)
            {
                if (page.GetType() == typeof(ActivityView))
                    return;
            }


            Navigation.PushAsync(new ActivityView());

        }


        public void changeIcon()
        {
            iconConnection = "ic_disconnect.png";
            
            if (App.conn!=null && App.conn.isConnected())
                iconConnection = "ic_connect.png";
            
                
            
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("iconConnection"));



        }


        async void openSelectConnection(Action<bool> onComplete)
        {
            if (App.conn != null)
                App.conn.DisconnectToLedbox();

            if (Device.RuntimePlatform == Device.iOS)
            {
                Preferences.Set("type_connection", "Wi-Fi");
                setConnection();

                onComplete(true);
            }
            else
            {


                string action = await App.main.DisplayActionSheet(AppResources.select_connection, AppResources.cancel, null, "Wi-Fi", "Bluetooth");


                if (action != null && action != AppResources.cancel)
                {
                    switch (action)
                    {
                        case "Wi-Fi":
                            Preferences.Set("type_connection", "Wi-Fi");
                            break;
                        case "Bluetooth":
                            Preferences.Set("type_connection", "Bluetooth");
                            break;

                    }

                    setConnection();

                    onComplete(true);
                }
                else
                {
                    onComplete(false);
                }
            }   
        }

        void setConnection()
        {
            switch (Preferences.Get("type_connection", "Wi-Fi"))
            {
                 case "Wi-Fi":
                    App.conn = new WifiConnection(App.main.Navigation, Preferences.Get("ledbox_ip", ""));
                    break;
                case "Bluetooth":
                    App.conn = new BluetoothConnection(App.main.Navigation, Preferences.Get("ledbox_bluetooth", ""));
                    break;
               
            }

            


        }

        /// <summary>
        /// Apre la finestra delle preferenze del LEDbox
        /// </summary>
        public async void openPreferences()
        {

            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
                return;
            }
            else
            {
                await Navigation.PushAsync(new SettingView());
            }

        }

        

        void open_layout_parameters()
        {
            if (App.conn == null || !App.conn.isConnected())
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
            else
            {
                App.conn.SendMessage(App.api.createShowInfoMessage());
            }
        }
        async void open_layout_waiting()
        {
            if (App.conn == null || !App.conn.isConnected())
                App.DisplayAlert(AppResources.error_connection_to_ledbox);
            else
            {
                App.conn.SendMessage(App.api.createLayoutMessage("waiting"));
            }
        }


        public void notifyChanges()
        {
           
            if (PropertyChanged != null)
            {

                PropertyChanged(this, new PropertyChangedEventArgs("labelDeviceName"));
                PropertyChanged(this, new PropertyChangedEventArgs("labelRole"));
                PropertyChanged(this, new PropertyChangedEventArgs("labelBtConnection"));
                PropertyChanged(this, new PropertyChangedEventArgs("deviceName"));
                PropertyChanged(this, new PropertyChangedEventArgs("ledboxPanelVisible"));
                PropertyChanged(this, new PropertyChangedEventArgs("sport"));

                changeIcon();


            }
        }

    }
}
