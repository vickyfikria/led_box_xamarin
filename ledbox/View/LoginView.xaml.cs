using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;
using Newtonsoft.Json;
using ledbox.Resources;
using System.Text.RegularExpressions;

namespace ledbox
{
    public partial class LoginView : ContentPage
    {

        public LoginView()
        {
            InitializeComponent();
            aliasInput.Text = App.alias;
            imageCover.Source = App.image_cover;

            checkSportListOnline((onCheckOnlineFinish) =>
            {
                setSportList();

                if (App.sport != null)
                    if(App.sports != null && App.sports.Count>0)
                        for (int i = 0; i < App.sports.Count; i++)
                            if (App.sports[i].name == App.sport.name)
                            {
                                sportPicker.SelectedIndex = i;
                                continue;
                            }
            });
            


        }

        async void BtOk_Clicked(object sender, System.EventArgs e)
        {
            if (aliasInput.Text==null || aliasInput.Text == "")
            {
                App.DisplayAlert(AppResources.username_required);
                return;
            }

            //elimina caratteri vuoti iniziali e finali nel nome utente
            App.sbvm.aliasText=aliasInput.Text.TrimStart(' ').TrimEnd(' ').ToUpper();

            //escludi dal nome utente tutti i caratteri non alphanumerici
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            App.sbvm.aliasText = rgx.Replace(App.sbvm.aliasText, "");


            if (sportPicker.SelectedIndex > -1)
            {
                App.sbvm.sport = App.sports[sportPicker.SelectedIndex];
                Preferences.Set("sport", App.sbvm.sport.name); //salva le impostazioni
            }
            else
            {
                App.DisplayAlert(AppResources.sport_required);
                return;
            }
            Preferences.Set("alias", aliasInput.Text); //salva le impostazioni
            Preferences.Set("imagecover_" + aliasInput.Text + "_" + App.sbvm.sport.name, App.image_cover);


            //Invia l'immagine di attesa al LEDbox
            App.UploadImageCoverToLedbox();

            //ricarica il file
            App.storage.readFile();

            await Navigation.PopModalAsync();
           
        }

        void setSportList()
        {
            sportPicker.Items.Clear();
            if(App.sports!=null && App.sports.Count>0)
                foreach (sport s in App.sports) {
                    sportPicker.Items.Add(s.getCurrentLanguage());
                }

        }

        /// <summary>
        /// Verify the list online and download the file
        /// </summary>
        /// <param name="onComplete"></param>
        void checkSportListOnline(Action<bool> onComplete)
        {

            if (App.isInternetConnection()){ 

                //DependencyService.Get<IMessage>().LongAlert(AppResources.download_list);

                App.webservice.DownloadSportsList((file_to_download) => {

                    if (file_to_download != "")
                    {
                        StreamReader streamReader = new StreamReader(file_to_download);
                        string sport_streamer = streamReader.ReadToEnd();
                        List<sport> sports_online = helper.parseSportJson(sport_streamer);
                        if(sports_online!=null)
                            App.sports = sports_online;

                        //copia il contenuto all'interno del file sport.json
                        string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                        string file_fullpath = directory + "/sports.json";
                        StreamWriter streamWriter = new StreamWriter(file_fullpath);
                        streamWriter.Write(sport_streamer);
                        streamWriter.Close();




                    }
                    onComplete(true);
                });
            }
            else
            {
                //se internet non è disponibile
                onComplete(true);
            }
        }


        void SelectImage(object sender, System.EventArgs e)
        {
            helper.AddPhoto((path) =>
            {
                if (path != "")
                {
                    App.image_cover = path;
                    imageCover.Source = App.image_cover;
                }
            });
        }


    }
}
