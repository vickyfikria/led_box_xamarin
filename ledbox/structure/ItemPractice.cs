using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using ledbox.Resources;

namespace ledbox
{
    public class ItemPractice : INotifyPropertyChanged, ICloneable
    {
        [JsonIgnore]
        public const int TYPE_IMAGE = 0;
        [JsonIgnore]
        public const int TYPE_VIDEO = 1;
        [JsonIgnore]
        public const int TYPE_AUDIO = 2;

        [JsonIgnore]
        public string TYPE_IMAGE_LABEL = AppResources.image;
        [JsonIgnore]
        public string TYPE_VIDEO_LABEL = AppResources.video;
        [JsonIgnore]
        public string TYPE_AUDIO_LABEL = AppResources.audio;


        public string title { get; set; }
        public string filename { get; set; }
        public string filepath { get; set; }
        public int round = 1;
        public int rest = 5;
        public int work = 20;
        public int type { get; set; }
        public int soundwork = 2;
        public int soundrest = 1;

        /// <summary>
        /// Definisce se l'item ha avuto un cambio di immagine
        /// </summary>
        [JsonIgnore]
        public bool is_changed = false;


        [JsonIgnore]
        public int ordering { get; set; }

        [JsonIgnore]
        public string Title { get { return title; } set { title = value; } }
        [JsonIgnore]
        public string Filename { get { return filename; } set { filename = value; } }
        [JsonIgnore]
        public string Filepath { get { return filepath; } set { filepath = value; } }
        [JsonIgnore]
        public int Round { get { return round; } set { round = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("RoundLabel")); } }
        [JsonIgnore]
        public int Rest { get { return rest; } set { rest = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("RestMMLabel")); } }
        [JsonIgnore]
        public int Work { get { return work; } set { work = value; if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("WorkMMLabel")); } }
        [JsonIgnore]
        public int Type { get { return type; } set { type = value; } }
        [JsonIgnore]
        public int SoundWork { get { return soundwork; } set { soundwork = value; } }

        [JsonIgnore]
        public int SoundRest { get { return soundrest; } set { soundrest = value; } }


        [JsonIgnore]
        public string WorkMM { get { return App.formatSecondToMinute(Work); } }
        [JsonIgnore]
        public string RestMM { get { return App.formatSecondToMinute(rest); } }

        [JsonIgnore]
        public string RoundLabel { get { return round.ToString(); } }
        [JsonIgnore]
        public string RestMMLabel { get { return App.formatSecondToMinute(rest); } }
        [JsonIgnore]
        public string WorkMMLabel { get { return App.formatSecondToMinute(work); } }


        [JsonIgnore]
        public bool isImageEditable { get
            {

                return false; //TODO disattivazione crop image

                if (filepath == null || filepath == "" ||  filepath.Contains(".gif"))
                    return false;

                return true;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;


        CancellationTokenSource tokenSource = new CancellationTokenSource();
        bool running = true;

        public ItemPractice()
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
            }, APILedbox.FILETYPE_MEDIA);


        }

        public void sendMessageShowPreview()
        {
            upload((isFinish)=>{ 


                APILedbox.section[] sections = new APILedbox.section[4];

                APILedbox.section round = new APILedbox.section();
                round.name = "round";
                round.value = new APILedbox.section_value() { attrib = "text", value = Round.ToString() };
                sections[0] = round;

                APILedbox.section status = new APILedbox.section();
                status.name = "status";
                status.value = new APILedbox.section_value() { attrib = "text", value = "r" };
                sections[1] = status;

                APILedbox.section timer = new APILedbox.section();
                timer.name = "timer";
                timer.value = new APILedbox.section_value() { attrib = "text", value = Rest.ToString() };
                sections[2] = timer;

                APILedbox.section image = new APILedbox.section();
                image.name = "image";
                image.value = new APILedbox.section_value() { attrib = "src", value = App.alias + "/" + Filename };
                sections[3] = image;

                App.conn.SendMessage(App.api.createSectionsMessage(sections));
            });

        }


        public async void sendMessageRun(Action<bool> isFinish)
        {
            tokenSource = new CancellationTokenSource();
            running = true;
            APILedbox.section[] sections = new APILedbox.section[3];
            APILedbox.section s_round = new APILedbox.section();
            APILedbox.section s_status = new APILedbox.section();
            //APILedbox.section s_statuscolor = new APILedbox.section();
            APILedbox.section s_timer = new APILedbox.section();
            APILedbox.section s_image = new APILedbox.section();

            try
            {

                for (int j = 0; j < Round && running; j++)
                {

                    for (int i = 0; i < Rest && running; i++)
                    {
                        await Task.Factory.StartNew(() =>
                        {

                            string time = (Rest - i).ToString();





                            sections = new APILedbox.section[4];

                            s_round = new APILedbox.section();
                            s_round.name = "round";
                            s_round.value = new APILedbox.section_value() { attrib = "text", value = (j + 1).ToString() + "/" + Round.ToString() };
                            sections[0] = s_round;

                            s_status = new APILedbox.section();
                            s_status.name = "status";
                            s_status.value = new APILedbox.section_value() { attrib = "text", value = "setup" };
                            sections[1] = s_status;

                            s_timer = new APILedbox.section();
                            s_timer.name = "timer";
                            s_timer.value = new APILedbox.section_value() { attrib = "text", value = time };
                            sections[2] = s_timer;

                            s_image = new APILedbox.section();
                            s_image.name = "image";
                            s_image.value = new APILedbox.section_value() { attrib = "src", value = App.alias + "/" + Filename };
                            sections[3] = s_image;


                            App.conn.SendMessage(App.api.createSectionsMessage(sections));

                        });
                        await Task.Delay(1000, tokenSource.Token);
                    }

                    App.conn.SendMessage(App.api.createHornMessage(soundrest));

                    for (int i = 0; i < Work && running; i++)
                    {
                        await Task.Factory.StartNew(() =>
                        {

                            string time = (Work - i).ToString();

                            sections = new APILedbox.section[3];



                            s_round = new APILedbox.section();
                            s_round.name = "round";
                            s_round.value = new APILedbox.section_value() { attrib = "text", value = (j + 1).ToString() + "/" + Round.ToString() };
                            sections[0] = s_round;

                            s_status = new APILedbox.section();
                            s_status.name = "status";
                            s_status.value = new APILedbox.section_value() { attrib = "text", value = "work" };
                            sections[1] = s_status;



                            s_timer = new APILedbox.section();
                            s_timer.name = "timer";
                            s_timer.value = new APILedbox.section_value() { attrib = "text", value = time };
                            sections[2] = s_timer;

                           
                            App.conn.SendMessage(App.api.createSectionsMessage(sections));
                        });
                        await Task.Delay(1000, tokenSource.Token);
                    }


                    App.conn.SendMessage(App.api.createHornMessage(soundwork));





                }
            }
            catch (TaskCanceledException ex)
            {
                isFinish(false);
                Console.WriteLine(ex.Message);
                return;

            }

            isFinish(true);
        }

        public void stop()
        {
            running = false;
            if (tokenSource != null)
            {
                tokenSource.Cancel();
                //tokenSource.Dispose();
            }
            tokenSource = null;

        }


        public int getTotalDuration()
        {
            return round * (work + rest);
        }

        public object Clone()
        {
            return this.MemberwiseClone();

        }

    }
}
