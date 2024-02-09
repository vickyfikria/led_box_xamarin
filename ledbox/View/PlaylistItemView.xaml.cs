using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using ledbox.Resources;
using Acr.UserDialogs;

namespace ledbox
{
    public partial class PlaylistItemView : ContentPage
    {


        public Playlist playlist;
        public PlaylistItemViewModel pim;
        ObservableCollection<PlaylistItemViewModel> items = new ObservableCollection<PlaylistItemViewModel>();
       
        private string openlastlayout;

        public PlaylistItemView(Playlist playlist,bool isNew=false,string openlastlayout="")
        {

            this.openlastlayout = openlastlayout;

            InitializeComponent();

            if (playlist.type == Playlist.TYPE_AUDIO)
                this.lstItems.RowHeight = 80;
            else
            {
                this.lstItems.RowHeight = 130;
            }

            pim = new PlaylistItemViewModel(base.Navigation, playlist,isNew,openlastlayout);
            this.BindingContext = pim;
            this.playlist = playlist;

            if (this.playlist.isremote)
            {
                this.ToolbarItems.Clear();
            }

        }
        

        async void lstItems_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            
            FilePlaylist p = (FilePlaylist)e.Item;

            string action;
            if (playlist.type==Playlist.TYPE_AUDIO)
                action = await DisplayActionSheet(AppResources.action, AppResources.cancel,"", AppResources.move_up, AppResources.move_down, AppResources.delete);
            else
                action = await DisplayActionSheet(AppResources.action, AppResources.cancel, "", AppResources.move_up, AppResources.move_down, AppResources.delete);
                //TODO disattivazione crop image
                //action = await DisplayActionSheet(AppResources.action, AppResources.cancel, "", AppResources.edit, AppResources.move_up, AppResources.move_down, AppResources.delete);

            if (action == AppResources.edit) pim.editPhoto(p);
            if (action == AppResources.delete) DeleteFileClicked(p);
            if (action == AppResources.move_up) MoveUp_Clicked(p);
            if (action == AppResources.move_down) MoveDown_Clicked(p);


            if (action == AppResources.preview)
            {
               
                //carica sul ledbox
                if (App.conn != null && App.conn.isConnected())
                    uploadFile(p);
                else
                    await Navigation.PushModalAsync(new ConnectionView((isconnected) =>
                    {
                        if (isconnected)
                            uploadFile(p);

                    }));
            }
        }


        void uploadFile(FilePlaylist p)
        {
            p.sendMessageShowPreview();

        }


        private void DeleteFileClicked(FilePlaylist filePlaylist)
        {
            
            var vm = BindingContext as PlaylistItemViewModel;
            vm.RemoveCommand(filePlaylist);

            playlist.setLastModified();


        }

        private void AddFile(object sender, EventArgs e)
        {
            if (playlist.type == Playlist.TYPE_AUDIO)
                pim.AddAudio();
                //PlaylistItemViewModel.AddAudio(pim);
            else
            {
                pim.AddPhoto();



                /*
                //TODO DA ABILITARE I VIDEO
                string action = await DisplayActionSheet("Galleria", AppResources.cancel, null, AppResources.image, AppResources.video);

                if(action== AppResources.image) pim.AddPhoto();
                if (action == AppResources.video) pim.AddVideo();*/
                

            }
        }

        void MoveUp_Clicked(FilePlaylist filePlaylist)
        {
            
            var vm = BindingContext as PlaylistItemViewModel;
            vm.movePlaylist(filePlaylist, -1);

        }

        void MoveDown_Clicked(FilePlaylist filePlaylist)
        {
            
            var vm = BindingContext as PlaylistItemViewModel;
            vm.movePlaylist(filePlaylist, 1);

        }

        async void ChangeTitle_Clicked(object sender, System.EventArgs e)
        {

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
                Text = pim.Playlist.Title
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                pim.Playlist.Title = pResult.Text;
                this.Title = pim.Playlist.Title;

                playlist.setLastModified();
            }




        }

    }
}
