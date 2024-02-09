using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using PCLStorage;

namespace ledbox
{
    public class storage
    {

        public struct project
        {
            public string alias;
            public string sport;
            public DateTime creation;
            public DateTime lastmodified;

            public List<Playlist> playlists;
            public List<Practice> practices;
            public List<CustomText> customTexts;
            public string imageIntro;

        }



        public project current_project;



        public storage()
        {
           
        }



        public void addPlaylist(Playlist playlist)
        {
            if (current_project.playlists == null)
                current_project.playlists = new List<Playlist>();

            playlist.id = current_project.playlists.Count+1;
            current_project.playlists.Add(playlist);

        }


        public void editPlaylist(Playlist playlist,int id)
        {
            //cerca la playlist
            for(int i = 0; i < current_project.playlists.Count; i++)
            {
                if (current_project.playlists[i].id == playlist.id)
                {
                    current_project.playlists[i] = playlist;
                    return;
                }
            }
        }

        public Playlist getPlaylist( int id)
        {
            //cerca la playlist
            for (int i = 0; i < current_project.playlists.Count; i++)
            {
                if (current_project.playlists[i].id == id)
                {
                    return current_project.playlists[i];
                    
                }
            }

            return null;
        }

        public Playlist getPlaylist(string name)
        {
            //cerca la playlist
            for (int i = 0; i < current_project.playlists.Count; i++)
            {
                if (current_project.playlists[i].Title == name)
                {
                    return current_project.playlists[i];

                }
            }

            return null;
        }

        public void addPractice(Practice practice)
        {
            if (current_project.practices == null)
                current_project.practices = new List<Practice>();

            practice.id = current_project.practices.Count + 1;

            current_project.practices.Add(practice);

        }

        public void editPractice(Practice practice, int id)
        {
            //cerca la playlist
            for (int i = 0; i < current_project.practices.Count; i++)
            {
                if (current_project.practices[i].id == practice.id)
                {
                    current_project.practices[i] = practice;
                    return;
                }
            }
        }

        public Practice getPractice(int id)
        {
            //cerca la playlist
            for (int i = 0; i < current_project.practices.Count; i++)
            {
                if (current_project.practices[i].id == id)
                {
                    return current_project.practices[i];

                }
            }

            return null;
        }

        public Practice getPractice(string name)
        {
            //cerca la playlist
            for (int i = 0; i < current_project.practices.Count; i++)
            {
                if (current_project.practices[i].Title == name)
                {
                    return current_project.practices[i];

                }
            }

            return null;
        }

        public void addCustomText(CustomText customText)
        {
            if (current_project.customTexts == null)
                current_project.customTexts = new List<CustomText>();

            customText.id = current_project.customTexts.Count + 1;

            current_project.customTexts.Add(customText);

        }

        public Playlist GetPlaylistByName(string name)
        {
            foreach (Playlist p in this.current_project.playlists)
                if (p.Title == name)
                    return p;

            return null;
        }



        public void newProject()
        {
            current_project = new project();
            current_project.alias = App.alias;

            if (App.sport!=null)
                current_project.sport = App.sport.name;

            current_project.creation = DateTime.Today;
            current_project.playlists = new List<Playlist>();
            current_project.practices = new List<Practice>();
            current_project.customTexts = new List<CustomText>();
            current_project.imageIntro = "";

        }

        public void saveFile()
        {

            //crea un file JSON con tutti i parametri
            project project = new project();
            project.alias = App.alias;
            if (current_project.alias != "") {
                project.creation = current_project.creation;
                project.playlists = current_project.playlists;
                project.practices = current_project.practices;
                project.customTexts = current_project.customTexts;
                project.imageIntro = current_project.imageIntro;
            }

            else
                project.creation = DateTime.Today;


            project.lastmodified = DateTime.Today;


            string file_content= JsonConvert.SerializeObject(project);

            string filename = App.alias.ToLowerInvariant() + "_" + App.sport.name.ToLowerInvariant() + ".json";
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

            try
            {
                StreamWriter stream = new StreamWriter(directory+filename);
                stream.Write(file_content);
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Error to save file " + filename+" "+ex.ToString());
            }

     
        }




        public void readFile()
        {



            try
            {
                string filename = App.alias.ToLowerInvariant() + "_"+App.sport.name.ToLowerInvariant()+".json";
                string directory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                StreamReader stream = new StreamReader(directory+filename);
                string file_content = stream.ReadToEnd();
                stream.Close();
                //IFolder folder = FileSystem.Current.LocalStorage;

                //IFile file = await folder.GetFileAsync(filename);
                //string file_content = await file.ReadAllTextAsync();

                current_project = JsonConvert.DeserializeObject<project>(file_content);
                //imposta tutte le playlist come non in esecuzione
                if(current_project.playlists.Count>0)
                    foreach (Playlist p in current_project.playlists)
                        p.Status = Playlist.STATUS_STOP;
            }
            catch
            {
                newProject();
            }


        }


    }
}
