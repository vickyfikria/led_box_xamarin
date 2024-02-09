using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using ledbox.Resources;

namespace ledbox.ViewModel
{
    public class ActivityViewModel : BaseViewModel,INotifyPropertyChanged
    {
        public ObservableCollection<Activity> OActivity{ get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public static List<Activity> activities; // attività correnti in esecuzione sul LEDBox


        public ActivityViewModel()
        {
            reloadList();

        }

        public void reloadList()
        {
            CheckActivities();
            OActivity = new ObservableCollection<Activity>();

            if (activities != null)
                foreach (Activity item in activities)
                {
                    OActivity.Add(item);
                }
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("OActivity"));

        }

        /// <summary>
        /// Aggiunge l'attività corrente del LEDbox all'elenco sul device
        /// </summary>
        public void AddActivity(string title,string hashname, int type, int status)
        {
            Activity activity = new Activity();
            activity.title = title;
            activity.hashname = hashname;
            activity.status = status;
            activity.type = type;

            if (activities == null)
                activities = new List<Activity>();


            //verifica se l'attività non è già presente
            foreach (Activity activity1 in activities)
            {
                if (activity1.hashname == activity.hashname && activity1.type == activity.type)
                {
                    activity1.status = activity.status;
                    reloadList();
                    return;
                }
            }

            //altrimenti aggiungilo all'elenco
            activities.Add(activity);
            reloadList();

        }

        /// <summary>
        /// Estre dalla lista corrente delle attività quelle della tipologia type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<Activity> getActivitiesByType(int[] types)
        {
            List<Activity> list_activities = new List<Activity>();
            if(activities!=null && activities.Count>0)
                foreach(Activity activity in activities)
                {
                    foreach (int type in types)
                    {
                        if (activity.type == type)
                            list_activities.Add(activity);
                    }
                }

            return list_activities;
        }





        public void CheckActivities()
        {
            if (activities != null && activities.Count > 0)
                App.sbvm.setShowMessage(true);
            else
                App.sbvm.setShowMessage(false);
        }

        /// <summary>
        /// Verifica se ci sono attività in corso e visualizza una dialog per la conferma di apertura della schermata ActivityView
        /// </summary>
        public async void DialogActivities()
        {
            if (activities != null && activities.Count > 0)
            {
               
                var answer = await App.main.DisplayAlert(AppResources.activity, AppResources.activitydialog, AppResources.yes, AppResources.no);
                if (answer)
                    App.sbvm.OpenActivities();
               
            }
        }


        /// <summary>
        /// Rimuove l'activity dall'elenco
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        public void RemoveActivity(string hashname, int type)
        {
            //verifica se l'attività non è già presente
            foreach (Activity activity1 in activities)
            {
                if (activity1.hashname == hashname && activity1.type == type)
                {
                    activities.Remove(activity1);
                    reloadList();
                    return;
                }
            }
        }


        public void ClearActivities()
        {
            if (activities != null)
            {
                activities.Clear();
                reloadList();
            }
        }

        
    }
}
