using System;
using System.Collections.Generic;
using ledbox.Resources;
using Xamarin.Forms;

namespace ledbox
{
    public partial class StoreItemView : ContentPage
    {
        public StoreItem storeItem;
        public StoreItemViewModel svm;

        public string path_manifest_downloaded="";

        public StoreItemView(StoreItem storeItem)
        {
            
            InitializeComponent();
            this.storeItem = storeItem;
            svm = new StoreItemViewModel(base.Navigation, storeItem);
            base.BindingContext = svm;

        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var answer = await DisplayAlert(AppResources.installation, AppResources.confirm_installation, AppResources.yes, AppResources.no);
            if (answer)
            {

                if (storeItem.category == "interface")
                {
                    //verifica se è presente una versione più vecchia
                    var old_version = storeItem.verify_old_version(App.interfaceFiles);

                    if (old_version.Item2)
                    {
                        var answer2 = await DisplayAlert(AppResources.installation, AppResources.interface_yet_present, AppResources.yes, AppResources.no);
                        if (answer2)
                            //disistalla l'interfaccia
                            old_version.Item1.disinstall();

                    }
                }

                storeItem.download_and_install((onFinish) =>
                {
                    if (onFinish == "")
                    {
                        DependencyService.Get<IMessage>().ShortAlert(AppResources.error_download);
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(AppResources.installation_complete);
                        //MessagingCenter.Send<webservice, string>(App.webservice, "storeItemDownloaded", onFinish);
                    }
                    closeAllWindow();
                });
            }
        }

        /// <summary>
        /// Chiude la finestra corrente
        /// </summary>
        async void closeWindow()
        {
            await Navigation.PopAsync(false);
        }

        /// <summary>
        /// Chiude tutte le finestre relative allo store (sia Item che List)
        /// </summary>
        async void closeAllWindow()
        {


            StoreView storeView=null;
            foreach (Page page in Navigation.NavigationStack)
            {

                if (page.GetType() == typeof(StoreView))
                    storeView = (StoreView)page;

            }

            await Navigation.PopAsync(false);
            if(storeView!=null)
                await storeView.Navigation.PopAsync(false);
            
        }

    }
}
