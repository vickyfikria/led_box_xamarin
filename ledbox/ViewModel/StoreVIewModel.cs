using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using ledbox.Resources;
using Acr.UserDialogs;

namespace ledbox
{
    public class StoreViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private INavigation Navigation;
        public ObservableCollection<StoreItem> OStoreItem { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ReloadListCommand { get; private set; }
        public int id_category = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public StoreViewModel(INavigation navigation,int id_category)
        {
            this.id_category = id_category;
            reloadList();
            this.Navigation = navigation;
            ReloadListCommand = new Command(reloadList);
            
        }


        public async void reloadList()
        {
            var loading = UserDialogs.Instance.Loading(null,()=> { },AppResources.cancel);
            
            OStoreItem = new ObservableCollection<StoreItem>();

            List<StoreItem> store_Files = await App.webservice.GetStore(App.sport.name,this.id_category);

            if (store_Files == null)
            {
                DependencyService.Get<IMessage>().ShortAlert(AppResources.error_connection);
                loading.Hide();
                return;
            }


            if(store_Files.Count>0)
                foreach (StoreItem storeItem in store_Files)
                {
                    OStoreItem.Add(storeItem);
                }

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OStoreItem"));

            loading.Hide();
        }

        




    }
}
