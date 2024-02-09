using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public class webservice
    {


        public static string URL_API = "http://ledbox.tech4sport.com/service.php?auth=YXBwOkYwcURUY1h3TW5IOG56bjc=&";

     
        public const string TASK_GETSTORE = "task=getStore";
        //public const string TASK_FILE_INTERFACES_ONLINE = "task=getInterfaces";
        public const string TASK_FILE_SPORTS_ONLINE = "task=getSports";
        public const string TASK_FILE_LASTUPDATE_LEDBOX = "task=getUpdates";
        public const string TASK_UPGRADE_LEDBOX = "task=setUpgrade";
        public const string TASK_GETPRESETPRACTICES = "task=getStoreItems&id_category=1";


        public const int INTERFACE_CATEGORY = 3;
        public const int PRACTICE_CATEGORY = 1;
        public const int PRACTICE_PRESET_CATEGORY = 4;
        public const int PLUGIN_CATEGORY = 5;

        public struct response
        {
            public string status;
        }

        public struct updateLedbox
        {
            public float version;
            public string file;
            public string date;
            public int size;
        }


        public webservice()
        {
            string test = "";
            if (App.isTestingMode)
                test = "test=true&";

            URL_API = URL_API + test;
        }


        HttpClient getHttpClient()
        {
            HttpClient client = new HttpClient();
            return client;
        }

        public async Task<List<StoreItem>> GetStore(string sport,int id_category)
        {


            try
            {

                

                var client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 5);
                string json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_GETSTORE + "&sport=" + sport+"&id_category="+id_category.ToString()));
                if (json != "")
                    return JsonConvert.DeserializeObject<List<StoreItem>>(json.ToString());
                else
                    return null;
            }
            catch (System.Exception exception)
            {
                Console.Write("Error GetStore " + exception.ToString());
                return null;
            }

        }

        public async Task<List<StoreItem>> GetStorePreInstall(string sport)
        {
            try
            {
                var client = new HttpClient();
                var json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_GETSTORE + "&sport=" + sport + "&preinstall=1"));
                return JsonConvert.DeserializeObject<List<StoreItem>>(json.ToString());
            }
            catch (System.Exception exception)
            {
                Console.Write("Error GetStore " + exception.ToString());
                return null;
            }

        }

        /*
        public async Task<List<InterfaceFile>> GetInterfaces(string sport)
        {
            try
            {
                var client = new HttpClient();
                var json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_FILE_INTERFACES_ONLINE+"&sport="+sport));
                return JsonConvert.DeserializeObject<List<InterfaceFile>>(json.ToString());
            }
            catch (System.Exception exception)
            {
                Console.Write("Error getinterfaces "+exception.ToString());
                return null;
            }

        }*/


        public async Task<List<sport>> GetSports()
        {
            try
            {
                var client = new HttpClient();
                string path = webservice.URL_API + webservice.TASK_FILE_SPORTS_ONLINE;
                var json = await client.GetStringAsync(string.Format(path));
                return helper.parseSportJson(json.ToString());
            }
            catch (System.Exception exception)
            {
                Console.Write("Error GetSports " + exception.ToString());
                return null;
            }

        }

        public void DownloadSportsList(Action<string> file_to_download)
        {
            string path = webservice.URL_API + webservice.TASK_FILE_SPORTS_ONLINE;

            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            helper.DownloadFile(path, directory, (onFinish) => {
                file_to_download(onFinish);
            });
        }

        public async Task<updateLedbox> GetLastUpdateLedbox()
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                var json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_FILE_LASTUPDATE_LEDBOX));
                return JsonConvert.DeserializeObject<updateLedbox>(json.ToString());
            }
            catch (System.Exception exception)
            {
                Console.Write("Error getLastUpdateLedbox " + exception.ToString());
                return new updateLedbox();
            }

        }
        public void DownloadLastUpdateLedbox(string url,Action<string> file_to_download)
        {
            
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            helper.DownloadFile(url, directory, (onFinish) => {
                file_to_download(onFinish);
            });
        }


        /// <summary>
        /// Imposta su LEDbox management che il device ha avuto un aggiornamento software
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SetDeviceUpgraded(string serialnumber,string version_sw)
        {
            try
            {
                var client = new HttpClient();
                var json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_UPGRADE_LEDBOX+"&serialnumber="+serialnumber+"&version_sw="+version_sw));
                response resp= JsonConvert.DeserializeObject<response>(json.ToString());
                return resp.status == "OK" ? true : false;
            }
            catch (System.Exception exception)
            {
                Console.Write("Error upgrade software LEDbox " + exception.ToString());
                return false;
            }

        }


        /// <summary>
        /// Scarica il file zip con i preset delle practice
        /// </summary>
        public async void DownloadLastPracticesPreset(Action<string> listPresetPractice)
        {
            string url = "";


            List<StoreItem> storeItems = await GetStore(App.sport.name, PRACTICE_PRESET_CATEGORY);

            if (storeItems==null || storeItems.Count == 0)
            {
                listPresetPractice("");
                return;

            }
            StoreItem storeItem = storeItems[storeItems.Count-1];

            url = storeItem.remote_file;

            /*
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                var json = await client.GetStringAsync(string.Format(webservice.URL_API + webservice.TASK_GETSTORE + "&sport=" + App.sport + "&id_category=" + PRACTICE_PRESET_CATEGORY));
                url = JsonConvert.DeserializeObject<string>(json.ToString());

                
            }
            catch (System.Exception exception)
            {
                Console.Write("Error download practice preset " + exception.ToString());
                
            }*/


            bool toLocal = true;
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            if (url != "")
            {
                //verifica se scaricare il file
                string[] spliturl = url.Split('/');
                string filenamezip = spliturl[spliturl.Length - 1];
                if (!File.Exists(directory + "/" + filenamezip))
                {
                    toLocal = false;
                }


            }

            if(toLocal)
                if (File.Exists(directory + "/"+App.DIRECTORY_PRACTICE_PRESET + "/files.json"))
                {
                    StreamReader stream = new StreamReader(directory + "/" + App.DIRECTORY_PRACTICE_PRESET + "/files.json");
                    string file_content = stream.ReadToEnd();
                    stream.Close();

                    listPresetPractice(file_content);
                    return;
                }
            
            
            if(url!="")
                helper.DownloadFile(url, directory, (path) => {

                    //decomprimi il file
                    helper.ExtractZipFile(path, "", App.DIRECTORY_PRACTICE_PRESET);
                    string pathjson = directory + "/" + App.DIRECTORY_PRACTICE_PRESET + "/files.json";
                    if (File.Exists(pathjson))
                    {
                        //mostra il file decompresso
                        StreamReader stream = new StreamReader(pathjson);
                        string file_content = stream.ReadToEnd();
                        stream.Close();

                        listPresetPractice(file_content);
                    }

                });
        }



    }
}
