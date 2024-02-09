using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using ledbox.Resources;
using MvvmHelpers;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public class Practice: ObservableObject, INotifyPropertyChanged, ICloneable, IActivity
    {
        public const int STATUS_PLAY = 1;
        public const int STATUS_STOP = 0;
        public const int STATUS_PAUSE = 2;
        public const int STATUS_WAITING = 3;

        public const int TYPE_AUDIO=0;
        public const int TYPE_IMAGE = 1;




        public int id = 0;
        public string hashname = "";
        public string title {get; set;}
        public int type { get; set; }
        public List<ItemPractice> items;
        public DateTime last_modified { get; set; }


        private int duration { get; set; }
        private int status { get; set; }
        private int category { get; set; }
        private int current_status { get; set; }

        [JsonIgnore]
        public string Title { get { return title; } set{title = value;} }
        [JsonIgnore]
        public int Duration { get { return duration; } set { duration = value; } }
        [JsonIgnore]
        public int Type { get { return type; } set { type = value; } }

        /// <summary>
        /// Definisce se la practice è stata presa da quelle presenti sul LEDbox 
        /// </summary>
        [JsonIgnore]
        public bool isremote = false;


        [JsonIgnore]
        public List<APILedbox.practicefile> items_api
        {
            get
            {
                List<APILedbox.practicefile> files = new List<APILedbox.practicefile>();
                foreach (ItemPractice item in items)
                {
                    files.Add(new APILedbox.practicefile()
                    {
                        filename = item.filename,
                        rest = item.rest,
                        work=item.work,
                        soundrest=item.soundrest,
                        soundwork=item.soundwork,
                        round=item.round,
                        type = item.type

                    });
                }

                return files;

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
                    PropertyChanged(this, new PropertyChangedEventArgs("BackgroundIcon"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseIconButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseTextButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("IsVisibleStop"));
                }
            }
        }



        [JsonIgnore]
        public List<ItemPractice> Items { get { return items; } set { items = value; } }
        [JsonIgnore]
        public int Category { get { return category; } set { category = value; } }
        [JsonIgnore]
        public int totalduration { get { return getTotalDuration(); } }

        [JsonIgnore]
        public string CountSlide {
            get {
                if (isremote)
                    return AppResources.on_ledbox;               
                return "(" + (items == null ? 0 : items.Count).ToString() + " " + AppResources.elements + ")";
            }
        }

        [JsonIgnore]
        public string PlayPauseIconButton
        {
            get
            {
                switch (Status)
                {
                    case STATUS_PLAY:
                        return "ic_pause_white_24dp.png" ;
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
                switch (Status)
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
        public string BackgroundIcon
        {
            get
            {
                switch (Status)
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
        public bool IsVisibleStop
        {
            get
            {
                switch (Status)
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
        public string TotalDuration {
            get {
                if (isremote)
                    return "";
                return helper.SecondToMinute(getTotalDuration());
            }
        }

        [JsonIgnore]
        public bool isEmpty
        {
            get
            {
                if (items.Count == 0)
                    return true;
                return false;

            }
        }


        [JsonIgnore]
        public ICommand StopCommand { get; private set; }
        [JsonIgnore]
        public ICommand PlayPauseCommand { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public Practice()
        {
            hashname = helper.GetHash(App.sport.name + "practice" + new Random().Next().ToString());
            items = new List<ItemPractice>();
            setLastModified();
            PlayPauseCommand = new Command(PlayPause);
            StopCommand = new Command(Stop);
        }


        public void setLastModified()
        {
            last_modified = DateTime.Now;
        }

        public void AddFile(ItemPractice fp)
        {
            if (items == null)
                items = new List<ItemPractice>();
            items.Add(fp);
        }


        void createZipMediaFile(Action<string, string> onComplete = null)
        {

            var loading = UserDialogs.Instance.Loading(AppResources.creation_progress, null, null, false);



            if (items.Count == 0)
            {
                loading.Hide();
                onComplete("", "");
                return;

            }

            string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = "practice_"+id.ToString()+"_"+title + "_" + helper.ToTimestamp(last_modified).ToString() + ".zip";
            string path = dir + "/" + filename;


            if (System.IO.File.Exists(path))
            {
                loading.Hide();
                onComplete(filename, path);
                return;
            }
               
            loading.Show();
            string[] files = new string[items.Count];
            int i = 0;
            foreach (ItemPractice itemPractice in items)
            {
                files[i] = itemPractice.filepath;
                i++;
            }

            helper.CompressZipFile(path, (c) => {
                loading.Hide();
                if (c)
                {
                    onComplete(filename, path);
                }
                else
                {
                    onComplete("", "");
                }


            }, files);

           
        }

        /// <summary>
        /// Invia direttamente il messaggio di PLAY practice al LEDbox
        /// </summary>
        public void sendMessageToPlayRemote()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }

            App.conn.SendMessage(App.api.createStartPracticeMessage(this.hashname,this.title));
        }

        /// <summary>
        /// Verifica se sul LEDbox è presente una practice, in caso contrario fa l'upload. Alla fine del processo invia il messaggio di PLAY
        /// </summary>
        public void sendMessageToPlay()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }

            if (title == "" || title == null)
            {
                App.DisplayAlert(AppResources.insert_title);
                return;
            }



            //verifica che non ci sia un processo di un altro plugin attivo
            List<Activity> activity_display = App.avm.getActivitiesByType(new int[] { Activity.TYPE_PLAYLIST_IMAGE, Activity.TYPE_CUSTOM_TEXT });

            if (activity_display.Count > 0)
            {
                App.DisplayAlert(AppResources.another_diplay_process_execute);
                return;
            }

            Status = STATUS_WAITING;

            //se è in stato di pause riavvia la riproduzione
            if (status == STATUS_PAUSE)
            {
                App.conn.SendMessage(App.api.createStartPracticeMessage(this.hashname,this.title));
                return;
            }


            System.Threading.Tasks.Task.Run(() =>
            {

                createZipMediaFile((filename, path) =>
                {

                    if (filename != "" && path != "")
                    {
                        //crea ed invia il messaggio
                        App.conn.SendMessage(App.api.createPracticeMessage(this));
                        MessagingCenter.Subscribe<APILedbox, Practice>(App.api, "practice_setted", ((sender, playlistname) =>
                        {
                            App.conn.startUploadFile(App.api.createUploadPracticeMessage(filename, path), (isFinish2) =>
                            {

                                App.conn.SendMessage(App.api.createStartPracticeMessage(this.hashname, this.title));

                                MessagingCenter.Unsubscribe<APILedbox, Practice>(App.api, "practice_setted");
                            }, APILedbox.FILETYPE_MEDIA);
                        }));


                    }
                });
            });


        }



        /// <summary>
        /// Invia il messaggio di STOP practice al LEDbox
        /// </summary>
        public void sendMessageToStop()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }

            if (App.conn != null){
                App.conn.SendMessage(App.api.createStopPracticeMessage(this.hashname));
                Status = STATUS_STOP;
                if (Items != null)
                    foreach (ItemPractice itemPractice in Items)
                    {
                        itemPractice.stop();
                    }
            }


        }

        /// <summary>
        /// Invia il messaggio di PAUSE practice al LEDbox
        /// </summary>
        public void sendMessageToPause()
        {

            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }

            if (App.conn != null)
            {
                App.conn.SendMessage(App.api.createPausePracticeMessage(this.hashname));
                Status = STATUS_PAUSE;
            }

        }

        
        /// <summary>
        /// Restituisce la durata complessiva della practice
        /// </summary>
        /// <returns></returns>
        public int getTotalDuration()
        {
            int result = 0;

            if (items == null)
                return 0;

            foreach (ItemPractice itemPractice in items)
            {
                result = result + itemPractice.getTotalDuration();
            }

            return result;
        }

        public object Clone()
        {
            return this.MemberwiseClone();

        }

        public int getStatus()
        {
            return 0;
        }
        public string getTitle()
        {
            return title;
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
            if(remotePlay)
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
    }
}
