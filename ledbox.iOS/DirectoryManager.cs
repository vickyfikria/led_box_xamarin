using System;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CoreGraphics;
using Foundation;
using ledbox.iOS;
using UIKit;
using Xamarin.Forms;

//[assembly: Dependency(typeof(ledbox.iOSUnified.iOS.DependencyServices.IPAddressManager))]
[assembly: Dependency(typeof(DirectoryManager))]

namespace ledbox.iOS
{
    class DirectoryManager : IDirectory
    {
        public string getBundleDirectory()
        {
            return Path.Combine(NSBundle.MainBundle.BundlePath, "Content");

        }

        //aggiunto da verificare
        public void copyFile(string fileAsset, string directory)
        {

            File.Copy(getBundleDirectory() + "/" + fileAsset, directory + "/" + fileAsset, true);

        }

        public bool copyFile(string fileIn, string fileOut, bool moving = false, bool absolute_path = false)
        {
            System.IO.Stream source;
            string pathfilein;

            try
            {
                if (absolute_path)
                {
                    source = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
                    pathfilein = fileIn;
                }
                else
                {
                    pathfilein = Path.Combine(NSBundle.MainBundle.BundlePath, string.Format("Content/{0}", fileIn));
                    source = new FileStream(pathfilein, FileMode.Open, FileAccess.Read);
                    //source = Android.App.Application.Context.Assets.Open("Content/" + fileIn);
                    
                }


                if (System.IO.File.Exists(fileOut))
                    System.IO.File.Delete(fileOut);

                using (FileStream dest = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write))
                {


                    source.CopyTo(dest);
                    source.Close();
                    if (moving)
                        System.IO.File.Delete(pathfilein);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public (byte[], int) GenerateThumbImage(string url, long usecond)
        {
            throw new NotImplementedException();
        }

        public int GetDurationMp3(string url)
        {
            throw new NotImplementedException();
        }

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {

            NSData datos = NSData.FromArray(imageData);
            UIImage sourceImage = new UIImage(datos);

            var sourceSize = sourceImage.Size;
            var maxResizeFactor = Math.Min(width / sourceSize.Width, height / sourceSize.Height);
            if (maxResizeFactor > 1) return imageData;
            var newWidth = maxResizeFactor * sourceSize.Width;
            var newHeight = maxResizeFactor * sourceSize.Height;


            UIGraphics.BeginImageContext(new CGSize(newWidth, newHeight));
            sourceImage.Draw(new CGRect(0, 0, newWidth, newHeight));
            var resultImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            NSData resultImageData = resultImage.AsPNG();

            byte[] myByteArray = new Byte[resultImageData.Length];
            System.Runtime.InteropServices.Marshal.Copy(resultImageData.Bytes, myByteArray, 0, Convert.ToInt32(resultImageData.Length));
            
            return myByteArray;
        }
     


    }
}