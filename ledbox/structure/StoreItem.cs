using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using ledbox.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ledbox
{
    public class StoreItem
    {

        public const string PERMISSION_ALL= "all";
        public const string PERMISSION_ONLY_ADMIN = "only_admin";


        public string name { get; set; }
        public string description { get; set; }
        public string permission { get; set; }
        public string allow_connection { get; set; } //tipologia di connessioni accettate (all, lan, bluetooth, usb)
        public string file { get; set; }
        public float version { get; set; }
        public DateTime date { get; set; }
        public string sport { get; set; }
        public string access { get; set; }
        public string remote_file { get; set; }
        public string image { get; set; }
        public bool preinstall { get; set; }
        public string category { get; set; }
        public string executive_directory { get; set; }

       

        public string language {
            set {

                try
                {
                    JArray languages = JsonConvert.DeserializeObject<JArray>(value);


                   
                    this.languages = new List<store_language>();
                    foreach (JObject jolanguage in languages)
                    {
                        store_language lang = new store_language();
                        lang.language = jolanguage["tag"].ToString();
                        lang.title = jolanguage["title"].ToString();
                        lang.description = jolanguage["description"].ToString();

                        this.languages.Add(lang);
                    }

                   

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                getCurrentLanguage();



            }
        }




        public string label_title { get; set; }


        public string label_description { get; set; }


        List<store_language> languages;

        [JsonIgnore]
        public string zipfile;

        [JsonIgnore]
        public string zipfile_path;

        [JsonIgnore]
        public string local_file;

        [JsonIgnore]
        public string Version { get { return AppResources.version + " " + version.ToString(); } }



        [JsonIgnore]
        public string directory
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), relative_directory);


            }
        }


        [JsonIgnore]
        public string relative_directory
        {
            get
            {

                string result;

                switch (category.ToLower())
                {
                    case "interface":
                        result = App.DIRECTORY_INTERFACES;
                        break;
                    case "practice":
                        result = App.DIRECTORY_PRACTICES;
                        break;
                    case "plugin":
                        result = App.DIRECTORY_PLUGINS;
                        break;
                    default:
                        result = category;
                        break;
                }

                return result;

                //return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.DIRECTORY_INTERFACES);


            }
        }


        public store_language getCurrentLanguage()
        {
            try
            {
                foreach (store_language language in languages)
                {

                    if (language.language.Contains(Preferences.Get("language", "it-IT")))
                    {
                        this.label_title = language.title;
                        this.label_description = language.description;
                        return language;
                    }

                }
            }
            catch
            {

            }


            store_language sl = new store_language();
            sl.title = this.name;
            sl.description = this.description;
            this.label_title = sl.title;
            this.label_description = sl.description;


            return sl;
        }

        /// <summary>
        /// Scarica ed installa l'item dall store
        /// </summary>
        /// <param name="onFinish">restituisce il path del file manifest contenuto all'interno dello zip</param>
        public void download_and_install(Action<string> onFinish)
        {
            DependencyService.Get<IMessage>().LongAlert(AppResources.downloading);


            //verifica se il file è già stato scaricato
            if (File.Exists(directory + "/" + file))
            {
                File.Delete(directory + "/" + file);
            }

            //scarica il file
            helper.DownloadFile(remote_file, directory, (path) =>
            {


                if (path == "")
                {
                    onFinish("");
                    return;
                }

                string out_name = App.sport.name + "_" + name.Replace(" ","_");

                //decomprimi il file
                helper.ExtractZipFile(path, "", relative_directory + "/" + out_name);

                //copia il file zip all'interno della cartella
                DependencyService.Get<IDirectory>().copyFile(path, directory + "/" + out_name + "/" + file, true, true);

                //trova il manifest dall'interno della cartella
                string manifest = directory + "/" + out_name + "/manifest.json";

                if (File.Exists(manifest))
                {

                    //avvia l'installazione a seconda della categoria
                    switch (this.category)
                    {
                        case "practice":
                            installPractice(manifest);
                            break;
                    }

                    onFinish(manifest);

                }
                else
                    onFinish("");







            });

        }


        public (StoreItem, bool) verify_old_version(List<StoreItem> items)
        {
            foreach (StoreItem storeItem in items)
            {
                if (storeItem.sport == this.sport && storeItem.name == this.name)
                {
                    if (storeItem.version <= this.version)
                    {
                        return (storeItem, true);
                    }
                }
            }
            return (null, false);
        }


        public void disinstall()
        {
            //trova la cartella dell'interfaccia
            string directory = Path.GetDirectoryName(local_file);

            //elimina la cartella
            Directory.Delete(directory, true);
        }


        void installPractice(string path_manifest)
        {
            string path = path_manifest.Replace("manifest.json", "");
            StreamReader stream = new StreamReader(path_manifest);

            string file_stream = stream.ReadLine();
            if (file_stream != "")
            {
                try
                {
                    JObject manifest = JsonConvert.DeserializeObject<JObject>(file_stream);
                    JArray items = (JArray)manifest["items"];

                    //crea una nuova sequenza di esercizi
                    Practice practice = new Practice();
                    practice.Title = manifest["name"].ToString();
                    practice.Items = new List<ItemPractice>();

                    foreach (JObject item in items)
                    {
                        ItemPractice itemPractice = new ItemPractice();
                        itemPractice.title = "";
                        itemPractice.round = item["rounds"].ToObject<int>();
                        itemPractice.work = item["work"].ToObject<int>();
                        itemPractice.rest = item["rest"].ToObject<int>();
                        itemPractice.filename = item["file"].ToString();
                        itemPractice.filepath = path + item["file"].ToString();

                        practice.Items.Add(itemPractice);

                    }


                    App.storage.addPractice(practice);
                    App.storage.saveFile();
                }
                catch
                {
                    Console.Write("Error install Practice");
                }


            }
        }
    }
}
