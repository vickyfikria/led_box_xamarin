using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ledbox
{
    public partial class PracticeRunView : ContentPage
    {

        Practice practice;




       

        public PracticeRunView(Practice practice)
        {

            this.practice = practice;
            InitializeComponent();
            BindingContext = practice;






           

        }


        List<ItemPractice> pollItemPractice;
        delegate void onEventItemPracticeFinish();
        event onEventItemPracticeFinish OnItemPracticeFinish;

        /*
        public void run(object sender, EventArgs e)
        {
           
            
                //apri il layout corretto

                running = true;

                lbl_title.Text = practice.Title;

                tokenSource = new CancellationTokenSource();

                OnItemPracticeFinish = null;

                //carica tutti i file sul device
                pollItemPractice = new List<ItemPractice>(practice.Items);
                runAllItemPractice();
                OnItemPracticeFinish += (() => {
                    //apri il layout di conclusione
                    App.conn.SendMessage(App.api.createLayoutMessage("practice_finish"));


                   

                });
            


        }

        private void runAllItemPractice()
        {
            //condizione di uscita
            if (pollItemPractice == null || pollItemPractice.Count == 0)
            {
                OnItemPracticeFinish();
                return;
            }

            //prendi il primo file e caricalo
            if (pollItemPractice.Count > 0)
                sendSection(tokenSource.Token,pollItemPractice[0],(isFinish) =>
                {
                    if (isFinish)
                    {
                        if (pollItemPractice.Count > 0)
                        {
                            pollItemPractice.RemoveAt(0);
                            runAllItemPractice();
                        }
                    }

                });


        }*/





        void Bt_Ok_Clicked(object sender, System.EventArgs e)
        {

            Navigation.PopModalAsync();
        }
    }
}
