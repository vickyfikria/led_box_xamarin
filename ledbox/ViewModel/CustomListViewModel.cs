using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;

namespace ledbox
{
    public class CustomListViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private INavigation Navigation;
        public ObservableCollection<CustomText> OCustomText { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        public bool isEmpty
        {
            get
            {
                if (OCustomText.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public CustomListViewModel(INavigation navigation)
        {

            reloadList();

            this.Navigation = navigation;

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "customtext_start", ((sender, customtextname) =>
            {
                foreach (CustomText p in OCustomText)
                {
                    if (p.Title == customtextname)
                    {
                        p.status = CustomText.STATUS_PLAY;


                    }
                }
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "customtext_stop", ((sender, customtextname) =>
            {
                foreach (CustomText p in OCustomText)
                {
                    if (p.Title == customtextname)
                        p.status = CustomText.STATUS_STOP;
                }
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "stop_all", ((sender, playlistname) =>
            {
                foreach (CustomText p in OCustomText)
                {
                    p.status = CustomText.STATUS_STOP;
                }
            })
           );

        }


        public void reloadList()
        {
            OCustomText = new ObservableCollection<CustomText>();

            if (App.storage.current_project.customTexts != null)
                foreach (CustomText item in App.storage.current_project.customTexts)
                {
                    OCustomText.Add(item);
                }
            NotifyChange();

        }

        public void RemoveCommand(CustomText c)
        {
            App.storage.current_project.customTexts.Remove(c);
            OCustomText.Remove(c);
            App.storage.saveFile();
        }

        /// <summary>
        /// Notifica le modifiche alla contentPage
        /// </summary>
        public void NotifyChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OCustomText"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }

        }


    }
}
