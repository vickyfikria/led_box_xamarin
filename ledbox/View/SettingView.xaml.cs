using System;
using System.Collections.Generic;
using System.ComponentModel;
using ledbox.Resources;
using Xamarin.Forms;
using Acr.UserDialogs;
using Xamarin.Essentials;

namespace ledbox
{
    public partial class SettingView : ContentPage
    {

        APILedbox.config[] current_configs;


        public SettingView()
        {
            InitializeComponent();
            IProgressDialog loading;

            loading = UserDialogs.Instance.Loading(null,()=>{},AppResources.cancel);


            serialnumber.Text = App.deviceName;
            version.Text = App.current_ledbox_version.ToString();

            if (Preferences.Get("last_update_ledbox_version", 0f) > 0)
            {
                btn_update.IsVisible = true;
                btn_update.Text = AppResources.update_to + " " + Preferences.Get("last_update_ledbox_version", 0f).ToString();
            }
            else
                btn_update.IsVisible = false;

            MessagingCenter.Subscribe<APILedbox, APILedbox.config[]>(this, "configs_getted", ((obj, configs) =>
                 {
                    current_configs = configs;
                    try
                    {
                         Device.BeginInvokeOnMainThread(() =>
                         {


                             ip_lan.Text = getConfigLedbox(configs, "NETWORK", "current_lan_ip", App.deviceName);
                             ip_wifi.Text = getConfigLedbox(configs, "NETWORK", "current_wifi_ip", App.deviceName);


                             switch (getConfigLedbox(configs, "WIFI", "mode", App.deviceName))
                             {
                                 case "ap":
                                     wifi_mode.SelectedIndex = 0;
                                     wifi_ssid_panel.IsVisible = false;
                                     wifi_psk_panel.IsVisible = false;
                                     break;
                                 case "client":
                                     wifi_mode.SelectedIndex = 1;
                                     wifi_ssid_panel.IsVisible = true;
                                     wifi_psk_panel.IsVisible = true;
                                     break;
                             }


                             wifi_ssid.Text = getConfigLedbox(configs, "WIFI", "ssid", App.deviceName).Replace("'", "");
                             wifi_psk.Text = getConfigLedbox(configs, "WIFI", "psk", App.deviceName).Replace("'", "");


                             switch (getConfigLedbox(configs, "NETWORK", "mode", App.deviceName))
                             {
                                 case "dhcp":
                                     network_mode.SelectedIndex = 0;
                                     network_ip_static_panel.IsVisible = false;
                                     network_subnet_static_panel.IsVisible = false;
                                     network_gateway_static_panel.IsVisible = false;
                                     btn_restart_dhcp.IsVisible = true;
                                     break;
                                 case "static":
                                     network_mode.SelectedIndex = 1;
                                     network_ip_static_panel.IsVisible = true;
                                     network_subnet_static_panel.IsVisible = true;
                                     network_gateway_static_panel.IsVisible = true;
                                     btn_restart_dhcp.IsVisible = false;
                                     break;
                             }


                             network_ip.Text = getConfigLedbox(configs, "NETWORK", "ip", App.deviceName).Replace("'", "");
                             network_subnet.Text = getConfigLedbox(configs, "NETWORK", "subnet", App.deviceName).Replace("'", "");
                             network_gateway.Text = getConfigLedbox(configs, "NETWORK", "gateway", App.deviceName).Replace("'", "");


                             switch (getConfigLedbox(configs, "GENERAL", "mode", App.deviceName))
                             {
                                 case "master":
                                     mode_execute.SelectedIndex = 0;
                                     break;
                                 case "slave":
                                     mode_execute.SelectedIndex = 1;
                                     break;

                             }

                             ip_master.Text = getConfigLedbox(configs, "GENERAL", "ip_master", App.deviceName).Replace("'", "");



                             switch (getConfigLedbox(configs, "LAYOUT", "modifier", App.deviceName))
                             {
                                 case "":
                                     modifier.SelectedIndex = 0;
                                     break;
                                 case "specular":
                                     modifier.SelectedIndex = 1;
                                     break;

                             }
                             loading.Hide();
                             panel.IsEnabled = true;
                         });
                         MessagingCenter.Unsubscribe<APILedbox, APILedbox.config[]>(this, "configs_getted");
                     }
                     catch (Exception e)
                     {
                         Console.WriteLine("ERROR Setting Config " + e.ToString());
                     }
                 }));

            App.conn.SendMessage(App.api.createGetConfigsMessage());
            System.Threading.Thread.Sleep(200);
            App.conn.SendMessage(App.api.createGetClientsMessage());
            

        }    

               
        string getConfigLedbox(APILedbox.config[] configs, string section, string field,string device)
        {
            
            //trova il settaggio
            foreach(APILedbox.config c in  configs)
            {
                if (c.device == device) 
                    if (c.section == section && c.field == field)
                    {
                        return c.value;

                    }
            }

            return "";

        }

        private async void DeletePlaylistImage(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel, AppResources.yes);
            if(answer==AppResources.yes)
                App.conn.SendMessage(App.api.createDeleteAllPlaylistImageMessage(App.alias));
        }

        private async void DeletePlaylistAudio(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel,  AppResources.yes);
            if (answer == AppResources.yes)
                App.conn.SendMessage(App.api.createDeleteAllPlaylistAudioMessage(App.alias));
        }

        private async void DeleteMatchscore(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel, AppResources.yes);
            if (answer == AppResources.yes)
                App.conn.SendMessage(App.api.createDeleteAllIntefacesMessage());
        }

        private async void DeletePractice(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel,  AppResources.yes);
            if (answer == AppResources.yes)
                App.conn.SendMessage(App.api.createDeleteAllPracticeMessage(App.alias));
        }

        private async void DeleteMedia(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel, AppResources.yes);
            if (answer == AppResources.yes)
                App.conn.SendMessage(App.api.createDeleteAllMedia(App.alias));
        }
        
        private async void restartLedbox(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel, AppResources.yes);
            if (answer == AppResources.yes)
            {
                App.conn.SendMessage(App.api.createRebootMessage());
                App.conn.DisconnectToLedbox();
                await Navigation.PopAsync();
                DependencyService.Get<IMessage>().ShortAlert(AppResources.rebooting);
            }            

        }

        private async void restartDHCP(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                DependencyService.Get<IMessage>().ShortAlert(AppResources.start_dhcp_restart);
            });

            MessagingCenter.Subscribe<APILedbox, bool>(this, "DHCP_restarted", (s,status) =>
             {
                 Device.BeginInvokeOnMainThread(() =>
                 {
                     DependencyService.Get<IMessage>().ShortAlert(AppResources.dhcp_restarted);
                 });
                 
             });
            App.conn.SendMessage(App.api.createRestartDHCPMessage());
        }

        private async void updateLedbox(object sender, EventArgs e)
        {
            var answer = await DisplayActionSheet(AppResources.are_you_sure, AppResources.cancel, AppResources.yes);
            if (answer == AppResources.yes)
            {
                App.RunUpdateLedbox();
                await Navigation.PopAsync();
            }            

        }

        void mode_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (wifi_mode.SelectedIndex)
            {
                case 0:
                    wifi_ssid_panel.IsVisible = false;
                    wifi_psk_panel.IsVisible = false;
                    break;
                case 1:
                    wifi_ssid_panel.IsVisible = true;
                    wifi_psk_panel.IsVisible = true;
                    break;

            }
        }

        void mode_network_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (network_mode.SelectedIndex)
            {
                case 0:
                    network_ip_static_panel.IsVisible = false;
                    network_subnet_static_panel.IsVisible = false;
                    network_gateway_static_panel.IsVisible = false;
                    break;
                case 1:
                    network_ip_static_panel.IsVisible = true;
                    network_subnet_static_panel.IsVisible = true;
                    network_gateway_static_panel.IsVisible = true;
                    break;

            }
        }


        void mode_execute_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            switch (mode_execute.SelectedIndex)
            {
                case 0:
                    ip_master_panel.IsVisible = false;
                    break;
                case 1:
                    ip_master_panel.IsVisible = true;
                    break;

            }
        }

        async void Bt_Save_Clicked(object sender, System.EventArgs e)
        {

            if(App.conn==null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.error_connection);
                return;
            }

            if (panel.IsEnabled == false)
                return;

            if (mode_execute.SelectedIndex == 1)
            {
                ip_master.Text = helper.validateIPAddress(ip_master.Text);
                if (ip_master.Text == "")
                {
                    App.DisplayAlert(AppResources.master_ip_not_valid);
                    return;
                }
            }

            if (network_mode.SelectedIndex == 1)
            {
                network_ip.Text = helper.validateIPAddress(network_ip.Text);
                if (network_ip.Text == "")
                {
                    App.DisplayAlert(AppResources.network_ip_not_valid);
                    return;
                }

                network_subnet.Text = helper.validateIPAddress(network_subnet.Text);
                if (network_subnet.Text == "")
                {
                    App.DisplayAlert(AppResources.network_subnet_not_valid);
                    return;
                }

                network_gateway.Text = helper.validateIPAddress(network_gateway.Text);
                if (network_gateway.Text == "")
                {
                    App.DisplayAlert(AppResources.network_gateway_not_valid);
                    return;
                }
            }

            APILedbox.config[] configs = new APILedbox.config[10];

            configs[0] = new APILedbox.config() { section = "WIFI", field = "mode", value = wifi_mode.SelectedIndex==0 ? "ap":"client", device=App.deviceName };
            configs[1] = new APILedbox.config() { section = "WIFI", field = "ssid", value = "'"+wifi_ssid.Text+"'", device = App.deviceName };
            configs[2] = new APILedbox.config() { section = "WIFI", field = "psk", value = "'"+wifi_psk.Text+"'", device = App.deviceName };
            configs[3] = new APILedbox.config() { section = "LAYOUT", field = "modifier", value = modifier.SelectedIndex == 0 ? "" : "specular", device = App.deviceName };
            configs[4] = new APILedbox.config() { section = "GENERAL", field = "mode", value = mode_execute.SelectedIndex == 0 ? "master" : "slave", device = App.deviceName };
            configs[5] = new APILedbox.config() { section = "GENERAL", field = "ip_master", value = ip_master.Text, device = App.deviceName };

            configs[6] = new APILedbox.config() { section = "NETWORK", field = "mode", value = network_mode.SelectedIndex == 0 ? "dhcp" : "static", device = App.deviceName };
            configs[7] = new APILedbox.config() { section = "NETWORK", field = "ip", value = network_ip.Text, device = App.deviceName };
            configs[8] = new APILedbox.config() { section = "NETWORK", field = "subnet", value = network_subnet.Text, device = App.deviceName };
            configs[9] = new APILedbox.config() { section = "NETWORK", field = "gateway", value = network_gateway.Text, device = App.deviceName };



            App.conn.SendMessage(App.api.createSetConfigsMessage(configs));

            DependencyService.Get<IMessage>().ShortAlert(AppResources.config_saved);


            var answer = await App.main.DisplayAlert(AppResources.reboot, AppResources.reboot_app, AppResources.yes, AppResources.no);

            if (answer)
            {
                App.conn.SendMessage(App.api.createRebootMessage());
                App.conn.DisconnectToLedbox();
            }

            Navigation.PopAsync();
        }
    }
}
