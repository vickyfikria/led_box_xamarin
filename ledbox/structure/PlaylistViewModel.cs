using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace ledbox.ViewModels
{
    public class PlaylistViewModel : BaseViewModel,INotifyPropertyChanged
    {
        private INavigation Navigation;
        public ObservableCollection<Playlist> OPlaylist{ get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public PlaylistViewModel(INavigation navigation)
        {

         

            reloadList();

            this.Navigation = navigation;


            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_start", ((sender, playlistname) =>
             {
                 foreach (Playlist p in OPlaylist)
                 {
                     if (p.Title == playlistname)
                     {
                         p.Status = Playlist.STATUS_PLAY;

                        
                     }
                 }
             })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_pause", ((sender, playlistname) =>
            {
                foreach (Playlist p in OPlaylist)
                {
                    if (p.Title == playlistname)
                    {
                        p.Status = Playlist.STATUS_PAUSE;


                    }
                }
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_stop", ((sender, playlistname) =>
            {
                foreach (Playlist p in OPlaylist)
                {
                        p.Status = Playlist.STATUS_STOP;
                }
            })
            );


        }


        public void reloadList()
        {
            OPlaylist = new ObservableCollection<Playlist>();

            if (App.storage.current_project.playlists != null)
                foreach (Playlist item in App.storage.current_project.playlists)
                {
                    OPlaylist.Add(item);
                }
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OPlaylist"));

        }

        public void RemoveCommand(Playlist p)
        {
            App.storage.current_project.playlists.Remove(p);
            OPlaylist.Remove(p);
            App.storage.saveFile();
        
        }


    }
}
