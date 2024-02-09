using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ledbox
{
    public partial class TimerPlaylistView : ContentPage
    {
        Playlist playlist;

        public TimerPlaylistView(Playlist pl)
        {
            playlist = pl;
            this.Title = pl.Title;
            InitializeComponent();
            int counter_tmp = pl.Counter_Duration;
            input_min.Value = playlist.Counter_Duration_min+1;
            input_sec.Value = playlist.Counter_Duration_sec+1;
            
            playlist.Counter_Duration = counter_tmp;
            lbl_input.Text = playlist.Counter_DurationZero;


        }

        void Bt_Skip_Clicked(object sender, System.EventArgs e)
        {
            //playlist.Counter_Enable = false;
            playlist.Counter_Duration = 0;

            playlist.sendMessageToPlay();
            Navigation.PopModalAsync();


        }

        void Bt_OK_Clicked(object sender, System.EventArgs e)
        {

            //playlist.Counter_Enable = true;
            playlist.Counter_Duration = (int)input_sec.Value*60;
            
            playlist.sendMessageToPlay();
            Navigation.PopModalAsync();


        }

        void Bt_Cancel_Clicked(object sender, System.EventArgs e)
        {

            Navigation.PopModalAsync();


        }

        void TimeStepperSec_ValueChanged(object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            int delta = (int)(e.NewValue - e.OldValue);

            playlist.Counter_Duration= playlist.Counter_Duration +  delta;
            lbl_input.Text = playlist.Counter_DurationZero;
                
        }

        void TimeStepperMin_ValueChanged(object sender, Xamarin.Forms.ValueChangedEventArgs e)
        {
            int delta = (int)(e.NewValue - e.OldValue);

            playlist.Counter_Duration = playlist.Counter_Duration + delta*60;
            lbl_input.Text = playlist.Counter_DurationZero;
        }

        void TimeStepperMin10_ValueChanged(object sender, EventArgs e)
        {

            int delta = 5;

            playlist.Counter_Duration = playlist.Counter_Duration + delta*60;
            lbl_input.Text = playlist.Counter_DurationZero;
            input_min.Value = input_min.Value + delta;

        }
        void TimeStepperSec10_ValueChanged(object sender, EventArgs e)
        {
            int delta = 5;

            playlist.Counter_Duration = playlist.Counter_Duration + delta;
            lbl_input.Text = playlist.Counter_DurationZero;
            input_sec.Value = input_sec.Value + delta;
            
        }

        void TimeStepperZero_ValueChanged(object sender, EventArgs e)
        {

            input_min.Value = 0;
            input_sec.Value = 0;
            playlist.Counter_Duration = 0;
            lbl_input.Text = playlist.Counter_DurationZero;
        }
    }
}
