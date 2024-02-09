using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Xamarin.Forms;
using ledbox.ViewModels;
using Acr.UserDialogs;
using ledbox.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ledbox
{
    public partial class PracticeView : ContentPage
    {

        PracticeViewModel pvm;

        Color backgroundColor;

        
        public PracticeView(string title, Color backgroundColor,int id_category)
        {

            this.backgroundColor = backgroundColor;

            Device.BeginInvokeOnMainThread(() =>
            {
                Title = title;
                lstView.BackgroundColor = backgroundColor;
            });
            InitializeComponent();
            pvm = new PracticeViewModel(this.Navigation,id_category);
            BindingContext = pvm;
    
        }

        private async void AddClicked(object sender, EventArgs e)
        {
            Practice p = new Practice();
            p.Category = pvm.id_category;

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                p.Title = pResult.Text;
                App.storage.addPractice(p);
                await openPracticeItemView(p);
            }

            
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            Practice practice = e.Item as Practice;


            await openPracticeItemView(practice);
        }


        async System.Threading.Tasks.Task openPracticeItemView(Practice practice)
        {

            PracticeItemView piv = new PracticeItemView(practice,backgroundColor);
            piv.Disappearing +=(object sender, EventArgs e) =>  {
                piv.pim.DoneEditing();
                pvm.reloadList();
                pvm.NotifyChange();
            };


            await this.Navigation.PushAsync(piv);
        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            var button = sender as MenuItem;
            var file = button.BindingContext as Practice;
            var vm = BindingContext as PracticeViewModel;
            vm.RemoveCommand(file);
            pvm.NotifyChange();


        }

        async void Bt_Store_Clicked(object sender, System.EventArgs e)
        {

            if (!App.isInternetConnection(true))
            {
                return;
            }

            StoreView sv = new StoreView(webservice.PRACTICE_CATEGORY);
            sv.Disappearing += (s, obj) =>
            {
                pvm.reloadList();
                pvm.NotifyChange();

            };


            await Navigation.PushAsync(sv);

        }

    }
}
