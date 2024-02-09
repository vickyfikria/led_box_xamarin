using System;
using System.Collections.Generic;
using Xamarin.Forms;
using ledbox.Resources;

namespace ledbox
{
    public partial class PracticeItemDetailView : ContentPage
    {

        PracticeItemDetailViewModel pidv;

        /// <summary>
        /// Constructor of window
        /// </summary>
        /// <param name="itemPractice"></param>
        /// <param name="backgroundColor">Background color of window</param>
        /// <param name="title">Window Title</param>
        /// <param name="isNew">if the new record</param>
        public PracticeItemDetailView(ItemPractice itemPractice, Color backgroundColor, string title, bool isNew)
        {



            Device.BeginInvokeOnMainThread(() =>
            {
                BackgroundColor = backgroundColor;
                Title = title;
            });
            pidv = new PracticeItemDetailViewModel(Navigation, itemPractice, isNew);
            InitializeComponent();

            //popolamento testi nei picker
            Rest_time_buzz.Items.Add(AppResources.none);
            Rest_time_buzz.Items.Add("1 " + AppResources.short_beep);
            Rest_time_buzz.Items.Add("2 " + AppResources.short_beeps);
            Rest_time_buzz.Items.Add("3 " + AppResources.short_beeps);
            Rest_time_buzz.Items.Add("1 " + AppResources.long_beep);
            Rest_time_buzz.Items.Add("2 " + AppResources.long_beeps);
            Rest_time_buzz.Items.Add("3 " + AppResources.long_beeps);
            Work_time_buzz.Items.Add(AppResources.none);
            Work_time_buzz.Items.Add("1 " + AppResources.short_beep);
            Work_time_buzz.Items.Add("2 " + AppResources.short_beeps);
            Work_time_buzz.Items.Add("3 " + AppResources.short_beeps);
            Work_time_buzz.Items.Add("1 " + AppResources.long_beep);
            Work_time_buzz.Items.Add("2 " + AppResources.long_beeps);
            Work_time_buzz.Items.Add("3 " + AppResources.long_beeps);

            BindingContext = pidv;





        }

        private async void AddFile(object sender, EventArgs e)
        {

            if (App.isInternetConnection())
            {

                string action = await DisplayActionSheet(AppResources.gallery, AppResources.cancel, null, AppResources.from_local, AppResources.from_online);

                if (action == AppResources.from_local) pidv.AddPhoto();
                //if (action == AppResources.video) pidv.AddVideo();
                if (action == AppResources.from_online) pidv.getPreset();

            }
            else
            {
                pidv.AddPhoto();
            }




        }


        private async void EditFile(object sender, EventArgs e)
        {
            image.Source = await helper.CropImage(pidv.itemPractice.filepath,2,1);
            pidv.itemPractice.is_changed = true;
        }

        

    }
}
