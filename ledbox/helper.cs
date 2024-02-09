using System;
using System.IO;
//using Plugin.ImageResizer;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;
using System.Text;
using ledbox.Resources;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Acr.UserDialogs;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
//using Xamarians.CropImage;

namespace ledbox
{
    public class helper
    {


        public helper()
        {
        }

        public static async void AddPhoto(Action<string> result)
        {

            string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string path = "";
            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Console.WriteLine("Error picking image file");
                return;
            }

            PickMediaOptions option = new PickMediaOptions();
            

            MediaFile file = await CrossMedia.Current.PickPhotoAsync(option);
            if (file != null)
            {
                try
                {
                    path = dir + "/" + Path.GetFileName(file.Path);
                    //verifica se si tratta di una GIF
                    if (Path.GetExtension(file.Path).ToLower() == ".gif")
                    {
                        //copia il file
                        File.Copy(file.Path, path);
                        result(path);
                        return;
                    }

                    byte[] imageByte = File.ReadAllBytes(file.Path);

                    //byte[] imageByteResized = await CrossImageResizer.Current.ResizeImageWithAspectRatioAsync(imageByte, 192, 64);

                    byte[] imageByteResized = DependencyService.Get<IDirectory>().ResizeImage(imageByte, 192, 64);
                   
                    FileStream streamer = File.OpenWrite(path);
                    streamer.Write(imageByteResized, 0, imageByteResized.Length);
                    streamer.Close();

                    result(path);
                    return;
                }
                catch
                {
                    DependencyService.Get<IMessage>().ShortAlert(AppResources.error_image);
                    return;
                }

            }

            result("");

        }

       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public static async void AddVideo(Action<string,string,int> result)
        {
            string dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string path = "";

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Console.WriteLine("Error");
                return;
            }


            MediaFile file = await CrossMedia.Current.PickVideoAsync();
            if (file != null)
            {
                try
                {
                    (byte[] imageByte,int duration) = DependencyService.Get<IDirectory>().GenerateThumbImage(file.Path, 0);
                    path = dir + "/thumb_" + Path.GetFileName(file.Path);


                    FileStream streamer = File.OpenWrite(path);
                    streamer.Write(imageByte, 0, imageByte.Length);
                    streamer.Close();

                    result(file.Path,path, duration);
                }
                catch (Exception e)
                {
                    Console.Write("error addvideo " + e.ToString());
                    DependencyService.Get<IMessage>().ShortAlert(AppResources.error_image);
                    return;
                }

            }
           

            result("","",0);

        }


        public static void DownloadFile(string url,string directory,Action<string> onFinish)
        {
            IDownloader downloader = DependencyService.Get<IDownloader>();
            string path="";
            Device.BeginInvokeOnMainThread(() =>
            {
                var progress = UserDialogs.Instance.Progress("Download", () =>
                {
                    downloader.CancelDownload();
                    onFinish("");
                }, AppResources.cancel, true, null);
            

                downloader.OnFileDownloading += (object s1, int percentReceived) => {
                    progress.PercentComplete = percentReceived;
                };
            

                EventHandler<DownloadEventArgs> handler = null;

                handler= (object sender, DownloadEventArgs e) => {
                    progress.Hide();
                    if (e.FileSaved)
                    {

                        if (File.Exists(path))
                        {
                            onFinish(path);
                        }
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(AppResources.error_download);
                        onFinish("");
                    }


                    downloader.OnFileDownloaded -= handler;



                };
           

                downloader.OnFileDownloaded += handler;

                path = downloader.DownloadFile(url, directory);
            });

            

        }


        public static bool CompressZipFile(string fullZipFileName, Action<bool> onComplete, params string[] fullFileName )
        {
            try { 
                using (FileStream fs = new FileStream(fullZipFileName, FileMode.OpenOrCreate, FileAccess.Write,FileShare.None))
                {
                    using (ZipOutputStream zs = new ZipOutputStream(fs))
                    {
                        foreach (var file in fullFileName)
                        {
                            string fileName = Path.GetFileName(file);

                            ZipEntry zipEntry = new ZipEntry(fileName);
                            zs.PutNextEntry(zipEntry);
                            byte[] fileContent = System.IO.File.ReadAllBytes(file);
                            zs.Write(fileContent,0,fileContent.Length);
                            zs.CloseEntry();
                        }

                        zs.Close();
                    }
                    fs.Close();
                }
                onComplete(true);
                return true;
            }
            catch
            {
                DependencyService.Get<IMessage>().ShortAlert(AppResources.error_zip);
                onComplete(false);
                return false;
                
            }
        }


        public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;

            outFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), outFolder);

            if (Directory.Exists(outFolder))
                Directory.Delete(outFolder, true);

            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            catch
            {
                
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }



        
          

        static public string ConvertHexToRGB(string hex)
        {

            string r_hex = hex.Substring(1, 2);
            string g_hex = hex.Substring(3, 2);
            string b_hex = hex.Substring(5, 2);

            int r = int.Parse(r_hex, System.Globalization.NumberStyles.HexNumber);
            int g = int.Parse(g_hex, System.Globalization.NumberStyles.HexNumber);
            int b = int.Parse(b_hex, System.Globalization.NumberStyles.HexNumber);

            return r.ToString()+","+g.ToString()+","+b.ToString();

        }


        public static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        public static byte[] CompressString(string str)
        {


            var bytes = Encoding.Default.GetBytes(str);
            //var bytes = Encoding.ASCII.GetBytes(str);

           

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new System.IO.Compression.GZipStream(mso, System.IO.Compression.CompressionMode.Compress))
                {
                    //msi.CopyTo(gs);
                    CopyTo(msi, gs);
                }

                return mso.ToArray();
            }
        }

        public static string UnCompressString(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new System.IO.Compression.GZipStream(msi, System.IO.Compression.CompressionMode.Decompress))
                {
                    //gs.CopyTo(mso);
                    CopyTo(gs, mso);
                }

                string result= Encoding.ASCII.GetString(mso.ToArray());
                return result;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


        public static string SecondToMinute(int seconds)
        {


            TimeSpan t = TimeSpan.FromSeconds(seconds);

            return t.Minutes.ToString("D2") + ":" + t.Seconds.ToString("D2");
          
        }

        public static long  ToTimestamp(DateTime date)
        {
            var dateTimeOffset = new DateTimeOffset(date);
            var unixDateTime = dateTimeOffset.ToUnixTimeSeconds();
            return unixDateTime;
        }


        public static List<sport> parseSportJson(string json)
        {

            List<sport> sports = new List<sport>();
            JObject jo = null;
            try
            {
                jo = JsonConvert.DeserializeObject<JObject>(json);
            }
            catch
            {
                return null;
            }
            
            if (jo == null)
                return null;

            JArray josports = jo["sports"].ToObject<JArray>();


            foreach (JObject joitem in josports)
            {
                sport s = new sport();
                s.name = joitem["name"].ToString();
                s.languages = new List<sport_language>();

                JArray languages = joitem["languages"].ToObject<JArray>();

                foreach (JObject jolanguage in languages)
                {
                    sport_language sport_Language = new sport_language();
                    sport_Language.language = jolanguage["language"].ToString();
                    sport_Language.value = jolanguage["value"].ToString();
                    s.languages.Add(sport_Language);
                }

                sports.Add(s);
                
            }

            return sports;
        }

        /*
        public static List<StoreItem> updateListStore(string relative_directory)
        {

            List<StoreItem> storeItems= new List<StoreItem>();

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), relative_directory);

            //esplora le cartelle
            DirectoryInfo d = new DirectoryInfo(directory);//Assuming Test is your Folder

            if(d.Exists)
                foreach (DirectoryInfo dir in d.GetDirectories())
                {
                
                        //prendi il file manifest
                        string manifest = dir.FullName + "/manifest.json";


                        if (File.Exists(manifest))
                        {
                            string zipfile = "";
                            string zipfile_path = "";

                            //trova il file zip
                            foreach (FileInfo f in dir.GetFiles())
                            {
                                if (f.Extension == ".zip")
                                {
                                    zipfile_path = f.FullName;
                                    zipfile = f.Name;
                                }
                            }
                            StreamReader stream = new StreamReader(manifest);
                            string file_stream = stream.ReadLine();
                            if (file_stream != "")
                            {
                                try
                                {
                                    StoreItem storeitem_obj = JsonConvert.DeserializeObject<StoreItem>(file_stream);
                                    //verifica se appartiene allo sport selezionato
                                    if (storeitem_obj.sport == App.sport.name)
                                    {
                                        storeitem_obj.zipfile = zipfile;
                                        storeitem_obj.zipfile_path = zipfile_path;

                                        storeItems.Add(storeitem_obj);

                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.Write("Error list store " + e.ToString());
                                    DependencyService.Get<IMessage>().ShortAlert(AppResources.error_read + " " + dir.Name);
                                }
                        
                        }
                    }
                }
            return storeItems;
        }*/


        public static List<StoreItem> updateListStore(string relative_directory,string category)
        {
            List<StoreItem> storeItems = new List<StoreItem>();

            App.interfaceFiles = new List<StoreItem>();

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), relative_directory);

            //esplora le cartelle
            DirectoryInfo d = new DirectoryInfo(directory);//Assuming Test is your Folder
            if(d.Exists)
                foreach (DirectoryInfo dir in d.GetDirectories())
                {
                    //entra nella cartella e prendi il file index.html
                    string file = dir.FullName + "/index.html";

                    if (File.Exists(file))
                    {

                        //prendi il file manifest
                        string manifest = dir.FullName + "/manifest.json";



                        if (File.Exists(manifest))
                        {
                            string zipfile = "";
                            string zipfile_path = "";

                            //trova il file zip
                            foreach (FileInfo f in dir.GetFiles())
                            {
                                if (f.Extension == ".zip")
                                {
                                    zipfile_path = f.FullName;
                                    zipfile = f.Name;
                                }
                            }
                            StreamReader stream = new StreamReader(manifest);
                            string file_stream = stream.ReadLine();
                            if (file_stream != "")
                            {
                                try
                                {
                                    StoreItem interface_obj = JsonConvert.DeserializeObject<StoreItem>(file_stream);
                                    //verifica se appartiene allo sport selezionato
                                    if (interface_obj.sport == App.sport.name)
                                    {
                                        interface_obj.local_file = file;
                                        interface_obj.zipfile = zipfile;
                                        interface_obj.zipfile_path = zipfile_path;
                                        interface_obj.category = category;
                                        interface_obj.executive_directory = dir.FullName;

                                        storeItems.Add(interface_obj);

                                    

                                    }
                                    interface_obj.getCurrentLanguage();
                                }
                                catch (Exception e)
                                {
                                    Console.Write("Error list interfaces " + e.ToString());
                                }
                            }
                        }
                    }
                }
            return storeItems;
        }

        /*
        public static void updateListInterfaces()
        {

            App.interfaceFiles = new List<StoreItem>();

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), App.DIRECTORY_INTERFACES);

            //esplora le cartelle
            DirectoryInfo d = new DirectoryInfo(directory);//Assuming Test is your Folder

            foreach (DirectoryInfo dir in d.GetDirectories())
            {
                //entra nella cartella e prendi il file index.html
                string file = dir.FullName + "/index.html";

                if (File.Exists(file))
                {

                    //prendi il file manifest
                    string manifest = dir.FullName + "/manifest.json";



                    if (File.Exists(manifest))
                    {
                        string zipfile = "";
                        string zipfile_path = "";

                        //trova il file zip
                        foreach (FileInfo f in dir.GetFiles())
                        {
                            if (f.Extension == ".zip")
                            {
                                zipfile_path = f.FullName;
                                zipfile = f.Name;
                            }
                        }
                        StreamReader stream = new StreamReader(manifest);
                        string file_stream = stream.ReadLine();
                        if (file_stream != "")
                        {
                            try
                            {
                                StoreItem interface_obj = JsonConvert.DeserializeObject<StoreItem>(file_stream);
                                //verifica se appartiene allo sport selezionato
                                if (interface_obj.sport == App.sport.name)
                                {
                                    interface_obj.local_file = file;
                                    interface_obj.zipfile = zipfile;
                                    interface_obj.zipfile_path = zipfile_path;
                                    interface_obj.category = "interface";
                                    interface_obj.executive_directory = dir.FullName;
                                    App.interfaceFiles.Add(interface_obj);

                                }
                            }
                            catch (Exception e)
                            {
                                Console.Write("Error list interfaces " + e.ToString());
                            }
                        }
                    }
                }
            }
        }*/


        public static async Task<string> CropImage(string filePath,int aspectX,int aspectY)
        {


            /*
            var cropResult = await CropImageService.Instance.CropImage(filePath, aspectX, aspectY);
            if (cropResult.IsSuccess)
            {
               return cropResult.FilePath;
            }*/

            return filePath;
        }


        public static string GetHash( string input)
        {
            SHA1 hashAlgorithm = SHA1.Create();
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static string validateIPAddress(string ipaddress)
        {
            IPAddress ip;
            bool ValidateIP = IPAddress.TryParse(ipaddress, out ip);
            if (ip != null)
                return ip.ToString();
            else
                return "";
           
        }


    }



    
}
