using ledbox.Resources;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;



namespace ledbox
{
    public class CustomViewModel : BaseViewModel, INotifyPropertyChanged
    {


        //string _iconStatusPlay = "ic_play_arrow_white_24dp";
        //string _iconStatusStop = "ic_stop_white_24dp.png";

        public ICommand PlayPauseCommand { get; private set; }
        public ICommand StopCommand { get; private set; }

        public ICommand DoneEditingCommand { get; private set; }
     
        private INavigation Navigation;
        private bool isNew = false;
        public CustomText customText { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;



        public CustomViewModel(INavigation navigation, CustomText customText, bool isNew)
        {
            this.isNew = isNew;
            if (customText == null)
                customText = new CustomText();

            this.customText = customText;


            DoneEditingCommand = new Command(DoneEditing);
            StopCommand = new Command(Stop);
            PlayPauseCommand = new Command(PlayPause);



            this.Navigation = navigation;

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "customtext_start", ((sender, customtextname) =>
            {
                if (customText.Title == customtextname)
                {
                    customText.status = CustomText.STATUS_PLAY;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("customText.iconStatus"));

                }

            })
          );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "customtext_pause", ((sender, customtextname) =>
            {
                if (customText.Title == customtextname)
                {
                    customText.status = CustomText.STATUS_PAUSE;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("customText.iconStatus"));

                }

            })
          );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "customtext_stop", ((sender, customtextname) =>
            {
                customText.status = CustomText.STATUS_STOP;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("customText.iconStatus"));

            })
            );
        }


        public void DoneEditing()
        {

            //verifica se il nome playlist è stato inserito
            if (this.customText.Title == "" || this.customText.Title == null)
            {
                App.DisplayAlert(AppResources.insert_title);
                return;
            }


            if (isNew)
                App.storage.addCustomText(this.customText);
          
            App.storage.saveFile();
            //await this.Navigation.PopAsync();


        }

        private void PlayPause()
        {

            switch (customText.status)
            {
                case CustomText.STATUS_PLAY:
                    Pause();
                    break;

                case CustomText.STATUS_PAUSE:
                    Play();
                    break;
                case CustomText.STATUS_STOP:
                    Play();
                    break;
            }



        }

        private void Play()
        {

                customText.sendMessageToPlay();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("customText.BackgroundStopIcon"));


            
        }

        private void Pause()
        {
            customText.sendMessageToPause();

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("customText.BackgroundStopIcon"));

            }
        }

        private void Stop()
        {
            customText.sendMessageToStop();
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("customText.BackgroundStopIcon"));


        }





    }
}
