using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ledbox
{
    public partial class PracticePresetsView : ContentPage
    {

        PracticePresetsViewModel ppvm;
        public PracticePreset presentSelected;

        public PracticePresetsView()
        {
            InitializeComponent();
            ppvm = new PracticePresetsViewModel(this.Navigation);
            BindingContext = ppvm;
            presentSelected = new PracticePreset();

        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            presentSelected = e.Item as PracticePreset;
            closeWindow();

        }


        async void closeWindow()
        {
            await Navigation.PopAsync(false);
        }
    }
}
