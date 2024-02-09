using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using ledbox.Resources;
using Acr.UserDialogs;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ledbox
{
    public class PracticePresetsViewModel : BaseViewModel, INotifyPropertyChanged
    {

        private INavigation Navigation;
        public ObservableCollection<PracticePreset> OPracticePreset { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ReloadListCommand { get; private set; }



        public PracticePresetsViewModel(INavigation navigation)
        {
            reloadList();
            this.Navigation = navigation;
            ReloadListCommand = new Command(reloadList);
        }


        public async void reloadList()
        {
            var loading = UserDialogs.Instance.Loading(null, () => { }, AppResources.cancel);
          
            


            //scarica i practice preset
            App.webservice.DownloadLastPracticesPreset((listPresetPractice) => {
                if (listPresetPractice == "")
                {
                    loading.Hide();
                    return;
                }
                List<PracticePreset> presets = JsonConvert.DeserializeObject<List<PracticePreset>>(listPresetPractice);
                OPracticePreset = new ObservableCollection<PracticePreset>();
                if (presets.Count > 0)
                    foreach (PracticePreset preset in presets)
                    {
                        OPracticePreset.Add(preset);
                    }

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("OPracticePreset"));

                loading.Hide();

            });




           

            

            
        }


    }
}
