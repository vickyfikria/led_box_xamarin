using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ledbox.ViewModels
{
    public class PracticeViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private INavigation Navigation;

        


        public ObservableCollection<Practice> OPractice { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public int id_category=0;


        public bool isEmpty
        {
            get
            {
                if (OPractice.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ledbox.ViewModels.PlaylistViewModel"/> class.
        /// </summary>
        /// <param name="navigation">Navigation.</param>
        public PracticeViewModel(INavigation navigation,int id_category=0)
        {

            
            this.id_category = id_category;

           

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

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_start", ((sender, practicename) =>
            {
                foreach (Practice p in OPractice)
                {
                    if (p.Title == practicename)
                    {
                        p.Status = Practice.STATUS_PLAY;


                    }
                }
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

                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
                }
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "practice_stop", ((sender, practicename) =>
            {
                foreach (Practice p in OPractice)
                {
                    p.Status = Practice.STATUS_STOP;
                }

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
            })
            );

            MessagingCenter.Subscribe<APILedbox, string>(App.api, "stop_all", ((sender, playlistname) =>
            {
                foreach (Practice p in OPractice)
                {
                    p.Status = Practice.STATUS_STOP;
                }

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
            })
           );



        }


        public void reloadList()
        {
            OPractice = new ObservableCollection<Practice>();

            if (App.storage.current_project.practices != null)
                foreach (Practice item in App.storage.current_project.practices)
                {
                    if (item.Category == id_category)
                        OPractice.Add(item);

                }

            reloadRemoteList();


            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
        }

        /// <summary>
        /// Azzera la lista delle practice da quelle già presenti sul LEDbox
        /// </summary>
        void resetRemotList()
        {
            //elimina tutti le practice remote già presenti
            for (int i = 0; i < OPractice.Count; i++)
                if (OPractice[i].isremote)
                    OPractice.Remove(OPractice[i]);
        }

        /// <summary>
        /// Verifica se una practice è presente nella ListView
        /// </summary>
        /// <param name="practicename"></param>
        /// <returns></returns>
        bool isPracticeInList(string practicename)
        {
            foreach (Practice practice in OPractice)
                if (practice.Title == practicename)
                    return true;

            return false;
        }


        /// <summary>
        /// Popola la lista delle practice già presenti sul LEDbox nella ListView
        /// </summary>
        public void reloadRemoteList()
        {

            resetRemotList();



            MessagingCenter.Subscribe<APILedbox, List<Practice>>(App.api, "practice_getlist", (api, value) => {

                resetRemotList();

                foreach (Practice item in value)
                {
                    if (!isPracticeInList(item.Title))
                    {
                        item.isremote = true;
                        OPractice.Add(item);
                    }
                }

                NotifyChange();

            });


            if (App.conn != null && App.conn.isConnected())
                App.conn.SendMessage(App.api.createGetListPracticeMessage());
        }



        /// <summary>
        /// Elimina una practice dalla lista
        /// </summary>
        /// <param name="p"></param>
        public void RemoveCommand(Practice p)
        {
            App.storage.current_project.practices.Remove(p);
            OPractice.Remove(p);
            App.storage.saveFile();
        }


        /// <summary>
        /// Notifica le modifiche alla contentPage
        /// </summary>
        public void NotifyChange()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("OPractice"));
                PropertyChanged(this, new PropertyChangedEventArgs("isEmpty"));
            }

        }

    }
}
