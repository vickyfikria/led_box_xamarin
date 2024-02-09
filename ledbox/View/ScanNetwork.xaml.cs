using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Xamarin.Forms;

namespace ledbox
{
    public partial class ScanNetwork : ContentPage
    {
        public ScanNetwork()
        {
            InitializeComponent();


        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PopModalAsync();
        }


    }
}
