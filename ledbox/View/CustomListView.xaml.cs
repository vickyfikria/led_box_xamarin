using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using ledbox.Resources;
using Xamarin.Forms;

namespace ledbox
{
    public partial class CustomListView : ContentPage
    {

        CustomListViewModel cvm;

        public CustomListView()
        {
            InitializeComponent();

            cvm = new CustomListViewModel(this.Navigation);
            BindingContext = cvm;
        }

        private async void AddClicked(object sender, EventArgs e)
        {
            CustomText c = new CustomText();

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText=AppResources.cancel,
                Title = AppResources.insert_title,
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                c.Title = pResult.Text;
                App.storage.addCustomText(c);

                await openCustomView(c, true);
            }


            
        }

        private async void EditClicked(object sender, EventArgs e)
        {
            var button = sender as MenuItem;
            CustomText customText = button.BindingContext as CustomText;

            await openCustomView(customText);

        }

        private void DeleteClicked(object sender, EventArgs e)
        {
            var button = sender as MenuItem;
            var customtext = button.BindingContext as CustomText;
            var vm = BindingContext as CustomListViewModel;
            vm.RemoveCommand(customtext);
            cvm.NotifyChange();


        }


        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

            CustomText customText = e.Item as CustomText;
            await openCustomView(customText);
        }

            async System.Threading.Tasks.Task openCustomView(CustomText customText, bool isNew = false)
        {
            CustomView cv = new CustomView(customText);

            cv.Disappearing += (object sender, EventArgs e) => {
                //if(App.conn.isConnected())
                    //App.conn.SendMessage(App.api.createLayoutMessage("waiting"));
                cv.cvm.DoneEditing();
                cvm.reloadList();
                cvm.NotifyChange();
            };

            await this.Navigation.PushAsync(cv);
        }

    }
}
