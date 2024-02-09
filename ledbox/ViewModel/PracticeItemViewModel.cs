using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using ledbox.Resources;

namespace ledbox
{
    public class PracticeItemViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ICommand DoneEditingCommand { get; private set; }
        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }

        private INavigation Navigation;
        private bool isNew = false;
        public Practice Practice { get; private set; }

        public ObservableCollection<ItemPractice> OItemPractice { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public bool isRemote
        {
            get
            {
                return Practice.isremote;
            }
        }

        public bool isLocal
        {
            get
            {
                return !Practice.isremote;
            }
        }

        public bool isEmpty
        {
            get
            {
                if (isRemote)
                    return false;
                return Practice.isEmpty;
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



        public PracticeItemViewModel(INavigation navigation, Practice practice,bool isNew)
        {
            this.isNew = isNew;
            if (practice == null)
                practice = new Practice();

            this.Practice = practice;

            reloadList();

            DoneEditingCommand = new Command(DoneEditing);
            PlayPauseCommand = new Command(PlayPause);
            StopCommand = new Command(Stop);


            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_start", ((sender, practicename) =>
            {
                if (Practice.hashname == practicename)
                {
                    Practice.Status = Practice.STATUS_PLAY;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Practice.Status"));

                }

            })
           );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_pause", ((sender, practicename) =>
            {
                if (Practice.hashname == practicename)
                {
                    Practice.Status = Practice.STATUS_PAUSE;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Practice.Status"));

                }

            })
           );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_stop", ((sender, practicename) =>
            {
                Practice.Status = Practice.STATUS_STOP;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Practice.Status"));
                
            })
            );


            this.Navigation = navigation;
        }


        public void reloadList()
        {
            bool is_changed = false;


            OItemPractice = new ObservableCollection<ItemPractice>();
            if (Practice != null && Practice.Items != null)
            {
                int i = 1;
                foreach (ItemPractice fp in Practice.Items)
                {
                    fp.ordering = i;
                    i++;
                    OItemPractice.Add(fp);
                    if (fp.is_changed)
                        is_changed = true;
                }
                    

            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OItemPractice"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }
            if (is_changed)
                Practice.setLastModified();
        }

        public void DoneEditing()
        {

            //verifica se il nome playlist è stato inserito
            if (this.Practice.Title == "" || this.Practice.Title == null)
            {
                App.DisplayAlert(AppResources.insert_name_practice);
                return;
            }

            if (isNew)
                App.storage.addPractice(this.Practice);
            
            /*else
                App.storage.editPractice(this.Practice, this.Practice.id);*/
            
            App.storage.saveFile();
            //await this.Navigation.PopAsync();


        }


        public void RemoveCommand(ItemPractice fp)
        {

            this.Practice.Items.Remove(fp);
            OItemPractice.Remove(fp);
            App.storage.saveFile();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
        }

        public void DuplicateCommand(ItemPractice fp)
        {
            ItemPractice fpclone = (ItemPractice)fp.Clone();
            this.Practice.Items.Add(fpclone);
            App.storage.saveFile();
            reloadList();

        }


        public void movePlaylist(ItemPractice fp, int direction = 1)
        {
            int oldIndex = 0;
            int newIndex;

            //trova l'indice del file
            for (int i = 0; i < this.Practice.Items.Count; i++)
                if (this.Practice.Items[i] == fp)
                {
                    oldIndex = i;
                    continue;
                }

            newIndex = oldIndex + direction;
            if (newIndex > (this.Practice.Items.Count - 1))
                newIndex = this.Practice.Items.Count - 1;

            if (newIndex < 0)
                newIndex = 0;

            var item = this.Practice.Items[oldIndex];

            this.Practice.Items.RemoveAt(oldIndex);
            this.Practice.Items.Insert(newIndex, item);

            reloadList();
        
        }

        private void PlayPause()
        {
            switch (Practice.Status)
            {
                case Practice.STATUS_PLAY:
                    Pause();
                    break;
                case Practice.STATUS_PAUSE:
                    Play();
                    break;
                case Practice.STATUS_STOP:
                    Play();
                    break;
            }

           
        }

        private void Play()
        {
            if (Practice.isremote)
            {
                Practice.sendMessageToPlayRemote();
            }
            else
            {
                if (Practice.Items != null && Practice.Items.Count > 0)
                {
                    Practice.sendMessageToPlay();
                }
                else

                    App.DisplayAlert(AppResources.none_practice_item);
            }
        }

            private void Pause()
            {
                    Practice.sendMessageToPause();

            }

            private void Stop()
        {
            Practice.sendMessageToStop();


        }


    }
}
