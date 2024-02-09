using System;
using System.Collections.Generic;
using System.ComponentModel;
using MvvmHelpers;
using Newtonsoft.Json;
using Xamarin.Forms;
using ledbox.Resources;
using System.Windows.Input;

namespace ledbox
{
    public class Activity: ObservableObject, INotifyPropertyChanged, ICloneable
    {

        public const int STATUS_PLAY = 1;
        public const int STATUS_STOP = 0;
        public const int STATUS_PAUSE = 2;



        //public const string TOPLAY = "in esecuzione";
        //public const string TOPAUSE = "in pausa";

        public const int TYPE_PLAYLIST_IMAGE=0;
        public const int TYPE_PLAYLIST_AUDIO = 1;
        public const int TYPE_PRACTICE = 2;
        public const int TYPE_CUSTOM_TEXT = 3;


        public event PropertyChangedEventHandler PropertyChanged;

        public string title {get; set;}
        public string hashname { get; set; }

        public int type { get; set; }
        public int status { get; set; }

        public ICommand StopCommand { get; private set; }
        public ICommand PlayPauseCommand { get; private set; }


        public Activity()
        {
            PlayPauseCommand = new Command(PlayPause);
            StopCommand = new Command(Stop);
        }


        public string Type { get
            {
                switch (type)
                {
                    case TYPE_PLAYLIST_IMAGE:
                        return AppResources.playlist + " " + AppResources.image;
                    case TYPE_PLAYLIST_AUDIO:
                        return AppResources.playlist + " " + AppResources.audio;
                    case TYPE_PRACTICE:
                        return AppResources.practice;
                    case TYPE_CUSTOM_TEXT:
                        return AppResources.custom_text;

                }
                return "";
            }
        }

        public void Stop()
        {
            switch (type)
            {
                case TYPE_PLAYLIST_IMAGE:
                    App.conn.SendMessage(App.api.createStopPlaylistImageMessage(hashname));
                    break;
                case TYPE_PLAYLIST_AUDIO:
                    App.conn.SendMessage(App.api.createStopPlaylistAudioMessage(hashname));
                    break;
                case TYPE_PRACTICE:
                    App.conn.SendMessage(App.api.createStopPracticeMessage(hashname));
                    break;
                case TYPE_CUSTOM_TEXT:
                    App.conn.SendMessage(App.api.createStopCustomTextMessage(hashname));
                    break;
            }
        }

        public void PlayPause()
        {
            switch (type)
            {
                case TYPE_PLAYLIST_IMAGE:
                    if(status==STATUS_PLAY)
                        App.conn.SendMessage(App.api.createPausePlaylistImageMessage(hashname));
                    if (status == STATUS_PAUSE)
                        App.conn.SendMessage(App.api.createStartPlaylistImageMessage(hashname));

                    break;
                case TYPE_PLAYLIST_AUDIO:
                    if (status == STATUS_PLAY)
                        App.conn.SendMessage(App.api.createPausePlaylistAudioMessage(hashname));
                    if (status == STATUS_PAUSE)
                        App.conn.SendMessage(App.api.createStartPlaylistAudioMessage(hashname));

                    break;
                case TYPE_PRACTICE:
                    if (status == STATUS_PLAY)
                        App.conn.SendMessage(App.api.createPausePracticeMessage(hashname));
                    if (status == STATUS_PAUSE)
                        App.conn.SendMessage(App.api.createStartPracticeMessage(hashname,title));

                    break;

                case TYPE_CUSTOM_TEXT:
                    if (status == STATUS_PLAY)
                        App.conn.SendMessage(App.api.createPauseCustomTextMessage());
                    if (status == STATUS_PAUSE)
                        App.conn.SendMessage(App.api.createStartCustomTextMessage());

                    break;
            }

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("iconStatus"));
            }


        public string iconStatus
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return "ic_pause_white_24dp";
                    case STATUS_PAUSE:
                        return "ic_play_arrow_white_24dp";
                    

                }

                return "ic_play_arrow_white_24dp";
            }
        }


        public object Clone()
        {
            return this.MemberwiseClone();
           
        }
    }
}
