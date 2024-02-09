using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Acr.UserDialogs;

namespace ledbox.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.


            UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(82, 83, 74);  //#52534A;
            UINavigationBar.Appearance.TintColor = UIColor.White;

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
