using System;
namespace ledbox
{
    public interface IDownloader
    {
        string DownloadFile(string url, string folder);
        event EventHandler<DownloadEventArgs> OnFileDownloaded;
        event EventHandler<int> OnFileDownloading;
        void CancelDownload();
    }

    public class DownloadEventArgs : EventArgs
    {
        public bool FileSaved = false;
        public DownloadEventArgs(bool fileSaved)
        {
            FileSaved = fileSaved;
        }
    }

    

}
