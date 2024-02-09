using System;
using System.IO;
using System.Net.Http;
using ledbox.Resources;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public class PracticePreset
    {

        public string title { get; set; }
        public string file { get; set; }
      

        public string File {
            get {
                if (file == "" || file==null)
                    return "";

                string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return directory +"/"+ App.DIRECTORY_PRACTICE_PRESET + "/" + file;

            }

        }
    }
}
