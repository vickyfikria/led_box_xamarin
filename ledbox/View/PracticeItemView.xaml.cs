using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using ledbox.Resources;
using Acr.UserDialogs;

namespace ledbox
{
    public partial class PracticeItemView : ContentPage
    {


        public Practice practice;
        public bool onSaving = false;
        public PracticeItemViewModel pim;

        //ObservableCollection<PracticeItemViewModel> items = new ObservableCollection<PracticeItemViewModel>();

        List<ItemPractice> pollItemPractice;
        delegate void onEventItemPracticeFinish();
        event onEventItemPracticeFinish OnItemPracticeFinish;


        ItemPractice current_itemPractice;

        Color backgroundColor;

        public PracticeItemView(Practice practice, Color backgroundColor, bool isNew = false)
        {

            this.backgroundColor = backgroundColor;
            
            InitializeComponent();
            pim = new PracticeItemViewModel(this.Navigation, practice,isNew);
            this.BindingContext = pim;

            this.practice = practice;


            if (this.practice.isremote)
            {
                this.ToolbarItems.Clear();
            }
           

        }



        private async  void EditClicked(ItemPractice itemPractice)
        {
            PracticeItemDetailView pidv = new PracticeItemDetailView(itemPractice, backgroundColor, practice.Title, false);
            pidv.Disappearing+=(sender,obj) => {
                pim.reloadList();
            };

            await Navigation.PushAsync(pidv);
            
        }


        async void lstItems_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            ItemPractice p = (ItemPractice)e.Item;

            string action = await DisplayActionSheet(AppResources.action,AppResources.cancel,"",AppResources.edit, AppResources.duplicate, AppResources.move_up, AppResources.move_down, AppResources.delete);


            if(action==AppResources.edit) EditClicked(p);
            if (action == AppResources.duplicate) DuplicateFileClicked(p);
            if (action == AppResources.delete) DeleteFileClicked(p);
            if (action == AppResources.move_up) MoveUp_Clicked(p);
            if (action == AppResources.move_down) MoveDown_Clicked(p);

        }


        void uploadFile(ItemPractice p)
        {
            p.sendMessageShowPreview();

        }

        private async void AddItem(object sender, EventArgs e)
        {
            ItemPractice itemPractice =new ItemPractice();
            itemPractice.Round = 1;
            itemPractice.Work = 20;
            itemPractice.Rest = 5;
            itemPractice.SoundRest = 1;
            itemPractice.SoundWork = 2;

            practice.Items.Add(itemPractice);
            pim.reloadList();

            PracticeItemDetailView pidv = new PracticeItemDetailView(itemPractice, backgroundColor, practice.Title, true);
            pidv.Disappearing += (sen, obj) => {
                pim.reloadList();
                App.storage.saveFile();
                practice.setLastModified();
            };

            await Navigation.PushAsync(pidv);


            

        }

        private void DeleteFileClicked(ItemPractice itemPractice)
        {
            
            var vm = BindingContext as PracticeItemViewModel;
            vm.RemoveCommand(itemPractice);
            practice.setLastModified();


        }   

        private void DuplicateFileClicked(ItemPractice itemPractice)
        {

            var vm = BindingContext as PracticeItemViewModel;
            vm.DuplicateCommand(itemPractice);
            practice.setLastModified();


        }

        void MoveUp_Clicked(ItemPractice itemPractice)
        {
            
            var vm = BindingContext as PracticeItemViewModel;
            vm.movePlaylist(itemPractice, -1);

        }

        void MoveDown_Clicked(ItemPractice itemPractice)
        {
            var vm = BindingContext as PracticeItemViewModel;
            vm.movePlaylist(itemPractice, 1);

        }



        public void Play(object sender, EventArgs e)
        {

            /* if (lstItems.SelectedItem != null)
             {

                 current_itemPractice = lstItems.SelectedItem as ItemPractice;
                 if (current_itemPractice != null) //verifica se è stato selezionato un elemento nella lista
                 {
                     current_itemPractice.sendMessageRun((bool isFinish) =>
                     {

                     });
                 }
             }
             else //se invece non c'è seleziona fai partire tutti gli esercizi in sequenza
             {*/


            OnItemPracticeFinish = null;

                //carica tutti i file sul device
                pollItemPractice = new List<ItemPractice>(practice.Items);
                runAllItemPractice();
                OnItemPracticeFinish += (() =>
                {
                    current_itemPractice.sendMessageShowPreview();
                    current_itemPractice = null;



                });

           // }

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
            {
                current_itemPractice = pollItemPractice[0];

                //seleziona dalla lista l'esercizio corrente
                lstItems.SelectedItem = current_itemPractice;

                current_itemPractice.sendMessageRun((isFinish) =>
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
            }

        }

        public void Stop(object sender, EventArgs e)
        {
            if(current_itemPractice!=null)
                current_itemPractice.stop();


        }

        async void ChangeTitle_Clicked(object sender, System.EventArgs e)
        {

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
                Text = pim.Practice.Title
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                pim.Practice.Title = pResult.Text;
                this.Title = pim.Practice.Title;
                practice.setLastModified();


            }




        }



    }
}
