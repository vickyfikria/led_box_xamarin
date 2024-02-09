using Acr.UserDialogs;
using ledbox.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ledbox
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CreditsView : ContentPage
    {

       
        int count_enable_testing = 0;

        public CreditsView()
        {
            InitializeComponent();

          

            string ver = Xamarin.Essentials.AppInfo.VersionString;
            this.AppVersion.Text = ver;

            //string AppBuild = Xamarin.Essentials.AppInfo.VersionString;
            //this.AppBuild.Text = AppBuild;

        }


        public async void EnableTestingMode(object sender, EventArgs e)
        {
            count_enable_testing++;
            if (count_enable_testing == 9)
            {
                if (App.isTestingMode)
                {

                    


                    Preferences.Set("testingMode", false);
                    App.DisplayAlert("Now you are in Normal Mode. Reboot App");
                }
                else
                {

                    PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
                    {
                        InputType = InputType.Name,
                        OkText = AppResources.ok,
                        CancelText = AppResources.cancel,
                        Title = "Password",
                    });

                    if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
                    {

                        if (pResult.Text == App.passwordTestingMode)
                        {

                            Preferences.Set("testingMode", true);
                            App.DisplayAlert("Now you are in Testing Mode. Reboot App");
                        }
                    }



                    
                }

              

            }
        }


        private void GoToSite(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri("http://www.tech4sport.com"));

        }



    }



}