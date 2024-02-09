using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ledbox
{
    public partial class UploadView : ContentPage
    {
        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync(false);
        }

        public UploadView()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<UploadView, int[]>(this, "upload", ((sender,value)=> {
                progressbar.Progress=value[0]/100;
               
                
            }));

           /* MessagingCenter.Subscribe<UploadView>(this, "hide_upload", ((obj) =>
            {

                Navigation.PopModalAsync(false);
                App.isUploadedOpen = false;

            }));*/

        }
    }
}
