
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace ledbox.Droid
{
   [Activity(Label = "LEDbox", Icon = "@drawable/icon", Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory =true, ScreenOrientation = ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class SplashActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Splash);

            FindViewById<TextView>(Resource.Id.lblVersion).Text = $"Version {PackageManager.GetPackageInfo(PackageName, 0).VersionName}";


            Handler handler = new Handler();
            handler.PostDelayed(()=> {
                StartActivity(typeof(MainActivity));
            }, 1500);

            /*
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(3000);
                StartActivity(typeof(MainActivity));
                Finish();
            }).Start();*/
            
        }
    }
}
