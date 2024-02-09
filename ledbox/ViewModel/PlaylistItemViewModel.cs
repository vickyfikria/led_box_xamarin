using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System.IO;
using Acr.UserDialogs;
using ledbox.Resources;

namespace ledbox
{
    public class PlaylistItemViewModel : BaseViewModel, INotifyPropertyChanged
    {

      
        public ICommand DoneEditingCommand { get; private set; }
        public ICommand EditTitleCommand { get; private set; }
        public ICommand AddFileCommand { get; private set; }

        public ICommand PlayPauseCommand { get; private set; }

        public ICommand OpenTimerCommand { get; private set; }

        public ICommand PlayCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand LoopCommand { get; private set; }

        public ICommand AddCounterCommand { get; private set; }
        public ICommand ReduceCounterCommand { get; private set; }

        private INavigation Navigation;
        private bool isNew = false;
        public Playlist Playlist { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public bool isRemote
        {
            get
            {
                return Playlist.isremote;
            }
        }

        public bool isLocal
        {
            get
            {
                return !Playlist.isremote;
            }
        }

        public bool isEmpty
        {
            get
            {
                if (isRemote)
                    return false;
                return Playlist.isEmpty;
            }
        }

        /// <summary>
        /// Definisce il messaggio di  stato delle practice del LEDbox
        /// </summary>
        public string lblStatus
        {
            get
            {

                return AppResources.item_only_on_ledbox;
            }
        }

        public ObservableCollection<FilePlaylist> OFilePlaylist { get; set; }

        private string openlastlayout;

        public PlaylistItemViewModel(INavigation navigation, Playlist playlist, bool isNew, string openlastlayout)
        {
            this.isNew = isNew;
            this.openlastlayout = openlastlayout;
            if (playlist == null)
                playlist = new Playlist();

            this.Playlist = playlist;




            reloadList();


            DoneEditingCommand = new Command(DoneEditing);
            EditTitleCommand = new Command(TitleEditing);
            //AddFileCommand = new Command(AddFile);
            PlayCommand = new Command(Play);
            StopCommand = new Command(Stop);
            PlayPauseCommand = new Command(PlayPause);
            //LoopCommand = new Command(Loop);

            AddCounterCommand = new Command(addCounter);
            ReduceCounterCommand = new Command(reduceCounter);
            OpenTimerCommand = new Command(openTimerPlaylist);




            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_start", ((sender, playlistname) =>
            {
                if (Playlist.hashname == playlistname)
                {
                    Playlist.Status = Playlist.STATUS_PLAY;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Status"));

                }

            })
           );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_pause", ((sender, playlistname) =>
            {
                if (Playlist.hashname == playlistname)
                {
                    Playlist.Status = Playlist.STATUS_PAUSE;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Status"));

                }

            })
           );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "playlist_stop", ((sender, playlistname) =>
                {
                    if (Playlist.hashname == playlistname)
                    {
                        Playlist.Status = Playlist.STATUS_STOP;
                        if (PropertyChanged != null)
                            PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Status"));
                    }
                })
            );

            this.Navigation = navigation;
        }


        public void reloadList()
        {
            OFilePlaylist = new ObservableCollection<FilePlaylist>();
            if (Playlist != null && Playlist.Items != null)
                foreach (FilePlaylist fp in Playlist.Items)
                    OFilePlaylist.Add(fp);
        }

        public void DoneEditing()
        {

            //verifica se il nome playlist è stato inserito
            if (this.Playlist.Title == "" || this.Playlist.Title == null)
            {
                App.DisplayAlert(AppResources.insert_name_playlist);
                return;
            }


            if (isNew)
                App.storage.addPlaylist(this.Playlist);
            /*else
                App.storage.editPlaylist(this.Playlist, this.Playlist.id);*/


            App.storage.saveFile();
            //await this.Navigation.PopAsync();


        }

        public async void TitleEditing()
        {

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
                Text = Playlist.Title
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                Playlist.Title = pResult.Text;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Title"));


            }




        }

        public void addCounter()
        {
            Playlist.Counter_Duration = Playlist.Counter_Duration + 60;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Counter_DurationMM"));
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Counter_Label_Min"));
            }

        }

        public void reduceCounter()
        {
            if (Playlist.Counter_Duration > 0)
                Playlist.Counter_Duration = Playlist.Counter_Duration - 60;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Counter_Duration"));
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Counter_Label_Min"));
            }


        }


        public void AddPhoto()
        {

            helper.AddPhoto((path) =>
            {

                if (path != "")
                {
                    FilePlaylist fp = new FilePlaylist();
                    fp.Type = FilePlaylist.TYPE_IMAGE;
                    fp.Filename = Path.GetFileName(path);
                    fp.Filepath = path;
                    fp.Previewpath = path;
                    fp.Duration = 5;

                    Playlist.AddFile(fp);
                    if (OFilePlaylist == null)
                        OFilePlaylist = new ObservableCollection<FilePlaylist>();
                    OFilePlaylist.Add(fp);

                    Playlist.setLastModified();

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));

                }
            });

        }

        public async void editPhoto(FilePlaylist filePlaylist)
        {
            filePlaylist.Previewpath = await helper.CropImage(filePlaylist.Previewpath, 3, 1);
            reloadList();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OFilePlaylist"));

        }


        public void AddVideo()
        {

            helper.AddVideo((path, preview, duration) =>
            {

                if (path != "")
                {
                    FilePlaylist fp = new FilePlaylist();
                    fp.Type = FilePlaylist.TYPE_VIDEO;
                    fp.Filename = Path.GetFileName(path);
                    fp.Filepath = path;
                    fp.Previewpath = preview;
                    //fp.Ordering = OFilePlaylist.Count + 1;
                    fp.Duration = duration;

                    Playlist.AddFile(fp);
                    if (OFilePlaylist == null)
                        OFilePlaylist = new ObservableCollection<FilePlaylist>();
                    OFilePlaylist.Add(fp);

                    Playlist.setLastModified();
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));

                }
            });

        }


        public async void AddAudio()
        {
            try
            {


                if (!DependencyService.Get<IPermissions>().checkPermission(6))
                    return;

                string[] allowType = new string[1];
                allowType[0] = "audio/mpeg";

                //TODO da abilitare senza il plugin FilePicker
                FileData fileData = await CrossFilePicker.Current.PickFile(allowType);
                if (fileData == null)
                    return; // user canceled file picking


                if (fileData.FilePath.Contains("content://"))
                    fileData.FilePath = DependencyService.Get<IDirectory>().GetPathFromUri(fileData.FilePath, fileData.FileName);

                //copia il file
                string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                string path = "";
                path = dir + "/" + fileData.FileName;
                if(!File.Exists(path))
                    File.Copy(fileData.FilePath, path);



                FilePlaylist fp = new FilePlaylist();
                fp.Type = FilePlaylist.TYPE_AUDIO;
                fp.Filename = fileData.FileName;
                fp.Filepath = path;
                //fp.Ordering = OFilePlaylist.Count + 1;
                fp.duration = DependencyService.Get<IDirectory>().GetDurationMp3(fp.Filepath);

                Playlist.AddFile(fp);
                if (OFilePlaylist == null)
                    OFilePlaylist = new ObservableCollection<FilePlaylist>();
                OFilePlaylist.Add(fp);

                Playlist.setLastModified();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception choosing file: " + ex.ToString());
            }




        }

        private void PlayPause()
        {

            switch (Playlist.Status)
            {
                case Playlist.STATUS_PLAY:
                    Pause();
                    break;

                case Playlist.STATUS_PAUSE:
                    Play();
                    break;
                case Playlist.STATUS_STOP:
                    Play();
                    break;
                case Playlist.STATUS_WAITING:
                    Stop();
                    break;
            }



        }


        private void openTimerPlaylist()
        {
            Playlist.onfinish = "";

            TimerPlaylistView timerPlaylistView = new TimerPlaylistView(Playlist);

            timerPlaylistView.Disappearing += ((sender, obj) =>
            {

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Playlist.Duration"));
            });

            Navigation.PushModalAsync(timerPlaylistView);
            
        }


        private void Play()
        {

            if (Playlist.isremote)
            {
                Playlist.sendMessageToPlayRemote();
            }
            else
            {

                if (Playlist.Items != null && Playlist.Items.Count > 0)
                {

                    Playlist.onfinish = openlastlayout;
                    Playlist.sendMessageToPlay();
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Playlist.BackgroundStopIcon"));


                }
                else

                    App.DisplayAlert(AppResources.none_playlist_item);
            }
        }

        private void Stop()
        {
            Playlist.sendMessageToStop();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.BackgroundStopIcon"));


        }

        private void Pause()
        {
            Playlist.sendMessageToPause();

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.BackgroundStopIcon"));

            }
        }



        public void RemoveCommand(FilePlaylist fp)
        {
            this.Playlist.Items.Remove(fp);
            OFilePlaylist.Remove(fp);
            App.storage.saveFile();
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.IsVisiblePlay"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }
        }

        public void DuplicateCommand(FilePlaylist fp)
        {
            FilePlaylist fpclone = (FilePlaylist)fp.Clone();
            this.Playlist.Items.Add(fpclone);
            OFilePlaylist.Add(fpclone);
            App.storage.saveFile();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Playlist.IsVisiblePlay"));


        }

        public void movePlaylist(FilePlaylist fp, int direction = 1)
        {
            int oldIndex = 0;
            int newIndex;

            //trova l'indice del file
            for (int i = 0; i < this.Playlist.Items.Count; i++)
                if (this.Playlist.Items[i] == fp)
                {
                    oldIndex = i;
                    continue;
                }

            newIndex = oldIndex + direction;
            if (newIndex > (this.Playlist.Items.Count - 1))
                newIndex = this.Playlist.Items.Count - 1;

            if (newIndex < 0)
                newIndex = 0;

            var item = this.Playlist.Items[oldIndex];

            this.Playlist.Items.RemoveAt(oldIndex);
            this.Playlist.Items.Insert(newIndex, item);

            OFilePlaylist.RemoveAt(oldIndex);
            OFilePlaylist.Insert(newIndex, fp);
            App.storage.saveFile();

        }


    }
}
