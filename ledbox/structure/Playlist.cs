using System;
using System.Collections.Generic;
using System.ComponentModel;
using MvvmHelpers;
using Newtonsoft.Json;
using Xamarin.Forms;
using ledbox.Resources;
using System.Threading;
using Acr.UserDialogs;
namespace ledbox
{
    public class Playlist : ObservableObject, INotifyPropertyChanged, ICloneable, IActivity
    {

        public const int STATUS_PLAY = 1;
        public const int STATUS_STOP = 0;
        public const int STATUS_PAUSE = 2;
        public const int STATUS_WAITING = 3;
        public const int TYPE_IMAGE = 0;
        public const int TYPE_VIDEO = 1;
        public const int TYPE_AUDIO = 2;


        [JsonIgnore]
        public string TYPE_IMAGE_LABEL = AppResources.image;
        [JsonIgnore]
        public string TYPE_VIDEO_LABEL = AppResources.video;
        [JsonIgnore]
        public string TYPE_AUDIO_LABEL = AppResources.audio;



        public int id = 0;
        public string hashname = "";
        public string title { get; set; }
        public int type { get; set; }
        public int max_counter_time { get; set; }
        public string onfinish = "";
        public List<FilePlaylist> items;
        public DateTime last_modified { get; set; }


        /// <summary>
        /// Definisce se la playlist è stata presa da quelle presenti sul LEDbox 
        /// </summary>
        [JsonIgnore]
        public bool isremote = false;



        private int duration { get; set; }
        private bool counter_enable { get; set; }
        private int current_status { get; set; }
        private int status { get; set; }


        

        [JsonIgnore]
        public List<APILedbox.playlistfile> items_api { get
            {
                List<APILedbox.playlistfile> files=new List<APILedbox.playlistfile>();
                foreach (FilePlaylist item in items)
                {
                    files.Add(new APILedbox.playlistfile()
                    {
                        filename = item.filename,
                        duration=item.duration,
                        type=item.type

                    }) ;
                }

                return files;
    
            }
        }
        

        [JsonIgnore]
        public string Title { get { return title; } set{title = value;} }

        [JsonIgnore]
        public int Duration {
            get {
                return duration;
            }
            set {
                duration = value;
            }
        }

        [JsonIgnore]
        public int Counter_Duration { get { return max_counter_time; } set { max_counter_time = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Counter_DurationMM")); } }

        [JsonIgnore]
        public int Counter_Duration_min { get {
                return int.Parse(App.formatSecondToMinute(max_counter_time, "mm"));
            } }
        [JsonIgnore]
        public int Counter_Duration_sec { get { return int.Parse(App.formatSecondToMinute(max_counter_time, "ss")); } }

        [JsonIgnore]
        public bool Counter_Visible { get
            {
                if (isremote)
                    return false;


                switch (type)
                {
                    case TYPE_IMAGE:
                        return true;
                    case TYPE_VIDEO:
                        return true;
                    case TYPE_AUDIO:
                        return false;
                }

                return true;
            }
        }



        [JsonIgnore]
        public string CountSlide {
            get {
                if (isremote)
                    return AppResources.on_ledbox;

                return "(" + (items == null ? 0 : items.Count).ToString() + " " + AppResources.elements + ")";
            }
        }
        
        [JsonIgnore]
        public string BackgroundIcon
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return "#F5A800";
                    case STATUS_PAUSE:
                        return "#F5A800";
                    case STATUS_STOP:
                        return "#99FFFFFF";
                }

                return "#99FFFFFF";

            }
        }

        [JsonIgnore]
        public Color BackgroundRow
        {
            get
            {
                if (isremote)
                    return Color.FromHex("#44E5E5E5");
                else
                    return Color.FromHex("#99E5E5E5");
            }
        }


        [JsonIgnore]
        public bool IsVisibleStop {
            get {
                switch (status)
                {
                    case STATUS_PLAY:
                        return true;
                    case STATUS_PAUSE:
                        return true;
                    default:
                        return false;
                }
            }

        }


        [JsonIgnore]
        public string Counter_DurationMM { get {
                string result;
                if (max_counter_time == 0)
                    result = AppResources.no;
                else
                    result = App.formatSecondToMinute(max_counter_time, "mm:ss");

                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Counter_Label_Min"));

                return result;
            }
        }

        [JsonIgnore]
        public string Counter_DurationZero
        {
            get
            {
                string result;
                result = App.formatSecondToMinute(max_counter_time, "mm:ss");

                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Counter_Label_Min"));

                return result;
            }
        }


        [JsonIgnore]
        public bool Counter_Label_Min
        {
            get
            {
                if (max_counter_time == 0)
                    return false;
                else
                    return true;


            }
        }

        [JsonIgnore]
        public string Type { 
            get
            {
                switch (type)
                {
                    case TYPE_IMAGE:
                        return TYPE_IMAGE_LABEL;
                 //       break;
                    case TYPE_VIDEO:
                        return TYPE_VIDEO_LABEL;
                 //       break;
                    case TYPE_AUDIO:
                        return TYPE_AUDIO_LABEL;
                //        break;

                }
                return TYPE_IMAGE_LABEL;
            }  
        }

        [JsonIgnore]
        public string TypeName
        {
            get
            {
                switch (type)
                {
                    case TYPE_IMAGE:
                        return "imagevideo";
                    //       break;
                    case TYPE_VIDEO:
                        return "imagevideo";
                    //       break;
                    case TYPE_AUDIO:
                        return "audio";
                        //        break;

                }
                return "imagevideo";
            }
        }


        [JsonIgnore]
        public int Status {
            get {
                if (isremote)
                    return current_status;
                return status;
            }
            set {
                status = value;
                current_status = value;
                if (PropertyChanged != null) {
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseIconButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseTextButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("IsVisibleStop"));
                }
            }
        }

        [JsonIgnore]
        public List<FilePlaylist> Items { get { return items; } set { items = value; } }

        [JsonIgnore]
        public bool ControlImage { get { if (type == 0) return true;  return false; } }
        [JsonIgnore] 
        public string Icon {
            get
            {

                switch (status)
                {

                    case STATUS_STOP:
                        if (type == 0)
                            return "ic_image_24dp.png";
                        else
                            return "ic_music_24dp.png";
                    case STATUS_PLAY:
                        if (type == 0)
                            return "ic_image_24dp.png";
                        else
                            return "ic_music_24dp.png";
                    case STATUS_PAUSE:
                        if (type == 0)
                            return "ic_image_24dp.png";
                        else
                            return "ic_music_24dp.png";
                }

                return "";
            }
        }

        [JsonIgnore]
        public string PlayPauseIconButton
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return "ic_pause_white_24dp.png";
                    case STATUS_PAUSE:
                        return "ic_play_arrow_white_24dp.png";
                    case STATUS_STOP:
                        return "ic_play_arrow_white_24dp.png";
                    case STATUS_WAITING:
                        return "ic_stop_white_24dp.png";
                }

                return "ic_play_arrow_white_24dp.png";

            }
        }

        [JsonIgnore]
        public string PlayPauseTextButton
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return AppResources.pause;
                    case STATUS_PAUSE:
                        return AppResources.play;
                    case STATUS_WAITING:
                        return AppResources.cancel;
                }

                return AppResources.play;

            }
        }

       
        [JsonIgnore]
        public string BackgroundLoopIcon
        {
            get
            {
                if (!counter_enable)
                    return "#F5A800";
                else
                    return "#99FFFFFF";
            }
        }

        [JsonIgnore]
        public string BackgroundSetCounterIcon
        {
            get
            {
                if (!counter_enable)
                    return "#CCCCCC";
                else
                   return "#F5A800";
                
            }
        }

        [JsonIgnore]
        public bool VisibleSetCounterIcon
        {
            get
            {
                if (!counter_enable)
                    return false;
                else
                    return true;

            }
        }


        [JsonIgnore]
        public string ColorCounter
        {
            get
            {
                if (!counter_enable)
                    return "#CCCCCC";
                else
                    return "#FFFFFF";

            }
        }




        [JsonIgnore]
        public string TotalDuration {
            get {
                if (isremote)
                    return "";
                return App.formatSecondToMinute(getTotalDuration());
            }
        }

        [JsonIgnore]
        public bool isEmpty
        {
            get
            {
                if (items == null || items.Count == 0)
                    return true;
                return false;

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        public Playlist()
        {
            max_counter_time = 0;
            hashname = helper.GetHash(App.sport.name + "playlist" + new Random().Next().ToString());
            setLastModified();
        }

        
        public void setLastModified()
        {
            last_modified = DateTime.Now;
        }


        public void AddFile(FilePlaylist fp)
        {
            if (items == null)
                items = new List<FilePlaylist>();
            items.Add(fp);
        }



        void createZipMediaFile(Action<string, string> onComplete = null)
        {



            var loading = UserDialogs.Instance.Loading(AppResources.creation_progress, null,null,false);


            if (items.Count == 0)
            {
                loading.Hide();
                onComplete("", "");
                return;

            }
           

            string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = "playlist_" + TypeName + "_" + id.ToString() + "_" + title + "_" + helper.ToTimestamp(last_modified).ToString() + ".zip";
            string path = dir + "/" + filename;


            if (System.IO.File.Exists(path)) {
                loading.Hide();
                onComplete(filename, path);
                 return;
            }
            //return (filename, path);


            loading.Show();
            string[] files = new string[items.Count];
            int i = 0;
            foreach (FilePlaylist filePlaylist in items)
            {
                files[i] = filePlaylist.filepath;
                i++;
            }


            helper.CompressZipFile(path, (c)=> {
                loading.Hide();
                if (c)
                {
                    onComplete(filename, path);
                }
                else
                {
                    onComplete("", "");
                }
            
            
            },files);

          
               
        }

        private void PlayPause()
        {
            switch (Status)
            {
                case Practice.STATUS_PLAY:
                    Pause();
                    break;
                case Practice.STATUS_PAUSE:
                    Play(isremote);
                    break;
                case Practice.STATUS_STOP:
                    Play(isremote);
                    break;
            }



        }

        private void Play(bool remotePlay)
        {
            if (remotePlay)
                sendMessageToPlayRemote();
            else
                sendMessageToPlay();
            Status = Practice.STATUS_PLAY;

        }

        private void Pause()
        {
            sendMessageToPause();
            Status = Practice.STATUS_PAUSE;

        }

        private void Stop()
        {
            sendMessageToStop();
            Status = Practice.STATUS_STOP;


        }

        /// <summary>
        /// Invia direttamente il messaggio di PLAY playlist al LEDbox
        /// </summary>
        public void sendMessageToPlayRemote()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }
            if(type==TYPE_IMAGE)
                App.conn.SendMessage(App.api.createStartPlaylistImageMessage(this.hashname,this.title));
            if (type == TYPE_AUDIO)
                App.conn.SendMessage(App.api.createStartPlaylistAudioMessage(this.hashname,this.title));
        }

        public void sendMessageToPlay()
        {

            if (App.conn==null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }

            if (max_counter_time == 0)
                counter_enable = false;
           


            if (title == "" || title==null)
            {
                App.DisplayAlert(AppResources.insert_title);
                return;
            }

            if (Items == null)
                return;

            Status = STATUS_WAITING;

            //se è in stato di pause riavvia la riproduzione
            if (status == STATUS_PAUSE)
            {
               
                App.conn.SendMessage(App.api.createStartPlaylistMessage(this.hashname,this.title,this.type));
                return;
            }

            if (type != TYPE_AUDIO)
            {
                //verifica che non ci sia un processo di un altro plugin attivo
                List<Activity> activity_display = App.avm.getActivitiesByType(new int[] { Activity.TYPE_PRACTICE, Activity.TYPE_CUSTOM_TEXT });

                if (activity_display.Count > 0)
                {
                    App.DisplayAlert(AppResources.another_diplay_process_execute);
                    return;
                }
            }


            //carica tutti i file sul device

           
            System.Threading.Tasks.Task.Run(() =>
            {
                createZipMediaFile((filename, path) =>
                {
                    if (filename != "" && path != "")
                    {

                        string dest_folder = APILedbox.FILETYPE_MEDIA + "/" + App.alias;
                        string messageUpload = "";
                        if (this.type == Playlist.TYPE_IMAGE)
                        {
                            dest_folder = dest_folder + "/playlistimage";
                            messageUpload = App.api.createUploadPlaylistImageMessage(filename, path);
                        }
                        if (this.type == Playlist.TYPE_AUDIO)
                        {
                            dest_folder = dest_folder + "/playlistaudio";
                            messageUpload = App.api.createUploadPlaylistAudioMessage(filename, path);
                        }


                    //crea ed invia il messaggio
                    App.conn.SendMessage(App.api.createPlaylistMessage(this, type));


                        MessagingCenter.Subscribe<APILedbox, Playlist>(App.api, "playlist_setted", ((sender, playlistname) =>
                        {

                            App.conn.startUploadFile(messageUpload, (isFinish2) =>
                            {
                               
                                App.conn.SendMessage(App.api.createStartPlaylistMessage(this.hashname, this.title, this.type));


                               
                                MessagingCenter.Unsubscribe<APILedbox, Playlist>(App.api, "playlist_setted");
                            }, dest_folder, false);
                        }));


                    }
                });
            });




        }


        public void sendMessageToStop()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }
            App.conn.SendMessage(App.api.createStopPlaylistMessage(this.hashname,this.type));
            Status = STATUS_STOP;


        }

        public void sendMessageToPause()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }
            App.conn.SendMessage(App.api.createPausePlaylistMessage(this.hashname,this.type));
            status = STATUS_PAUSE;


        }

        public int getTotalDuration()
        {
            int result = 0;

            if (items == null)
                return 0;

            foreach(FilePlaylist filePlaylist in items)
            {
                result = result + filePlaylist.duration;
            }

            return result;
        }

        

        public object Clone()
        {
            return this.MemberwiseClone();
           
        }

        public int getStatus()
        {
            return status;
        }

        public string getTitle()
        {
            return title;
        }
    }
}
