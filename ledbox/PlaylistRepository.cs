using System;
using System.Collections.ObjectModel;

namespace ledbox
{
    public class PlaylistRepository
    {
        private ObservableCollection<Playlist> playlist=new ObservableCollection<Playlist>();

        public ObservableCollection<Playlist> Playlist
        {
            get { return playlist; }
            set { this.playlist = value; }
        }


        public PlaylistRepository()
        {

            if (App.storage.playlists != null)
                foreach (Playlist item in App.storage.playlists)
                {
                    playlist.Add(item);

                }

        }
    }
}
