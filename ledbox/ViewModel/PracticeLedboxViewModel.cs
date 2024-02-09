using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using ledbox.Resources;
using System.Collections.Generic;

namespace ledbox.ViewModels
{
    public class PracticeLedboxViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private INavigation Navigation;

        


        public ObservableCollection<Practice> OPractice { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;


        /// <summary>
        /// Definisce se gli elementi si riferiscono a Practice o a Weight Gym
        /// </summary>
        public int id_category=0;

        /// <summary>
        /// Definisce se vi sono o meno practice
        /// </summary>
        public bool isEmpty
        {
            get
            {
                if (OPractice!=null && OPractice.Count > 0)
                    return false;
                return true;
            }
        }



        /// <summary>
        /// Definisce il messaggio di  stato delle practice del LEDbox
        /// </summary>
        public string lblStatus
        {
            get
            {
                if (App.conn == null)
                    return AppResources.no_ledbox_connect_to_practice_remote;

                if (OPractice ==null || OPractice.Count == 0)
                    return AppResources.list_empty_practice_remote;

                return "";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public PracticeLedboxViewModel(INavigation navigation,int id_category=0)
        {

            
            this.id_category = id_category;


            this.Navigation = navigation;

            MessagingCenter.Subscribe<APILedbox, APILedbox.current_setting>(App.api, "connected", ((sender, setting) =>
            {
                reloadList();
            })
            );

            MessagingCenter.Subscribe<APILedbox>(this, "disconnect", (arg) =>
            {
                OPractice = new ObservableCollection<Practice>();
                NotifyChange();
            });


            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_pause", ((sender, practicename) =>
            {
                foreach (Practice p in OPractice)
                {
                    if (p.Title == practicename)
                    {
                        p.Status = Practice.STATUS_PAUSE;


                    }
                }
                NotifyChange();
            })
          );



            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_pause", ((sender, practicename) =>
            {
                foreach (Practice p in OPractice)
                {
                    if (p.Title == practicename)
                    {
                        p.Status = Practice.STATUS_PAUSE;


                    }
                }
                NotifyChange();
            })
          );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_stop", ((sender, practicename) =>
            {
                foreach (Practice p in OPractice)
                {
                    p.Status = Practice.STATUS_STOP;
                }
                NotifyChange();
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "stop_all", ((sender, playlistname) =>
            {
                foreach (Practice p in OPractice)
                {
                    p.Status = Practice.STATUS_STOP;
                }
                NotifyChange();
            })
           );

        }

        /// <summary>
        /// Popola la lista delle practice nella ListView
        /// </summary>
        public void reloadList()
        {
            OPractice = new ObservableCollection<Practice>();

            


            MessagingCenter.Subscribe<APILedbox, List<Practice>>(App.api, "practice_getlist", (api, value) => {




                OPractice = new ObservableCollection<Practice>();

                foreach (Practice item in value)
                {
                    item.isremote = true;
                    OPractice.Add(item);
                }

                NotifyChange();

            });
            if (App.conn != null)
                App.conn.SendMessage(App.api.createGetListPracticeMessage());
            else
                App.DisplayAlert(AppResources.connecting_before_ledbox);
        }


        /// <summary>
        /// Notifica le modifiche alla contentPage
        /// </summary>
        void NotifyChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
                PropertyChanged(this, new PropertyChangedEventArgs("lblStatus"));
            }

        }            

    }
}
