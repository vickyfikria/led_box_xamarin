using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmHelpers;

namespace ledbox
{
    public class WeightGym : ObservableObject
    {



        public const int TYPE_AUDIO = 0;
        public const int TYPE_IMAGE = 1;



        private string title { get; set; }

        private int duration { get; set; }
        private int type { get; set; }
        private string alias { get; set; }
        private List<ItemPractice> items;
        private string status { get; set; }



        public string Title { get { return title; } set { title = value; } }
        public int Duration { get { return duration; } set { duration = value; } }
        public int Type { get { return type; } set { type = value; } }
        public string Status { get { return status; } set { status = value; } }
        public List<ItemPractice> Items { get { return items; } set { items = value; } }

        private List<ItemPractice> pollToUpload;
        delegate void onEventUploadFinish();
        event onEventUploadFinish OnUploadFinish;

        public void AddFile(ItemPractice fp)
        {
            if (items == null)
                items = new List<ItemPractice>();
            items.Add(fp);
        }



        public void sendMessageToPlay()
        {
            OnUploadFinish = null;

            //carica tutti i file sul device
            pollToUpload = new List<ItemPractice>(Items);
            uploadAllFiles();
            OnUploadFinish += (() => {

                //apri il layout corretto
                App.conn.SendMessage(App.api.createLayoutMessage("practice"));

                //per ogni elemento invia i messaggi per la visualizzazione
                foreach (ItemPractice item in Items)
                {
                    sendSection(item);
                }

                //crea ed invia il messaggio

                Status = "in esecuzione";

            });

        }


        async void sendSection(ItemPractice item)
        {
            for (int i = 0; i < item.Duration; i++)
            {
                await Task.Factory.StartNew(() => {

                    APILedbox.section[] sections = new APILedbox.section[2];

                    APILedbox.section round = new APILedbox.section();
                    round.name = "round";
                    round.attrib = "text";
                    round.value = item.Round.ToString();

                    sections[0] = round;

                    APILedbox.section time = new APILedbox.section();
                    time.name = "time";
                    time.attrib = "text";
                    time.value = (item.Duration - i).ToString();

                    sections[1] = time;

                    App.conn.SendMessage(App.api.createSectionsMessage(sections));


                });
                await Task.Delay(1000);
            }
        }

        public void sendMessageToStop()
        {

            App.conn.SendMessage(App.api.createStopPlaylistMessage());


        }

        private void uploadAllFiles()
        {
            //condizione di uscita
            if (pollToUpload == null || pollToUpload.Count == 0)
            {
                OnUploadFinish();
                return;
            }

            //prendi il primo file e caricalo
            if (pollToUpload.Count > 0)
                pollToUpload[0].upload((isFinish) =>
                {
                    pollToUpload.RemoveAt(0);
                    uploadAllFiles();
                });


        }





    }
}
