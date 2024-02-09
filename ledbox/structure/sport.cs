using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace ledbox
{
    public class sport
    {
        [JsonProperty("name")]
        public string name;

       

        [JsonProperty(PropertyName ="languages",TypeNameHandling =TypeNameHandling.Arrays)]
        public IList<sport_language> languages;

        private string title;
       
        public string Title { get {
                title=getCurrentLanguage();
                return title;
                 }
            set
            {
                title = value;
            }
        }


        
        public string getCurrentLanguage()
        {
            foreach(sport_language language in languages)
            {
           
                if (language.language.Contains(Preferences.Get("language", "it-IT")))
                    return language.value;
            }

            return "";
        }

        public void reloadLanguage()
        {
            Title = getCurrentLanguage();
        }

    }
}
