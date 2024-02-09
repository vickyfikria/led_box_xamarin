using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ledbox.Resources;
using MvvmHelpers;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ledbox
{
    public class CustomText: ObservableObject, INotifyPropertyChanged
    {

        const string NO_ANIMATION= "";
        const string SCROLLER_X_LR = "scroller_x_lr";
        const string SCROLLER_Y_TB = "scroller_y_tb";
        const string SCROLLER_X_RL = "scroller_x_rl";
        const string SCROLLER_Y_BT = "scroller_y_bt";
        const string BLINKING = "blinking";



        public const int STATUS_PLAY = 1;
        public const int STATUS_STOP = 0;
        public const int STATUS_PAUSE = 2;

        public int id = 0;
        public string title;
        public string hashname { get { return title; } }
        public string text;
        public int fontsize=10;
        public string color;
        public string animation;
        public int animation_velocity=1;

        [JsonIgnore]
        public int _status = 0;
        [JsonIgnore]
        public int status { get { return _status; } set { _status = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseIconButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseTextButton"));
                    PropertyChanged(this, new PropertyChangedEventArgs("BackgroundStopIcon"));
                    PropertyChanged(this, new PropertyChangedEventArgs("IsVisibleStop"));
                    
                    

                }
            }
        }

        [JsonIgnore]
        public string PlayPauseTextButton
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return "PAUSE";
                    case STATUS_PAUSE:
                        return "PLAY";
                }

                return "PLAY";

            }
        }



        [JsonIgnore]
        public string Title { get { return title; } set { title = value; } }
        [JsonIgnore]
        public int Color { get {

                switch (color)
                {
                    case "255,255,255": //bianco
                        return 0;
                    case "255,0,0": // rosso
                        return 1;
                    case "255,255,0": // giallo
                        return 2;
                    case "0,0,255": // blu
                        return 3;
                    case "255,153,51": // arancio
                        return 4;
                    case "255,0,255": // viola
                        return 5;
                    case "0,255,0": // verde
                        return 6;
                        
                }

                return 0;

            } set {
                switch (value)
                {
                    case 0:
                        color= "255,255,255"; //bianco
                        break;
                    case 1:
                        color= "255,0,0"; // rosso
                        break;
                    case 2:
                        color = "255,255,0"; // giallo
                        break;
                    case 3:
                        color = "0,0,255"; // blu
                        break;
                    case 4:
                        color = "255,153,51"; // arancio
                        break;
                    case 5:
                        color = "255,0,255"; // viola
                        break;
                    case 6:
                        color = "0,255,0"; // verde
                        break;
                }
                
            }
        }
        [JsonIgnore]
        public string Text { get { return text; } set { text = value; } }
        [JsonIgnore]
        public int Animation { get
            {
                switch (animation)
                {
                    case NO_ANIMATION:
                        return 0;
                    case SCROLLER_X_LR:
                        return 1;
                    case SCROLLER_Y_TB:
                        return 2;
                    case SCROLLER_X_RL:
                        return 3;
                    case SCROLLER_Y_BT:
                        return 4;
                    case BLINKING:
                        return 5;
                    


                }

                return 0;


            }

            set
            {
                switch (value)
                {
                    case 0:
                        animation = NO_ANIMATION;
                        break;
                    case 1:
                        animation = SCROLLER_X_LR;
                        break;
                    case 2:
                        animation = SCROLLER_Y_TB;
                        break;
                    case 3:
                        animation = SCROLLER_X_RL;
                        break;
                    case 4:
                        animation = SCROLLER_Y_BT;
                        break;
                    case 5:
                        animation = BLINKING;
                        break;


                    default:
                        animation = NO_ANIMATION;
                        break;
                }

                


            }


        }

        [JsonIgnore]
        public int AnimationVelocity { get { return animation_velocity; } set { animation_velocity = value;if(PropertyChanged!=null) PropertyChanged(this, new PropertyChangedEventArgs("AnimationVelocity")); } }


        int[] fontsizearray = new int[] { 10, 12, 14, 16, 20, 24, 32, 40, 48, 60 };

        [JsonIgnore]
        public int FontSize
        {
            get
            {
                return Array.IndexOf<int>(fontsizearray,fontsize);
          
            }

            set
            {
                
                fontsize = fontsizearray[value];
            }
        }

        [JsonIgnore]
        public string PlayPauseIconButton
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return "ic_pause_white_24dp";
                    case STATUS_PAUSE:
                        return "ic_play_arrow_white_24dp";
                    case STATUS_STOP:
                        return "ic_play_arrow_white_24dp";

                }

                return "ic_play_arrow_white_24dp";
            }
        }


        [JsonIgnore]
        public string BackgroundStopIcon
        {
            get
            {
                switch (_status)
                {
                    case STATUS_STOP:
                        return "#99FFFFFF";
                    case STATUS_PLAY:
                        return "#F5A800";
                    case STATUS_PAUSE:
                        return "#F5A800";
                }
                return "#F5A800";
            }
        }


        [JsonIgnore]
        public bool IsVisibleStop
        {
            get
            {
                switch (status)
                {
                    case STATUS_PLAY:
                        return true;
                    case STATUS_PAUSE:
                        return true;
                    default:
                        return false;
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        public CustomText()
        {
        }

        public void sendMessageToPlay()
        {

            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }



            if (title == "" || title == null)
            {
                App.DisplayAlert(AppResources.insert_title);
                return;
            }

            //verifica che non ci sia un processo di un altro plugin attivo
            List<Activity> activity_display = App.avm.getActivitiesByType(new int[] { Activity.TYPE_PLAYLIST_IMAGE, Activity.TYPE_PRACTICE });

            if (activity_display.Count > 0)
            {
                App.DisplayAlert(AppResources.another_diplay_process_execute);
                return;
            }


            App.conn.SendMessage(App.api.createStartCustomTextMessage(this));
            //status = STATUS_PLAY;

        }


        public void sendMessageToPause()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }
            App.conn.SendMessage(App.api.createPauseCustomTextMessage(this));
            //status = STATUS_PAUSE;


        }

        public void sendMessageToStop()
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                App.DisplayAlert(AppResources.connecting_before_ledbox);
                return;
            }
            App.conn.SendMessage(App.api.createStopCustomTextMessage(title));
            //status = STATUS_STOP;


        }



    }
}
