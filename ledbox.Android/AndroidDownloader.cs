using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using ledbox.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(AndroidDownloader))]


namespace ledbox.Droid
{

   
    public class AndroidDownloader : IDownloader
    {
        public event EventHandler<DownloadEventArgs> OnFileDownloaded;
        public event EventHandler<int> OnFileDownloading;

        private WebClient webClient;

        public string DownloadFile(string url, string folder)
        {
            string pathToNewFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, folder);
            Directory.CreateDirectory(pathToNewFolder);

            try
            {
                webClient = new WebClient();
               
               

                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                string pathToNewFile = Path.Combine(pathToNewFolder, Path.GetFileName(url));
                if (File.Exists(pathToNewFile))
                    File.Delete(pathToNewFile);


                
                webClient.DownloadFileAsync(new Uri(url), pathToNewFile);

                return pathToNewFile;
            }
            catch (Exception ex)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
                Console.WriteLine(ex.ToString());
            }

            return "";
        }


        public void CancelDownload()
        {
            if (webClient != null)
            {
                webClient.CancelAsync();
                webClient.Dispose();

            }
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(false));
            }
            else
            {
                if (OnFileDownloaded != null)
                    OnFileDownloaded.Invoke(this, new DownloadEventArgs(true));
            }
        }


        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {

            OnFileDownloading.Invoke(this, e.ProgressPercentage);

            // Displays the operation identifier, and the transfer progress.
            Console.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
                (string)e.UserState,
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }
    }
}
