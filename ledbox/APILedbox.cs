using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using ledbox.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ledbox
{
    public class APILedbox
    {

       

        public const string CMD_INIT = "Init";
        public const string CMD_DISCONNECT = "Disconnect";
        public const string CMD_CLEAR = "Clear";
        public const string CMD_UPLOAD = "Upload";
        public const string CMD_UPLOADED = "Uploaded";
        public const string CMD_CHANGEWAITING = "ChangeWaiting";

        public const string CMD_SETSECTION = "SetSection";
        public const string CMD_SETSECTIONS = "SetSections";
        public const string CMD_HORN = "Horn";
        public const string CMD_SETLAYOUT = "SetLayout";
        public const string CMD_RELOADLAYOUT = "ReloadLayout";

        public const string CMD_SETPLAYLISTIMAGE = "SetPlaylistImage";
        public const string CMD_STOPPLAYLISTIMAGE = "StopPlaylistImage";
        public const string CMD_STARTPLAYLISTIMAGE = "StartPlaylistImage";
        public const string CMD_PAUSEPLAYLISTIMAGE = "PausePlaylistImage";
        public const string CMD_GETLISTPLAYLISTIMAGE = "GetListPlaylistImage";
        public const string CMD_UPLOADLISTPLAYLISTIMAGE = "UploadPlaylistImage";


        public const string CMD_SETPLAYLISTAUDIO = "SetPlaylistAudio";
        public const string CMD_STOPPLAYLISTAUDIO = "StopPlaylistAudio";
        public const string CMD_STARTPLAYLISTAUDIO = "StartPlaylistAudio";
        public const string CMD_PAUSEPLAYLISTAUDIO = "PausePlaylistAudio";
        public const string CMD_GETLISTPLAYLISTAUDIO = "GetListPlaylistAudio";
        public const string CMD_UPLOADLISTPLAYLISTAUDIO = "UploadPlaylistAudio";


        public const string CMD_STARTCUSTOMTEXT = "StartCustomText";
        public const string CMD_PAUSECUSTOMTEXT = "PauseCustomText";
        public const string CMD_STOPCUSTOMTEXT = "StopCustomText";


        public const string CMD_SETPRACTICE = "SetPractice";
        public const string CMD_STARTPRACTICE = "StartPractice";
        public const string CMD_STOPPRACTICE = "StopPractice";
        public const string CMD_PAUSEPRACTICE = "PausePractice";
        public const string CMD_GETLISTPRACTICE = "GetListPractice";
        public const string CMD_UPLOADPRACTICE = "UploadPractice";



        public const string CMD_SETCONFIG = "SetConfig";
        public const string CMD_SETCONFIGS = "SetConfigs";
        public const string CMD_GETCONFIG = "GetConfig";
        public const string CMD_GETCONFIGS = "GetConfigs";

        public const string CMD_GETCLIENTS = "GetClients";

        public const string CMD_GETLISTMEDIA = "GetListMedia";

        public const string CMD_DELETEALLPLAYLISTIMAGE = "DeleteAllPlaylistImage";
        public const string CMD_DELETEALLPLAYLISTAUDIO = "DeleteAllPlaylistAudio";
        public const string CMD_DELETEINTERFACES = "DeleteInterfaces";
        public const string CMD_DELETEALLPRACTICE = "DeleteAllPractice";

        public const string CMD_DELETEALLMEDIA = "DeleteMediaAlias";
        


        public const string CMD_REBOOT = "Reboot";
        public const string CMD_STOPALLPROCESS = "StopAllProcess";
        public const string CMD_RESTARTDHCP = "RestartDHCP";
        public const string CMD_SHOWINFO = "showInfo";



        public const string CMD_FILEIMAGESELECTED = "fileImageSelected";

        public const string FILETYPE_MEDIA = "media";
        public const string FILETYPE_LAYOUT = "layout";
        public const string FILETYPE_INTERFACE = "interface";
        public const string FILETYPE_SETTING = "";





        List<JObject> pollMessages = new List<JObject>();

        public struct messageSender<T>
        {
            public string cmd;
            public T value;
            public string name;
            public string alias { get { return App.alias; } }
            public string sport { get { return App.sport.name; } }
            
        }

        public struct messageReceiver<T>
        {
            public string sender;
            public string status;
            public T value;
            public string name;
        }

        public struct messageReceiverSystem<T>
        {
            public string cmd;
            public T value;
        }

        public struct messageError
        {
            public string status;
            public string sender;
            public int error_code;
            public string error_message;
        }

        public struct file
        {
            public string filename;
            public string filepath;
            public bool exist;
            public bool forceUpload;
            public string type;
        };

        public struct section
        {
            public string name;
            public section_value value;
        };


        public struct section_value
        {
            public string attrib;
            public string value;
        };

        public struct playlist
        {
            public string name;
            public string animation;
            public int type;
            public List<file> files;
        };

        public struct playlistvalue
        {
            public string hashname;
            public string title;
        }

        public struct playlistsetvalue
        {
            public string hashname;
            public string title;
            public string onfinish;
            public int max_counter_time;
            public List<playlistfile> items;
        }


        public struct playlistfile
        {
            public string filename;
            public int duration;
            public int type;
        }



        public struct practicevalue
        {
            public string hashname;
            public string title;
        }


        public struct practicesetvalue
        {
            public string hashname;
            public string title;
            public List<practicefile> items;
            public int totalduration;
            public int type;
        }

        public struct practicefile
        {
            public string filename;
            public int work;
            public int rest;
            public int soundrest;
            public int soundwork;
            public int type;
            public int round;
        }

        public struct customtextvalue
        {
            public string hashname;
            public string title;
        }

        public struct current_setting
        {
            public string version;
            public string deviceName;
            public string role;
            //TODO per versione ledbox 0.5
            //public string sport;
            public string current_layout;
            public List<plugin> plugins;


        }


        public struct plugin
        {
            public string name;
            public float version;
            public List<JObject> parameters;
        }

        public struct horn
        {
            public int times;
            public float sleep;

        };

       

        public struct config
        {
            public string section;
            public string field;
            public string value;
            public string device;

        }


        public struct client
        {
            public string id;
            public string socket;
            public string alias;
            public string sport;
            public string type;
            public config[] configs;


        }



        public APILedbox()
        {
        }


        #region CREATE STRUCT MESSAGES
        /// <summary>
        /// Crea un messaggio LEDbox per inizializzare il ledbox
        /// </summary>
        /// <returns>The init message.</returns>
        /// <param name="alias">Alias.</param>
        public string createInitMessage(string alias)
        {
            messageSender<current_setting> m = new messageSender<current_setting>();
            m.cmd = CMD_INIT;
            m.name = alias;
            m.value = new current_setting();
            m.value.version = Xamarin.Essentials.AppInfo.VersionString;
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Disconnette il client dal LEDbox
        /// </summary>
        /// <returns></returns>
        public string createDisconnectMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DISCONNECT;
            m.name = "";
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per impostare una sezione nel layout corrente
        /// </summary>
        /// <returns>The section message.</returns>
        /// <param name="name">Name.</param>
        /// <param name="attrib">Attrib.</param>
        /// <param name="value">Value.</param>
        public string createSectionMessage(string name, string attrib, string value)
        {
            messageSender<section_value> m = new messageSender<section_value>();
            m.cmd = CMD_SETSECTION;
            m.name = name;

            section_value s = new section_value();

            s.attrib = attrib;
            s.value = value;
            
            m.value = s;
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per impostare una sezione nel layout corrente
        /// </summary>
        /// <returns>The section message.</returns>
        /// <param name="name">Name.</param>
        /// <param name="attrib">Attrib.</param>
        /// <param name="value">Value.</param>
        public string createSectionsMessage(section[] sections)
        {
            messageSender<section[]> m = new messageSender<section[]>();
            m.cmd = CMD_SETSECTIONS;
            m.value = sections;
            return JsonConvert.SerializeObject(m);
        }


        /// <summary>
        /// Crea un messaggio LEDbox per l'invio di file
        /// </summary>
        /// <returns>The file upload message.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="filepath">Filepath.</param>
        public string createFileUploadMessage(string filename,string filepath,string alias,string type,bool forceUpload=false)
        {

            
            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_UPLOAD;
            m.name = "";

            file f = new file();

            f.filename = filename;
            f.filepath = filepath;
            //f.alias = alias;

            //TODO per versione ledbox 0.5
            //f.sport = App.sport.name;

            f.exist = false;
            f.type = type;
            f.forceUpload = forceUpload;
            

            m.value = f;
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per l'invio di file
        /// </summary>
        /// <returns>The file upload message.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="filepath">Filepath.</param>
        public string createChangeWaitingMessage( string filepath)
        {


            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_CHANGEWAITING;
            m.name = "";

            file f = new file();

            f.filepath = filepath;
            f.forceUpload = true;
            m.value = f;
            return JsonConvert.SerializeObject(m);
        }




        /// <summary>
        /// Crea un messaggio LEDbox per la selezione del layout
        /// </summary>
        /// <returns>The layout message.</returns>
        /// <param name="name">Name.</param>
        public string createLayoutMessage(string name)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_SETLAYOUT;
            m.name = name;
            m.value = name;
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per la selezione del layout
        /// </summary>
        /// <returns>The layout message.</returns>
        /// <param name="name">Name.</param>
        public string createReloadLayoutMessage(string name)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_RELOADLAYOUT;
            m.name = name;
            m.value = name;
            return JsonConvert.SerializeObject(m);
        }

        public string createClearMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_CLEAR;
            m.name = "";
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createRebootMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_REBOOT;
            m.name = "";
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createRestartDHCPMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_RESTARTDHCP;
            m.name = "";
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }
        

        public string createUplodedMessage(string path_file_uploaded)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_UPLOADED;
            m.name = "";
            m.value = path_file_uploaded;
            return JsonConvert.SerializeObject(m);
        }

        /* PLAYLIST */

        public string createPlaylistMessage(Playlist playlist,int type)
        {
            if (type == Playlist.TYPE_IMAGE || type == Playlist.TYPE_VIDEO)
                return createPlaylistImageMessage(playlist);
            else
                return createPlaylistAudioMessage(playlist);
        }

        public string createStartPlaylistMessage(string playlistname,string playlisttitle, int type)
        {
            if (type == Playlist.TYPE_IMAGE || type == Playlist.TYPE_VIDEO)
                return createStartPlaylistImageMessage(playlistname, playlisttitle);
            else
                return createStartPlaylistAudioMessage(playlistname, playlisttitle);
        }

        public string createPausePlaylistMessage(string playlistname, int type)
        {
            if (type == Playlist.TYPE_IMAGE || type == Playlist.TYPE_VIDEO)
                return createPausePlaylistImageMessage(playlistname);
            else
                return createPausePlaylistAudioMessage(playlistname);
        }

        public string createStopPlaylistMessage(string playlistname,int type)
        {
            if (type == Playlist.TYPE_IMAGE || type == Playlist.TYPE_VIDEO)
                return createStopPlaylistImageMessage(playlistname);
            else
                return createStopPlaylistAudioMessage(playlistname);
        }

        /*PLAYLIST IMAGE*/

        public string createPlaylistImageMessage(Playlist playlist)
        {
            messageSender<playlistsetvalue> m = new messageSender<playlistsetvalue>();
            m.cmd = CMD_SETPLAYLISTIMAGE;
            m.value = new playlistsetvalue()
            {
                hashname = playlist.hashname,
                title = playlist.title,
                onfinish=playlist.onfinish,
                max_counter_time = playlist.max_counter_time,
                items = playlist.items_api
            };
            return JsonConvert.SerializeObject(m);
        }



        public string createStartPlaylistImageMessage(string playlistname, string playlisttitle="")
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_STARTPLAYLISTIMAGE;
            m.value = new playlistvalue() { hashname = playlistname,title= playlisttitle };
            return JsonConvert.SerializeObject(m);
        }

        public string createPausePlaylistImageMessage(string playlistname)
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_PAUSEPLAYLISTIMAGE;

            m.value = new playlistvalue() { hashname = playlistname };
            return JsonConvert.SerializeObject(m);

        }

        public string createStopPlaylistImageMessage(string playlistname)
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_STOPPLAYLISTIMAGE;
            m.value = new playlistvalue(){ hashname = playlistname };
            return JsonConvert.SerializeObject(m);
        }

        public string createGetListPlaylistImageMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_GETLISTPLAYLISTIMAGE;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createDeleteAllPlaylistImageMessage(string alias)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DELETEALLPLAYLISTIMAGE;
            m.name = alias;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per l'invio di file
        /// </summary>
        /// <returns>The file upload message.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="filepath">Filepath.</param>
        public string createUploadPlaylistImageMessage(string filename,string filepath, bool forceUpload = false)
        {

            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_UPLOADLISTPLAYLISTIMAGE;
            m.name = "";

            file f = new file();

            f.filename = filename;
            f.filepath = filepath;
            f.exist = false;
            f.forceUpload = forceUpload;

            m.value = f;
            return JsonConvert.SerializeObject(m);
        }

        /*PLAYLIST AUDIO*/

        public string createPlaylistAudioMessage(Playlist playlist)
        {
            messageSender<playlistsetvalue> m = new messageSender<playlistsetvalue>();
            m.cmd = CMD_SETPLAYLISTAUDIO;
            m.value = new playlistsetvalue()
            {
                hashname = playlist.hashname,
                title = playlist.title,
                onfinish=playlist.onfinish,
                max_counter_time = playlist.max_counter_time,
                items = playlist.items_api
            };
            return JsonConvert.SerializeObject(m);
        }

        public string createStartPlaylistAudioMessage(string playlistname,string playlisttitle="")
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_STARTPLAYLISTAUDIO;
            m.value = new playlistvalue() { hashname = playlistname, title = playlisttitle };
            return JsonConvert.SerializeObject(m);
        }

        public string createPausePlaylistAudioMessage(string playlistname)
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_PAUSEPLAYLISTAUDIO;

            m.value = new playlistvalue() { hashname = playlistname, title = "" };
            return JsonConvert.SerializeObject(m);
        }

        public string createStopPlaylistAudioMessage(string playlistname)
        {
            messageSender<playlistvalue> m = new messageSender<playlistvalue>();
            m.cmd = CMD_STOPPLAYLISTAUDIO;
            m.value = new playlistvalue() { hashname = playlistname };
            return JsonConvert.SerializeObject(m);
        }

        public string createGetListPlaylistAudioMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_GETLISTPLAYLISTAUDIO;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createDeleteAllPlaylistAudioMessage(string alias)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DELETEALLPLAYLISTAUDIO;
            m.name = alias;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per l'invio di file
        /// </summary>
        /// <returns>The file upload message.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="filepath">Filepath.</param>
        public string createUploadPlaylistAudioMessage(string filename, string filepath, bool forceUpload = false)
        {

            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_UPLOADLISTPLAYLISTAUDIO;
            m.name = "";

            file f = new file();

            f.filename = filename;
            f.filepath = filepath;
            f.exist = false;
            f.forceUpload = forceUpload;

            m.value = f;
            return JsonConvert.SerializeObject(m);
        }

        /* PRACTICE */

        public string createStartPracticeMessage(string practicename,string practicetitle)
        {
            messageSender<practicevalue> m = new messageSender<practicevalue>();
            m.cmd = CMD_STARTPRACTICE;
            m.value = new practicevalue() { hashname = practicename,title=practicetitle };
            return JsonConvert.SerializeObject(m);
        }

        public string createPracticeMessage(Practice practice)
        {
            messageSender<practicesetvalue> m = new messageSender<practicesetvalue>();
            m.cmd = CMD_SETPRACTICE;
            m.value = new practicesetvalue(){
                hashname=practice.hashname,
                title=practice.title,
                items=practice.items_api,
                totalduration=practice.totalduration,
                type=practice.type
            };
            
            return JsonConvert.SerializeObject(m);
        }

        public string createPausePracticeMessage(string practicename)
        {
            messageSender<practicevalue> m = new messageSender<practicevalue>();
            m.cmd = CMD_PAUSEPRACTICE;
            m.value = new practicevalue() { hashname = practicename };
            return JsonConvert.SerializeObject(m);
        }

        public string createStopPracticeMessage(string practicename)
        {
            messageSender<practicevalue> m = new messageSender<practicevalue>();
            m.cmd = CMD_STOPPRACTICE;
            m.value = new practicevalue() { hashname = practicename };
            return JsonConvert.SerializeObject(m);
        }

        public string createGetListPracticeMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_GETLISTPRACTICE;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createDeleteAllPracticeMessage(string alias)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DELETEALLPRACTICE;
            m.name = alias;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        /// <summary>
        /// Crea un messaggio LEDbox per l'invio di file
        /// </summary>
        /// <returns>The file upload message.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="filepath">Filepath.</param>
        public string createUploadPracticeMessage(string filename, string filepath, bool forceUpload = false)
        {

            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_UPLOADPRACTICE;
            m.name = "";

            file f = new file();

            f.filename = filename;
            f.filepath = filepath;
            f.exist = false;
            f.forceUpload = forceUpload;

            m.value = f;
            return JsonConvert.SerializeObject(m);
        }




        /* CUSTOM TEXT */

        public string createStartCustomTextMessage(CustomText customText=null)
        {
            messageSender<CustomText> m = new messageSender<CustomText>();
            m.cmd = CMD_STARTCUSTOMTEXT;
            m.value = customText;
            return JsonConvert.SerializeObject(m);
        }

        public string createPauseCustomTextMessage(CustomText customText=null)
        {
            messageSender<CustomText> m = new messageSender<CustomText>();
            m.cmd = CMD_PAUSECUSTOMTEXT;
            m.value = customText;
            return JsonConvert.SerializeObject(m);
        }

        public string createStopCustomTextMessage(string customTitle)
        {
            messageSender<customtextvalue> m = new messageSender<customtextvalue>();
            m.cmd = CMD_STOPCUSTOMTEXT;
            m.value = new customtextvalue() { hashname = customTitle };
            return JsonConvert.SerializeObject(m);
        }

        /* INTERFACES */

        public string createDeleteAllIntefacesMessage()
        {
                                 
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DELETEINTERFACES;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        /* HORN */

        public string createHornMessage(int idx)
        {
            int times = 0;
            float sleep = 0.5f;

            switch (idx)
            {
                case 0: //nessun suono
                    times = 0;
                    break;
                case 1: //1 suono breve
                    times = 1;
                    sleep = 0.5f;
                    break;
                case 2: //2 suoni brevi
                    times = 2;
                    sleep = 0.5f;
                    break;
                case 3: //3 suoni brevi
                    times = 3;
                    sleep = 0.5f;
                    break;
                case 4: //1 suono lungo
                    times = 1;
                    sleep = 1f;
                    break;
                case 5: //2 suono lungo
                    times = 2;
                    sleep = 1f;
                    break;
                case 6: //3 suono lungo
                    times = 3;
                    sleep = 1f;
                    break;
            }

            messageSender<horn> m = new messageSender<horn>();
            m.cmd = CMD_HORN;

            m.value = new horn() { times = times, sleep = sleep };


            return JsonConvert.SerializeObject(m);



        }


        public string createDeleteAllMedia(string alias)
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_DELETEALLMEDIA;
            m.name = alias;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }


        public string createSetConfigMessage(config config)
        {
            messageSender<config> m = new messageSender<config>();
            m.cmd = CMD_SETCONFIG;
            m.value = config;
            return JsonConvert.SerializeObject(m);
        }

        public string createSetConfigsMessage(config[] configs)
        {
            messageSender<config[]> m = new messageSender<config[]>();
            m.cmd = CMD_SETCONFIGS;
            m.value = configs;
            return JsonConvert.SerializeObject(m);
        }

        public string createGetConfigMessage(config config)
        {
            messageSender<config> m = new messageSender<config>();
            m.cmd = CMD_GETCONFIG;
            m.value = config;
            return JsonConvert.SerializeObject(m);
        }

        public string createGetConfigsMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_GETCONFIGS;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createGetClientsMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_GETCLIENTS;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createStopAllProcessMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_STOPALLPROCESS;
            m.value = "";
            return JsonConvert.SerializeObject(m);
        }

        public string createFileImageSelected(file file,string name)
        {
            messageSender<file> m = new messageSender<file>();
            m.cmd = CMD_FILEIMAGESELECTED;
            m.name = name;
            m.value = file;
            return JsonConvert.SerializeObject(m);
        }

        public string createShowInfoMessage()
        {
            messageSender<string> m = new messageSender<string>();
            m.cmd = CMD_SHOWINFO;
            m.name = "";
            return JsonConvert.SerializeObject(m);
        }


        public messageReceiver<T> parseMessage<T>(string message)
        {
            messageReceiver<T> m = new messageReceiver<T>();
          
            m = JsonConvert.DeserializeObject<messageReceiver<T>>(message);
                        
            return m;
             
        }

        public messageReceiverSystem<T> parseMessageSystem<T>(string message)
        {
            messageReceiverSystem<T> m = new messageReceiverSystem<T>();

            m = JsonConvert.DeserializeObject<messageReceiverSystem<T>>(message);

            return m;

        }

        public messageError parseErrorMessage(string message)
        {
            messageError m = new messageError();

            m = JsonConvert.DeserializeObject<messageError>(message);

            return m;

        }

        #endregion


        /// <summary>
        /// Processa i messaggi ricevuti dal LEDbox
        /// </summary>
        /// <param name="message">Message.</param>
        public void processMessage(string message)
        {

          

            Console.WriteLine("MESSAGE RECIVED: "+message);



            try
            {
                JObject jo = JsonConvert.DeserializeObject<JObject>(message);
                
                if (jo != null)
                {

                   
                   



                    if (jo.ContainsKey("status"))
                    {
                        if (jo["status"].ToString() == "error")
                        {

                            messageError me = App.api.parseErrorMessage(message);
                            Console.WriteLine("MESSAGE RECIVED ERROR : "+ me.error_message);

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                UserDialogs.Instance.HideLoading();
                            });
                            switch (me.error_code)
                            {
                                case 8: //app non compatibile
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        App.DisplayAlert(AppResources.app_not_compatible);
                                        if(App.conn!=null)
                                            App.conn.DisconnectToLedbox();
                                    });
                                    break;
                                default:
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.error_communication);
                                    });
                                    break;
                            }

                        }
                    }
                    if (jo.ContainsKey("sender"))
                    {


                        MessagingCenter.Send<APILedbox>(this, "message_received");


                        switch (jo["sender"].ToString())
                        {
                            case APILedbox.CMD_INIT:
                                APILedbox.messageReceiver<APILedbox.current_setting> i = App.api.parseMessage<APILedbox.current_setting>(message);
                                if (i.sender == APILedbox.CMD_INIT)
                                {
                                    
                                    Preferences.Set("ledbox_deviceName", i.value.deviceName);

                                    App.deviceName = i.value.deviceName;
                                    App.current_ledbox_version = float.Parse(i.value.version.Replace(".", ","));
                                    App.role = i.value.role;
                                    App.current_ledbox_layout = i.value.current_layout;

                                    MessagingCenter.Send<APILedbox, APILedbox.current_setting>(this, "connected", i.value);






                                    //trova se è installato il plugin Playlist
                                    foreach(plugin plug in i.value.plugins)
                                    {
                                        string current_playlistname = "";
                                        string current_playlisttitle = "";
                                        string current_practicename = "";
                                        string current_practicetitle = "";

                                        string current_customtextname = "";
                                        int current_status = 0;

                                        if (plug.name == "playlistimage")
                                        {
                                            
                                            for(int k = 0; k < plug.parameters.Count; k++)
                                            {

                                                if (plug.parameters[k].ContainsKey("current_playlisttitle"))
                                                    if (plug.parameters[k].GetValue("current_playlisttitle").ToString() != "")
                                                        current_playlisttitle = plug.parameters[k].GetValue("current_playlisttitle").ToString();

                                                if (plug.parameters[k].ContainsKey("current_playlistname"))
                                                    if (plug.parameters[k].GetValue("current_playlistname").ToString() != "")
                                                        current_playlistname = plug.parameters[k].GetValue("current_playlistname").ToString();

                                                if (plug.parameters[k].ContainsKey("current_status"))
                                                    if (plug.parameters[k].GetValue("current_status").ToString() != "")
                                                        current_status = plug.parameters[k].GetValue("current_status").Value<int>();

                                            }
                                            if(current_playlistname!="")
                                                App.avm.AddActivity(current_playlisttitle, current_playlistname, Activity.TYPE_PLAYLIST_IMAGE, current_status);
                                     
                                        }

                                        current_playlistname = "";
                                        current_playlisttitle = "";
                                        current_status = 0;

                                        if (plug.name == "playlistaudio")
                                        {

                                            for (int k = 0; k < plug.parameters.Count; k++)
                                            {
                                                if (plug.parameters[k].ContainsKey("current_playlisttitle"))
                                                    if (plug.parameters[k].GetValue("current_playlisttitle").ToString() != "")
                                                        current_playlisttitle = plug.parameters[k].GetValue("current_playlisttitle").ToString();

                                                if (plug.parameters[k].ContainsKey("current_playlistname"))
                                                    if (plug.parameters[k].GetValue("current_playlistname").ToString() != "")
                                                        current_playlistname = plug.parameters[k].GetValue("current_playlistname").ToString();

                                                if (plug.parameters[k].ContainsKey("current_status"))
                                                    if (plug.parameters[k].GetValue("current_status").ToString() != "")
                                                        current_status = plug.parameters[k].GetValue("current_status").Value<int>();
                                            }

                                            if (current_playlistname != "")
                                                App.avm.AddActivity(current_playlisttitle, current_playlistname, Activity.TYPE_PLAYLIST_AUDIO, current_status);

                                        }

                                        

                                        if (plug.name == "practice")
                                        {
                                            for (int k = 0; k < plug.parameters.Count; k++)
                                            {
                                                if (plug.parameters[k].ContainsKey("current_practicetitle"))
                                                    if (plug.parameters[k].GetValue("current_practicetitle").ToString() != "")
                                                        current_practicetitle = plug.parameters[k].GetValue("current_practicetitle").ToString();

                                                if (plug.parameters[k].ContainsKey("current_practicename"))
                                                    if (plug.parameters[k].GetValue("current_practicename").ToString() != "")
                                                        current_practicename = plug.parameters[k].GetValue("current_practicename").ToString();

                                                if (plug.parameters[k].ContainsKey("current_status"))
                                                    if (plug.parameters[k].GetValue("current_status").ToString() != "")
                                                        current_status = plug.parameters[k].GetValue("current_status").Value<int>();
                                            }

                                            if (current_practicename != "")
                                                App.avm.AddActivity(current_practicetitle,current_practicename, Activity.TYPE_PRACTICE, current_status);
                                           

                                        }

                                        if (plug.name == "customtext")
                                        {
                                            for (int k = 0; k < plug.parameters.Count; k++)
                                            {
                                                if (plug.parameters[k].ContainsKey("current_customtextname"))
                                                    if (plug.parameters[k].GetValue("current_customtextname").ToString() != "")
                                                        current_customtextname = plug.parameters[k].GetValue("current_customtextname").ToString();


                                            }

                                            if (current_customtextname != "")
                                                App.avm.AddActivity(current_customtextname, current_customtextname, Activity.TYPE_CUSTOM_TEXT, 0);
                                         
                                        }


                                    }


                                    App.OnAfterConnect();


                                }

                                break;
                            case APILedbox.CMD_UPLOAD:
                                APILedbox.messageReceiver<APILedbox.file> m = App.api.parseMessage<APILedbox.file>(message);
                                if (m.sender == APILedbox.CMD_UPLOAD)
                                {
                                    App.conn.sendFile(m.value.filepath, m.value.exist,m.value.forceUpload);
                                }
                                break;
                            case APILedbox.CMD_UPLOADED:
                                APILedbox.messageReceiverSystem<APILedbox.file> uploaded = App.api.parseMessageSystem<APILedbox.file>(message);

                                MessagingCenter.Send<ConnectionInterface, APILedbox.file>(App.conn, "uploaded", uploaded.value);


                                break;
                            case APILedbox.CMD_CHANGEWAITING:
                                APILedbox.messageReceiver<APILedbox.file> cw = App.api.parseMessage<APILedbox.file>(message);
                                if (cw.sender == APILedbox.CMD_CHANGEWAITING)
                                {
                                    App.conn.sendFile(cw.value.filepath, cw.value.exist, cw.value.forceUpload);
                                }
                                break;
                            case APILedbox.CMD_SETPLAYLISTIMAGE:
                                APILedbox.messageReceiver<Playlist> p = App.api.parseMessage<Playlist>(message);
                                if (p.sender == APILedbox.CMD_SETPLAYLISTIMAGE)
                                {
                                    MessagingCenter.Send<APILedbox, Playlist>(this, "playlist_setted", p.value);
                                }

                                break;


                            case APILedbox.CMD_STARTPLAYLISTIMAGE:
                                APILedbox.messageReceiver<playlistvalue> sp = App.api.parseMessage<playlistvalue>(message);
                                if (sp.sender == APILedbox.CMD_STARTPLAYLISTIMAGE)
                                {
                                    App.avm.AddActivity(sp.value.title,sp.value.hashname, Activity.TYPE_PLAYLIST_IMAGE, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_start", sp.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPLAYLISTIMAGE:
                                APILedbox.messageReceiver<playlistvalue> sps = App.api.parseMessage<playlistvalue>(message);
                                if (sps.sender == APILedbox.CMD_PAUSEPLAYLISTIMAGE)
                                {
                                    App.avm.AddActivity(sps.value.title,sps.value.hashname, Activity.TYPE_PLAYLIST_IMAGE, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_pause", sps.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPPLAYLISTIMAGE:
                                APILedbox.messageReceiver<playlistvalue> ssp = App.api.parseMessage<playlistvalue>(message);
                                if (ssp.sender == APILedbox.CMD_STOPPLAYLISTIMAGE)
                                {
                                    App.avm.RemoveActivity(ssp.value.hashname, Activity.TYPE_PLAYLIST_IMAGE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_stop", ssp.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_GETLISTPLAYLISTIMAGE:
                                APILedbox.messageReceiver<List<Playlist>> glpli = App.api.parseMessage<List<Playlist>>(message);
                                if (glpli.sender == APILedbox.CMD_GETLISTPLAYLISTIMAGE)
                                {
                                    MessagingCenter.Send<APILedbox, (List<Playlist>,int)>(this, "playlist_getlist", (glpli.value,Playlist.TYPE_IMAGE));
                                }

                                break;

                            case APILedbox.CMD_DELETEALLPLAYLISTIMAGE:
                                APILedbox.messageReceiver<string> dpi = App.api.parseMessage<string>(message);
                                if (dpi.sender == APILedbox.CMD_DELETEALLPLAYLISTIMAGE)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.confirm_deletion);
                                    });
                                }

                                break;
                            case APILedbox.CMD_UPLOADLISTPLAYLISTIMAGE:
                                APILedbox.messageReceiver<APILedbox.file> uploadpi = App.api.parseMessage<APILedbox.file>(message);
                                if (uploadpi.sender == APILedbox.CMD_UPLOADLISTPLAYLISTIMAGE)
                                {
                                    App.conn.sendFile(uploadpi.value.filepath, uploadpi.value.exist, uploadpi.value.forceUpload);
                                }
                                break;


                            case APILedbox.CMD_SETPLAYLISTAUDIO:
                                APILedbox.messageReceiver<Playlist> pa = App.api.parseMessage<Playlist>(message);
                                if (pa.sender == APILedbox.CMD_SETPLAYLISTAUDIO)
                                {
                                    MessagingCenter.Send<APILedbox, Playlist>(this, "playlist_setted", pa.value);
                                }

                                break;


                            case APILedbox.CMD_STARTPLAYLISTAUDIO:
                                APILedbox.messageReceiver<playlistvalue> spa = App.api.parseMessage<playlistvalue>(message);
                                if (spa.sender == APILedbox.CMD_STARTPLAYLISTAUDIO)
                                {
                                    App.avm.AddActivity(spa.value.title,spa.value.hashname, Activity.TYPE_PLAYLIST_AUDIO, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_start", spa.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPLAYLISTAUDIO:
                                APILedbox.messageReceiver<playlistvalue> spsa = App.api.parseMessage<playlistvalue>(message);
                                if (spsa.sender == APILedbox.CMD_PAUSEPLAYLISTAUDIO)
                                {
                                    App.avm.AddActivity(spsa.value.title,spsa.value.hashname, Activity.TYPE_PLAYLIST_AUDIO, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_pause", spsa.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPPLAYLISTAUDIO:
                                APILedbox.messageReceiver<playlistvalue> sspa = App.api.parseMessage<playlistvalue>(message);
                                if (sspa.sender == APILedbox.CMD_STOPPLAYLISTAUDIO)
                                {
                                    App.avm.RemoveActivity(sspa.value.hashname, Activity.TYPE_PLAYLIST_AUDIO);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_stop", sspa.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_GETLISTPLAYLISTAUDIO:
                                APILedbox.messageReceiver<List<Playlist>> glpla = App.api.parseMessage<List<Playlist>>(message);
                                if (glpla.sender == APILedbox.CMD_GETLISTPLAYLISTAUDIO)
                                {
                                    MessagingCenter.Send<APILedbox, (List<Playlist>,int)>(this, "playlist_getlist", (glpla.value,Playlist.TYPE_AUDIO));
                                }

                                break;

                            case APILedbox.CMD_DELETEALLPLAYLISTAUDIO:
                                APILedbox.messageReceiver<string> dpa = App.api.parseMessage<string>(message);
                                if (dpa.sender == APILedbox.CMD_DELETEALLPLAYLISTAUDIO)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.confirm_deletion);
                                    });
                                }

                                break;
                            case APILedbox.CMD_UPLOADLISTPLAYLISTAUDIO:
                                APILedbox.messageReceiver<APILedbox.file> uploadpa = App.api.parseMessage<APILedbox.file>(message);
                                if (uploadpa.sender == APILedbox.CMD_UPLOADLISTPLAYLISTAUDIO)
                                {
                                    App.conn.sendFile(uploadpa.value.filepath, uploadpa.value.exist, uploadpa.value.forceUpload);
                                }
                                break;

                            case APILedbox.CMD_SETPRACTICE:
                                APILedbox.messageReceiver<Practice> pr = App.api.parseMessage<Practice>(message);
                                if (pr.sender == APILedbox.CMD_SETPRACTICE)
                                {
                                    MessagingCenter.Send<APILedbox, Practice>(this, "practice_setted", pr.value);
                                }

                                break;


                            case APILedbox.CMD_STARTPRACTICE:
                                APILedbox.messageReceiver<practicevalue> spr = App.api.parseMessage<practicevalue>(message);
                                if (spr.sender == APILedbox.CMD_STARTPRACTICE)
                                {
                                    App.avm.AddActivity(spr.value.title,spr.value.hashname, Activity.TYPE_PRACTICE, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "practice_start", spr.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPRACTICE:
                                APILedbox.messageReceiver<practicevalue> sprp = App.api.parseMessage<practicevalue>(message);
                                if (sprp.sender == APILedbox.CMD_PAUSEPRACTICE)
                                {
                                    App.avm.AddActivity(sprp.value.title,sprp.value.hashname, Activity.TYPE_PRACTICE, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "practice_pause", sprp.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPPRACTICE:
                                APILedbox.messageReceiver<practicevalue> sspr = App.api.parseMessage<practicevalue>(message);
                                if (sspr.sender == APILedbox.CMD_STOPPRACTICE)
                                {
                                    App.avm.RemoveActivity(sspr.value.hashname, Activity.TYPE_PRACTICE);
                                    MessagingCenter.Send<APILedbox, string>(this, "practice_stop", sspr.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_GETLISTPRACTICE:
                                APILedbox.messageReceiver<List<Practice>> glpr = App.api.parseMessage<List<Practice>>(message);
                                if (glpr.sender == APILedbox.CMD_GETLISTPRACTICE)
                                {
                                    MessagingCenter.Send<APILedbox, List<Practice>>(this, "practice_getlist", glpr.value);
                                }

                                break;

                            case APILedbox.CMD_DELETEALLPRACTICE:
                                APILedbox.messageReceiver<string> dpr = App.api.parseMessage<string>(message);
                                if (dpr.sender == APILedbox.CMD_DELETEALLPRACTICE)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.confirm_deletion);
                                    });
                                }

                                break;
                            case APILedbox.CMD_UPLOADPRACTICE:
                                APILedbox.messageReceiver<APILedbox.file> uploadpr = App.api.parseMessage<APILedbox.file>(message);
                                if (uploadpr.sender == APILedbox.CMD_UPLOADPRACTICE)
                                {
                                    App.conn.sendFile(uploadpr.value.filepath, uploadpr.value.exist, uploadpr.value.forceUpload);
                                }
                                break;

                            case APILedbox.CMD_STARTCUSTOMTEXT:
                                APILedbox.messageReceiver<customtextvalue> sct = App.api.parseMessage<customtextvalue>(message);
                                if (sct.sender == APILedbox.CMD_STARTCUSTOMTEXT)
                                {
                                    App.avm.AddActivity(sct.value.title,sct.value.hashname, Activity.TYPE_CUSTOM_TEXT, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "customtext_start", sct.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSECUSTOMTEXT:
                                APILedbox.messageReceiver<customtextvalue> pct = App.api.parseMessage<customtextvalue>(message);
                                if (pct.sender == APILedbox.CMD_PAUSECUSTOMTEXT)
                                {
                                    App.avm.AddActivity(pct.value.title,pct.value.hashname, Activity.TYPE_CUSTOM_TEXT, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "customtext_pause", pct.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPCUSTOMTEXT:
                                APILedbox.messageReceiver<customtextvalue> ssct = App.api.parseMessage<customtextvalue>(message);
                                if (ssct.sender == APILedbox.CMD_STOPCUSTOMTEXT)
                                {
                                    App.avm.RemoveActivity(ssct.value.hashname, Activity.TYPE_CUSTOM_TEXT);
                                    MessagingCenter.Send<APILedbox, string>(this, "customtext_stop", ssct.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_SETLAYOUT:
                                APILedbox.messageReceiver<string> l = App.api.parseMessage<string>(message);
                                if (l.sender == APILedbox.CMD_SETLAYOUT)
                                {
                                    MessagingCenter.Send<APILedbox, string>(this, "layout_loaded", l.value);
                                }
                                break;

                            case APILedbox.CMD_GETCONFIG:
                                APILedbox.messageReceiver<config> conf = App.api.parseMessage<config>(message);
                                if (conf.sender == APILedbox.CMD_GETCONFIG)
                                {

                                    App.setConfigLedbox(conf.value);
                                    MessagingCenter.Send<APILedbox, APILedbox.config>(this, "configs_getted", conf.value);
                                }
                                break;

                            case APILedbox.CMD_GETCONFIGS:
                                APILedbox.messageReceiver<APILedbox.config[]> confs = App.api.parseMessage<APILedbox.config[]>(message);
                                if (confs.sender == APILedbox.CMD_GETCONFIGS)
                                {
                                    foreach(APILedbox.config c in confs.value)
                                    {
                                        App.setConfigLedbox(c);
                                    }
                                    MessagingCenter.Send<APILedbox, APILedbox.config[]>(this, "configs_getted", confs.value);
                                }
                                break;

                            case APILedbox.CMD_SETCONFIGS:
                                APILedbox.messageReceiver<string> sconfs = App.api.parseMessage<string>(message);
                                if (sconfs.sender == APILedbox.CMD_SETCONFIGS)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.config_saved);
                                       
                                    });
                                    MessagingCenter.Send<APILedbox>(this, "configs_saved");
                                }
                                break;


                            case APILedbox.CMD_STOPALLPROCESS:
                                APILedbox.messageReceiver<string> sap = App.api.parseMessage<string>(message);
                                if (sap.sender == APILedbox.CMD_STOPALLPROCESS)
                                {
                                    App.avm.ClearActivities();
                                    MessagingCenter.Send<APILedbox, string>(App.api, "stop_all", "");
                                }
                                break;

                            case APILedbox.CMD_DELETEALLMEDIA:
                                APILedbox.messageReceiver<string> dam = App.api.parseMessage<string>(message);
                                if (dam.sender == APILedbox.CMD_DELETEALLMEDIA)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.confirm_deletion);
                                    });
                                }

                                break;

                            case APILedbox.CMD_DELETEINTERFACES:
                                APILedbox.messageReceiver<string> dai = App.api.parseMessage<string>(message);
                                if (dai.sender == APILedbox.CMD_DELETEINTERFACES)
                                {
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        DependencyService.Get<IMessage>().ShortAlert(AppResources.confirm_deletion);
                                    });
                                }

                                break;

                            case APILedbox.CMD_GETCLIENTS:
                                APILedbox.messageReceiver<APILedbox.client[]> clients = App.api.parseMessage<APILedbox.client[]>(message);
                                if (clients.sender == APILedbox.CMD_GETCONFIGS)
                                {
                                    MessagingCenter.Send<APILedbox, APILedbox.client[]>(this, "clients_connetted", clients.value);
                                }
                                break;

                            case APILedbox.CMD_RESTARTDHCP:
                                APILedbox.messageReceiver<bool> rdhcp = App.api.parseMessage<bool>(message);
                                if (rdhcp.sender == APILedbox.CMD_RESTARTDHCP)
                                {
                                    MessagingCenter.Send<APILedbox, bool>(this, "DHCP_restarted", rdhcp.value);
                                }
                                break;
                        }
                    }
                    
                    if (jo.ContainsKey("cmd"))
                    {
                        switch (jo["cmd"].ToString())
                        {
                            case APILedbox.CMD_UPLOADED:
                                APILedbox.messageReceiverSystem<APILedbox.file> m = App.api.parseMessageSystem<APILedbox.file>(message);

                                MessagingCenter.Send<ConnectionInterface, APILedbox.file>(App.conn, "uploaded", m.value);


                                break;

                            case APILedbox.CMD_STARTPLAYLISTIMAGE:
                                APILedbox.messageReceiverSystem<playlistvalue> pistart = App.api.parseMessageSystem<playlistvalue>(message);
                                if (pistart.cmd == APILedbox.CMD_STARTPLAYLISTIMAGE)
                                {

                                    App.avm.AddActivity(pistart.value.title,pistart.value.hashname, Activity.TYPE_PLAYLIST_IMAGE, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_start", pistart.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPLAYLISTIMAGE:
                                APILedbox.messageReceiverSystem<playlistvalue> pipause = App.api.parseMessageSystem<playlistvalue>(message);
                                if (pipause.cmd == APILedbox.CMD_PAUSEPLAYLISTIMAGE)
                                {

                                    App.avm.AddActivity(pipause.value.title,pipause.value.hashname, Activity.TYPE_PLAYLIST_IMAGE, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_pause", pipause.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPPLAYLISTIMAGE:
                                APILedbox.messageReceiverSystem<playlistvalue> pistop = App.api.parseMessageSystem<playlistvalue>(message);
                                if (pistop.cmd == APILedbox.CMD_STOPPLAYLISTIMAGE)
                                {

                                    App.avm.RemoveActivity(pistop.value.hashname, Activity.TYPE_PLAYLIST_IMAGE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_stop", pistop.value.hashname);
                                }

                                break;


                            case APILedbox.CMD_STARTPLAYLISTAUDIO:
                                APILedbox.messageReceiverSystem<playlistvalue> pastart = App.api.parseMessageSystem<playlistvalue>(message);
                                if (pastart.cmd == APILedbox.CMD_STARTPLAYLISTAUDIO)
                                {

                                    App.avm.AddActivity(pastart.value.title,pastart.value.hashname, Activity.TYPE_PLAYLIST_AUDIO, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_start", pastart.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPLAYLISTAUDIO:
                                APILedbox.messageReceiverSystem<playlistvalue> papause = App.api.parseMessageSystem<playlistvalue>(message);
                                if (papause.cmd == APILedbox.CMD_PAUSEPLAYLISTIMAGE)
                                {

                                    App.avm.AddActivity(papause.value.title,papause.value.hashname, Activity.TYPE_PLAYLIST_AUDIO, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_pause", papause.value.hashname);
                                }

                                break;


                            case APILedbox.CMD_STOPPLAYLISTAUDIO:
                                APILedbox.messageReceiverSystem<playlistvalue> pa = App.api.parseMessageSystem<playlistvalue>(message);
                                if (pa.cmd == APILedbox.CMD_STOPPLAYLISTAUDIO)
                                {

                                    App.avm.RemoveActivity(pa.value.hashname, Activity.TYPE_PLAYLIST_AUDIO);
                                    MessagingCenter.Send<APILedbox, string>(this, "playlist_stop", pa.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STARTPRACTICE:
                                APILedbox.messageReceiverSystem<practicevalue> prastart = App.api.parseMessageSystem<practicevalue>(message);
                                if (prastart.cmd == APILedbox.CMD_STARTPRACTICE)
                                {

                                    App.avm.AddActivity(prastart.value.title,prastart.value.hashname, Activity.TYPE_PRACTICE, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "practice_start", prastart.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSEPRACTICE:
                                APILedbox.messageReceiverSystem<practicevalue> prapause = App.api.parseMessageSystem<practicevalue>(message);
                                if (prapause.cmd == APILedbox.CMD_PAUSEPRACTICE)
                                {

                                    App.avm.AddActivity(prapause.value.title,prapause.value.hashname, Activity.TYPE_PRACTICE, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "practire_pause", prapause.value.hashname);
                                }

                                break;


                            case APILedbox.CMD_STOPPRACTICE:
                                APILedbox.messageReceiverSystem<practicevalue> pr = App.api.parseMessageSystem<practicevalue>(message);
                                if (pr.cmd == APILedbox.CMD_STOPPRACTICE)
                                {

                                    App.avm.RemoveActivity(pr.value.hashname, Activity.TYPE_PRACTICE);
                                    MessagingCenter.Send<APILedbox, string>(this, "practice_stop", pr.value.hashname);
                                }

                                break;


    
                            case APILedbox.CMD_STARTCUSTOMTEXT:
                                APILedbox.messageReceiverSystem<customtextvalue> ctstart = App.api.parseMessageSystem<customtextvalue>(message);
                                if (ctstart.cmd == APILedbox.CMD_STARTCUSTOMTEXT)
                                {

                                    App.avm.AddActivity(ctstart.value.title,ctstart.value.hashname, Activity.TYPE_CUSTOM_TEXT, Activity.STATUS_PLAY);
                                    MessagingCenter.Send<APILedbox, string>(this, "customtext_start", ctstart.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_PAUSECUSTOMTEXT:
                                APILedbox.messageReceiverSystem<customtextvalue> ctpause = App.api.parseMessageSystem<customtextvalue>(message);
                                if (ctpause.cmd == APILedbox.CMD_PAUSECUSTOMTEXT)
                                {

                                    App.avm.AddActivity(ctpause.value.title,ctpause.value.hashname, Activity.TYPE_CUSTOM_TEXT, Activity.STATUS_PAUSE);
                                    MessagingCenter.Send<APILedbox, string>(this, "custontext_pause", ctpause.value.hashname);
                                }

                                break;
                            case APILedbox.CMD_STOPCUSTOMTEXT:
                                APILedbox.messageReceiverSystem<customtextvalue> ctstop = App.api.parseMessageSystem<customtextvalue>(message);
                                if (ctstop.cmd == APILedbox.CMD_STOPCUSTOMTEXT)
                                {

                                    App.avm.RemoveActivity(ctstop.value.hashname, Activity.TYPE_CUSTOM_TEXT);
                                    MessagingCenter.Send<APILedbox, string>(this, "customtext_stop", ctstop.value.hashname);
                                }

                                break;

                            case APILedbox.CMD_STOPALLPROCESS:
                                APILedbox.messageReceiverSystem<string> sap = App.api.parseMessageSystem<string>(message);
                                if (sap.cmd == APILedbox.CMD_STOPALLPROCESS)
                                {
                                    App.avm.ClearActivities();
                                    MessagingCenter.Send<APILedbox, string>(App.api, "stop_all", "");
                                }
                                break;

                        }
                    }
                   






                }

                //rinoltra il messaggio ai vari sender
                MessagingCenter.Send<APILedbox, string>(this, "messageReceived", message);

            }
            catch(JsonException ex){
                Device.BeginInvokeOnMainThread (() =>
                {
                    DependencyService.Get<IMessage>().ShortAlert(AppResources.error_communication);
                    UserDialogs.Instance.HideLoading();
                });
                
                Console.WriteLine("MESSAGE RECIVED ERROR : Error nel parsing JSON del messaggio " + message+" "+ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR : "+ ex.Message);

            }




        }






    }
}
