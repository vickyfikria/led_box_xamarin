using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using ledbox.ViewModel;
using Acr.UserDialogs;
using ledbox.Resources;



namespace ledbox
{
    public partial class ActivityView : ContentPage
    {

        
        public ActivityView()
        {

            InitializeComponent();
            BindingContext = App.avm;

        }


        void Stop_clicked(object sender, System.EventArgs e)
        {
            
            App.conn.SendMessage(App.api.createStopAllProcessMessage());
            
        }
     
    }
}
