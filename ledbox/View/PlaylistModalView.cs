using System;

using Xamarin.Forms;

namespace ledbox
{
    public class PlaylistModalView : ContentPage
    {
        public PlaylistModalView()
        {
            Content = new StackLayout
            {
                Children = {
                    new Label { Text = "Hello ContentPage" }
                }
            };
        }
    }
}

