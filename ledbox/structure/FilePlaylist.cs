using System;
using System.ComponentModel;
using ledbox.Resources;
using Newtonsoft.Json;

namespace ledbox
{
    public class FilePlaylist: INotifyPropertyChanged, ICloneable
    {

        public string filename { get; set; }
        public int duration { get; set; }
        public int type { get; set; }
        public string filepath { get; set; }
        public string previewpath { get; set; }
        

        [JsonIgnore]
        public const int TYPE_IMAGE=0;
        [JsonIgnore]
        public const int TYPE_VIDEO = 1;
        [JsonIgnore]
        public const int TYPE_AUDIO = 2;


        [JsonIgnore]
        public string Filename { get { return filename; } set { filename = value; } }
        [JsonIgnore] 
        public string Filepath { get { return filepath; } set { filepath = value; } }
        [JsonIgnore]
        public string Previewpath { get { return previewpath; } set { previewpath = value; } }

        [JsonIgnore]
        public int Duration { get { return duration; } set {

                if((type==TYPE_AUDIO && duration==0) || type!=TYPE_AUDIO ) //evita una riassegnazione impropria della durata del file
                {
                    duration = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("DurationLabel"));
                }
            }
        }
        [JsonIgnore]
        public int Type { get { return type; } set { type = value; } }

        
        [JsonIgnore]
        public string DurationLabel { get { return App.formatSecondToMinute(duration); }  }
        [JsonIgnore]
        public string TypeLabel
        {
            get
            {
                switch (type)
                {
                    case FilePlaylist.TYPE_IMAGE:
                        return AppResources.image;
                    case FilePlaylist.TYPE_AUDIO:
                        return AppResources.audio;
                    case FilePlaylist.TYPE_VIDEO:
                        return AppResources.video;
                }
                return "";
            }
        }
        [JsonIgnore]
        public string Icon
        {

            get
            {
                switch (type)
                {
                    case FilePlaylist.TYPE_IMAGE:
                        return filepath;
                    case FilePlaylist.TYPE_AUDIO:
                        return "ic_music_video_black_24dp.png";
                    case FilePlaylist.TYPE_VIDEO:
                        return "ic_image_black_24dp";
                }


                return filepath;
            }


        }


        [JsonIgnore]
        public bool ControlMediaVisible { get { return type == TYPE_IMAGE ? true : false; } }

        [JsonIgnore]
        public bool AudioMediaVisible { get { return type == TYPE_IMAGE ? false : true; } }


        public event PropertyChangedEventHandler PropertyChanged;


        public FilePlaylist()
        {
        }

        public void upload(Action<bool> isFinish)
        {
            App.conn.startUploadFile(Filename, Filepath, App.alias, (isFinish2) =>
            {
                if (isFinish2 == "")
                    isFinish(false);
                else
                    isFinish(true);
            },APILedbox.FILETYPE_MEDIA);


        }

        public void sendMessageShowPreview()
        {
            upload((isFinish) => {
                App.conn.SendMessage(App.api.createLayoutMessage("image"));
                App.conn.SendMessage(App.api.createSectionMessage("media", "src", App.alias + "/" + Filename));
            });
          
        }

        public object Clone()
        {
            return this.MemberwiseClone();

        }


    }
}
