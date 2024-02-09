using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading;

namespace ledbox.ViewModels
{
    public class PlaylistViewModel : BaseViewModel,INotifyPropertyChanged
    {
        private INavigation Navigation;
        public ObservableCollection<Playlist> OPlaylist{ get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        public bool isEmpty
        {
            get
            {
                if (OPlaylist.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public PlaylistViewModel(INavigation navigation)
        {

         

            reloadList();

            this.Navigation = navigation;

            MessagingCenter.Subscribe<APILedbox, APILedbox.current_setting>(App.api, "connected", ((sender, setting) =>
            {
                reloadRemoteList();
            })
           );

            MessagingCenter.Subscribe<APILedbox>(this, "disconnect", (arg) =>
            {
                resetRemotList();
                NotifyChange();

            });


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
                    if(p.Title==playlistname)
                        p.Status = Playlist.STATUS_STOP;
                }
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "stop_all", ((sender, playlistname) =>
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


            reloadRemoteList();

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OPlaylist"));

        }




        /// <summary>
        /// Azzera la lista delle practice da quelle già presenti sul LEDbox
        /// </summary>
        /// <param name="type_playlist">Se non specificato elimina tutte le playlist</param>
        void resetRemotList(int type_playlist=-1)
        {
            //elimina tutti le practice remote già presenti
            for (int i = 0; i < OPlaylist.Count; i++)
                if (OPlaylist[i].isremote)
                    if(OPlaylist[i].type == type_playlist || type_playlist==-1)
                        OPlaylist.Remove(OPlaylist[i]);
        }

        /// <summary>
        /// Verifica se una playlist è presente nella ListView
        /// </summary>
        /// <param name="playlisthashname"></param>
        /// <returns></returns>
        bool isPlaylistInList(string playlisthashname,int type)
        {
            foreach (Playlist playlist in OPlaylist)
                if (playlist.hashname == playlisthashname && playlist.type==type)
                    return true;

            return false;
        }

        public void RemoveCommand(Playlist p)
        {
            App.storage.current_project.playlists.Remove(p);
            OPlaylist.Remove(p);
            App.storage.saveFile();
        
        }


        /// <summary>
        /// Popola la lista delle practice già presenti sul LEDbox nella ListView
        /// </summary>
        public void reloadRemoteList()
        {

            resetRemotList();



            MessagingCenter.Subscribe<APILedbox, (List<Playlist>,int)>(App.api, "playlist_getlist", (api, valuecombined) => {

                List<Playlist> value = valuecombined.Item1;
                int type_playlist = valuecombined.Item2;

                resetRemotList(type_playlist);

                foreach (Playlist item in value)
                {
                    if (!isPlaylistInList(item.hashname,item.type))
                    {
                        item.isremote = true;
                        OPlaylist.Add(item);
                    }
                }

                NotifyChange();

            });


            if (App.conn != null && App.conn.isConnected())
            {
                App.conn.SendMessage(App.api.createGetListPlaylistImageMessage());
                Thread.Sleep(500);
                App.conn.SendMessage(App.api.createGetListPlaylistAudioMessage());
            }
        }


        /// <summary>
        /// Notifica le modifiche alla contentPage
        /// </summary>
        public void NotifyChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OPlaylist"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }

        }


    }
}
