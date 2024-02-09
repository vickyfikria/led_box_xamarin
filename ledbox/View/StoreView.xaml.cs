using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ledbox.Resources;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public partial class StoreView : ContentPage
    {

        StoreViewModel svm;

        public StoreView(int id_category)
        {
            InitializeComponent();
            svm = new StoreViewModel(this.Navigation,id_category);
            BindingContext = svm;


           
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            StoreItem storeItem = e.Item as StoreItem;

            await this.Navigation.PushAsync(new StoreItemView(storeItem));
        }


        async void closeWindow()
        {
            await Navigation.PopAsync(false); 
        }
    }
}
