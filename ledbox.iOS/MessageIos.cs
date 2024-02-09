using System;
using System.Threading.Tasks;
using GlobalToast;
using ledbox.iOS;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(MessageIos))]


namespace ledbox.iOS
{
    public class MessageIos:IMessage
    {
        public MessageIos()
        {
        }

        public void DisplayAlert(string message)
        {
            Toast.ShowToast(message);
        }

        public void LongAlert(string message)
        {
            Toast.ShowToast(message).SetDuration(2000);
        }

        public void PromptYesNo(string message, Action<bool> result, string yes_button = "Yes", string no_button = "No")
        {

            var tcs = new TaskCompletionSource<int>();
            var alert = new UIAlertView
            {
                Title = "",
                Message = message
            };
            
            alert.AddButton(yes_button);
            alert.AddButton(no_button);

            alert.Clicked += (s, e) => tcs.TrySetResult((int)e.ButtonIndex);
            alert.Show();
            if (tcs.Task.Result == 0)
            {
                result(true);
            }
            else
            {
                result(false);
            }


        }

        public void ShortAlert(string message)
        {
            Toast.ShowToast(message);
        }
    }
}
