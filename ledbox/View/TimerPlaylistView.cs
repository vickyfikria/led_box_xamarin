using System;

using Xamarin.Forms;

namespace ledbox.View
{
    public class TimerPlaylistView : ContentPage
    {
        public TimerPlaylistView()
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

