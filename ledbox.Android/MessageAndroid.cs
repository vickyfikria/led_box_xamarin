using System;
using Android.App;
using Android.Widget;
using ledbox.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(MessageAndroid))]

namespace ledbox.Droid
{
    public class MessageAndroid:IMessage
    {
        public MessageAndroid()
        {

        }

        public void LongAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Long).Show();
        }

        public void ShortAlert(string message)
        {
            Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
        }


        public void DisplayAlert(string message)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Application.Context);

            AlertDialog alert = builder.Create();
            alert.SetTitle("Title");
            alert.SetMessage(message);
            alert.SetButton("OK", (c, ev) =>
            {
                // Ok button click task  
            });
            alert.Show();


           

        }


        public void PromptYesNo(string message,Action<bool> result, string yes_button = "Yes", string no_button = "No")
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Application.Context);
            builder.SetMessage(message);

            builder.SetPositiveButton(yes_button, (sent, args) =>
            {
                result(true);

            });
            builder.SetNegativeButton(no_button, (sent, args) =>
            {
                result(false);

            });

            builder.Create();
            builder.Show();

            
        }

    }
}
