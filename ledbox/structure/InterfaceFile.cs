using System;
using System.IO;
using System.Net.Http;
using ledbox.Resources;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public class InterfaceFile: StoreItem
    {
        /*
        public string name { get; set; }
        public string description { get; set; }
        public string file { get; set; }
        public float version { get; set; }
        public DateTime date { get; set; }
        public string sport { get; set; }
        public string access { get; set; }
        public string remote_file { get; set; }
        public string image { get; set; }       
        public bool preinstall { get; set; }

        //[JsonIgnore]
        //public ImageSource imageUri
        //{
        //    get {return ImageSource.FromUri(new Uri(image));}
        //    set { imageUri = value; }
        //}

        [JsonIgnore]
        public string zipfile;

        [JsonIgnore]
        public string zipfile_path;

        [JsonIgnore]
        public string local_file;

        [JsonIgnore]
        public string Version { get { return AppResources.version + " " + version.ToString(); } }

        public void download_and_install(Action<bool> onFinish)
        {
            DependencyService.Get<IMessage>().LongAlert(AppResources.downloading);

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.DIRECTORY_INTERFACES);


            //verifica se il file è già stato scaricato
            if (File.Exists(directory + "/" + file))
            {
                File.Delete(directory + "/" + file);
            }

            //scarica il file
            helper.DownloadFile(remote_file, directory, (path) => {

            //    helper.DownloadFile(webservice.URL_STOREONLINE +"interfaces/"+App.sport.name+"/"+file, directory, (path) => {

                if (path == "")
                {
                    onFinish(false);
                    return;
                }

                //decomprimi il file
                helper.ExtractZipFile(path, "", App.DIRECTORY_INTERFACES + "/" + name);

                DependencyService.Get<IDirectory>().copyFile(path, directory + "/" + name+"/"+file, true,true);


                onFinish(true);
            });

        }


        public (InterfaceFile,bool) verify_old_version()
        {
            foreach(InterfaceFile interfaceFile in App.interfaceFiles)
            {
                if(interfaceFile.sport==this.sport && interfaceFile.name == this.name)
                {
                    if (interfaceFile.version <= this.version)
                    {
                        return (interfaceFile,true);
                    }
                }
            }
            return (null,false);
        }


        public void disinstall()
        {
            //trova la cartella dell'interfaccia
            string directory = Path.GetDirectoryName(local_file);

            //elimina la cartella
            Directory.Delete(directory, true);
        }
        */




    }
}
