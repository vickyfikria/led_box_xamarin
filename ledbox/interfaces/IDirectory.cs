using System;
using Xamarin.Forms;

namespace ledbox
{
    public interface IDirectory
    {

         string getBundleDirectory();
        int GetDurationMp3(string url);
        byte[] ResizeImage(byte[] imageData, float width, float height);
         bool copyFile(string fileIn, string fileOut, bool moving = false, bool absolute_path = false);
        (byte[], int) GenerateThumbImage(string url, long usecond);

        string GetPathFromUri(string url, string filename);



       }
}
