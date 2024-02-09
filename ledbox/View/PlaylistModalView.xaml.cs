using System;
using System.Collections.Generic;
using ledbox.ViewModels;
using Xamarin.Forms;

namespace ledbox
{
    public partial class PlaylistModalView : ContentPage
    {
        
        PlaylistViewModel pvm;

        public PlaylistModalView()
        {
            InitializeComponent();
            pvm = new PlaylistViewModel(this.Navigation);
            BindingContext = pvm;
        }

        void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

            Playlist playlist = e.Item as Playlist;

            if (playlist == null)
                return;
            playlist.onfinish = "";
            //this.Navigation.PopModalAsync(false);
            openTimerPlaylist(playlist);

            
/*
            if (playlist.Status)
                playlist.sendMessageToStop();
            else
                playlist.sendMessageToPlay();*/
            
            

        }

        private  void openTimerPlaylist(Playlist playlist)
        {
            TimerPlaylistView tpv = new TimerPlaylistView(playlist);

            tpv.Disappearing += (object sender, EventArgs e) =>
            {
                Navigation.PopModalAsync(false);
                Navigation.PopAsync(false);
                
            };

            Navigation.PushAsync(tpv);

        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            this.Navigation.PopModalAsync();
        }
    }
}
