using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using ledbox.ViewModels;
using Acr.UserDialogs;
using ledbox.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ledbox
{
    public partial class PracticeLedboxView: ContentPage
    {

        PracticeLedboxViewModel plvm;

        Color backgroundColor;

     
        public PracticeLedboxView(string title, Color backgroundColor, int id_category)
        {

            this.Appearing += (s, obj) =>
            {
                if(App.conn!=null)
                    plvm.reloadList();
            };

            

            this.backgroundColor = backgroundColor;

            Device.BeginInvokeOnMainThread(() =>
            {
                Title = title;
                lstView.BackgroundColor = backgroundColor;
            });
            InitializeComponent();

            plvm = new PracticeLedboxViewModel(this.Navigation, id_category);

            BindingContext = plvm;
       
        }


        private async void reloadList(object sender, EventArgs e) 
        {

            plvm.reloadList();
        }



        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            Practice practice = e.Item as Practice;

            

        }

     
      



    }
}
