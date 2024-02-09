using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using ledbox.ViewModels;
using Acr.UserDialogs;
using ledbox.Resources;



namespace ledbox
{
    public partial class PlaylistView : ContentPage
    {

        PlaylistViewModel pvm;
        private string openlastlayout;

        public PlaylistView(string openlastlayout="waiting")
        {

            this.openlastlayout = openlastlayout;
            InitializeComponent();
            pvm = new PlaylistViewModel(this.Navigation);
            BindingContext = pvm;

        }

        private async void AddClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(AppResources.create_playlist, AppResources.cancel, null, AppResources.image, AppResources.audio);

            Playlist p = new Playlist();

            if(action== AppResources.image) p.type = Playlist.TYPE_IMAGE;
            if (action == AppResources.audio) p.type = Playlist.TYPE_AUDIO;
            if (action == AppResources.cancel) return;



            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
            });

            if(pResult.Ok &&  !string.IsNullOrWhiteSpace(pResult.Text))
            {
                p.Title = pResult.Text;
                App.storage.addPlaylist(p);

                await openPlaylistItemView(p);
            }


        }


        private async void EditClicked(object sender, EventArgs e)
        {
            var button = sender as MenuItem;
            Playlist playlist = button.BindingContext as Playlist;

            await openPlaylistItemView(playlist);

        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

            Playlist playlist = e.Item as Playlist;


            await openPlaylistItemView(playlist);
          

        }

        async System.Threading.Tasks.Task openPlaylistItemView(Playlist playlist)
        {
            PlaylistItemView piv = new PlaylistItemView(playlist,false,openlastlayout);

            piv.Disappearing += (object sender, EventArgs e) => {
                piv.pim.DoneEditing();
                pvm.reloadList();
                pvm.NotifyChange();
            };

            await this.Navigation.PushAsync(piv);
        }

        private void DeletePlaylistClicked(object sender, EventArgs e)
        {
            var button = sender as MenuItem;
            var file = button.BindingContext as Playlist;
            var vm = BindingContext as PlaylistViewModel;
            vm.RemoveCommand(file);
            pvm.NotifyChange();


        }

    }
}
