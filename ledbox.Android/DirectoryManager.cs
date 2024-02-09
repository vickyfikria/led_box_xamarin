using System;
using ledbox.Droid;
using System.Net;
using Xamarin.Forms;
using Android.Net.Wifi;
using Android.App;
using Android.Text.Format;
using System.IO;
using Android.Content.Res;
using Android.Media;
using System.Collections.Generic;
using Android.Graphics;
using Java.IO;
using Android.OS;
using Android.Provider;
using Android.Content;
using Android.Database;
using Android.Text;

[assembly: Dependency(typeof(DirectoryManager))]

namespace ledbox.Droid
{
    class DirectoryManager : IDirectory
    {
        public string getBundleDirectory()
        {
          
            return "";
        }

        public void deleteFile(string fileAsset,string directory)
        {
            System.IO.File.Delete(directory + "/" + fileAsset);

        }

        public bool copyFile(string fileIn,string fileOut,bool moving=false,bool absolute_path=false)
        {
            System.IO.Stream source;
            string p;

            try
            {
                if (absolute_path)
                {
                    source = new FileStream(fileIn, FileMode.Open, FileAccess.Read);
                    p = fileIn;
                }
                else
                {
                

                    source = Android.App.Application.Context.Assets.Open("Content/" + fileIn);
                    p = "Content/" + fileIn;
                }

            
                if (System.IO.File.Exists(fileOut))
                    System.IO.File.Delete(fileOut);

                using (FileStream dest = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.Write))
                {
                   

                    source.CopyTo(dest);
                    source.Close();
                    if (moving)
                        System.IO.File.Delete(p);
                }
            }
            catch 
            {
                return false;
            }

            return true;
           
        }

        [Obsolete]
        public (byte[],int) GenerateThumbImage(string url, long usecond)
        {
            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            FileInputStream inputStream = new FileInputStream(url);
            retriever.SetDataSource(inputStream.FD);

            string time = retriever.ExtractMetadata(MediaMetadataRetriever.MetadataKeyDuration);
            int timeInSec = (int.Parse(time)/1000);

            Bitmap bitmap = retriever.GetFrameAtTime(usecond);
            if (bitmap != null)
            {
                MemoryStream stream = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                byte[] bitmapData = stream.ToArray();
                return (bitmapData,timeInSec);
            }
            return (null,0);
        }

        [Obsolete]
        public int GetDurationMp3(string url)
        {

            Uri uri = new Uri(url);
           


            MediaMetadataRetriever retriever = new MediaMetadataRetriever();
            FileInputStream inputStream = new FileInputStream(url);
            retriever.SetDataSource(inputStream.FD);

            string time = retriever.ExtractMetadata(MediaMetadataRetriever.MetadataKeyDuration);
         

            int timeInSec = (int.Parse(time) / 1000);

           

            return timeInSec;
        }

        /*
        public string GetPathFromUri(string url)
        {
            Android.Content.Context context = Android.App.Application.Context;
            var activity = (MainActivity)Forms.Context;

            Android.Net.Uri uri = Android.Net.Uri.Parse(url);
            string doc_id = "";
            using (var c1 = context.ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                string document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = context.ContentResolver.Query(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;


           /*

            Android.Net.Uri.Builder uri = new Android.Net.Uri.Builder();
            uri.EncodedPath(url);
            

            

           
            string doc_id = "";
            using (var c1 = context.ContentResolver.Query(uri.Build(), null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = activity.ManagedQuery(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
            */
        //}

        public byte[] ResizeImage(byte[] imageData, float width, float height)
        {
            // Load the bitmap 
            BitmapFactory.Options options = new BitmapFactory.Options();// Create object of bitmapfactory's option method for further option use
            options.InPurgeable = true; // inPurgeable is used to free up memory while required
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length, options);

            float newHeight = 0;
            float newWidth = 0;

            var originalHeight = originalImage.Height;
            var originalWidth = originalImage.Width;

            if (originalHeight > originalWidth)
            {
                newHeight = height;
                float ratio = originalHeight / height;
                newWidth = originalWidth / ratio;
            }
            else
            {
                newWidth = width;
                float ratio = originalWidth / width;
                newHeight = originalHeight / ratio;
            }

            Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, true);

            // originalImage.Recycle();

            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Png, 100, ms);

                resizedImage.Recycle();

                return ms.ToArray();
            }

        }




        public string GetPathFromUri(string url,string filename)
        {
            Android.Content.Context context = Android.App.Application.Context;
            var activity = (MainActivity)Forms.Context;

            Android.Net.Uri uri = Android.Net.Uri.Parse(url);

       

            bool isKitKat = Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat;

            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider
                if (isExternalStorageDocument(uri))
                {
                    string docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    string[] split = docId.Split(chars);
                    string type = split[0];

                    if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }
                }
                // DownloadsProvider
                else if (isDownloadsDocument(uri))
                {
                    return global::Android.OS.Environment.ExternalStorageDirectory.ToString() + "/Download/" + filename;
                   
                }
                // MediaProvider
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);

                    char[] chars = { ':' };
                    String[] split = docId.Split(chars);

                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[]
                    {
                    split[1]
                    };

                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)
            else if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {

                // Return the remote address
                if (isGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return getDataColumn(context, uri, null, null);
            }
            // File
            else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                return uri.Path;
            }

            return null;
        }

        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection, String[] selectionArgs)
        {
            ICursor cursor = null;
            String column = "_data";
            String[] projection =
            {
            column
        };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }

        //Whether the Uri authority is ExternalStorageProvider.
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is DownloadsProvider.
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is MediaProvider.
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        //Whether the Uri authority is Google Photos.
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }
    }

}