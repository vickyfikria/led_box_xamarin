using System;
using Xamarin.Forms;
using Android.Content;
using Android.Net;
using ledbox.Droid;
using Java.Net;
using Java.IO;
using Org.Apache.Http.Util;
using Android.OS;

using Java.Lang;

[assembly: Dependency(typeof(WifiAndroid))]

namespace ledbox.Droid
{
    public class WifiAndroid : IWifi
    {

        public static Context _context = Android.App.Application.Context;


        public WifiAndroid()
        {
        }



        public Socket createSocketOverWifi()
        {
            ConnectivityManager connectivity = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);

            if (connectivity != null)
            {
                foreach (Network network in connectivity.GetAllNetworks())
                {
                    NetworkInfo networkInfo = connectivity.GetNetworkInfo(network);

                    if (networkInfo != null && networkInfo.GetType().ToString() == "WIFI")
                    {
                        if (networkInfo.IsConnected)
                        {
                            Socket sock = network.SocketFactory.CreateSocket();
                            return sock;
                        }
                    }
                }
            }

            return null;
        }

        [Obsolete]
        Network GetCurrentNetwork(string type = "WIFI")
        {
            ConnectivityManager connectivity = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);

            if (connectivity != null)
            {
                foreach (Network network in connectivity.GetAllNetworks())
                {
                    NetworkInfo networkInfo = connectivity.GetNetworkInfo(network);

                    string type_connection = networkInfo.Type.ToString().ToUpper();

                    if (networkInfo != null && type_connection.Contains(type))
                    {
                        if (networkInfo.IsConnected)
                        {
                            LinkProperties linkProperties = connectivity.GetLinkProperties(network);
                            
                            return network;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Forces the wifi over cellular.
        /// </summary>
        public void ForceWifiOverCellular()
        {
            ConnectivityManager connection_manager = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);

            NetworkRequest.Builder request = new NetworkRequest.Builder();
            request.AddTransportType(TransportType.Wifi);


            connection_manager.RegisterNetworkCallback(request.Build(), new CustomNetworkAvailableCallBack());
           



        }

        /// <summary>
        /// Forces the cellular over wifi.
        /// </summary>
        public void ForceCellularOverWifi()
        {
            ConnectivityManager connection_manager = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);
            
            NetworkRequest.Builder request = new NetworkRequest.Builder();
            request.AddTransportType(TransportType.Cellular);
            request.AddCapability(NetCapability.Internet);

            connection_manager.RegisterNetworkCallback(request.Build(), new CustomNetworkAvailableCallBack());


            connection_manager.NetworkPreference = ConnectivityType.Mobile;

            /*
            Thread t = new Thread(new Runnable(() =>
            {
                test(network);
            }));
            t.Start();
            */







    }



         string test(Network network)
        {
            HttpURLConnection urlConnection = null;
            string result = "";
            try
            {
                URL url = new URL("http://www.google.com");
                urlConnection =  (HttpURLConnection)network.OpenConnection(url);

         
                
                System.IO.Stream istream = urlConnection.InputStream;
                if (istream != null) {
                    BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(istream));
                    string line = "";

                    while ((line = bufferedReader.ReadLine()) != null)
                        result += line;
                }
                istream.Close();
                

                return result;
            }
            catch (MalformedURLException e)
            {
                e.PrintStackTrace();
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }

            finally
            {
                
            }
            return result;

        }



        

    }




    /// <summary>
    /// Custom network available call back.
    /// </summary>
    public class CustomNetworkAvailableCallBack : ConnectivityManager.NetworkCallback
    {
        public static Context _context = Android.App.Application.Context;

        ConnectivityManager connection_manager = (ConnectivityManager)_context.GetSystemService(Context.ConnectivityService);

        public override void OnAvailable(Network network)
            
        {



            //ConnectivityManager.SetProcessDefaultNetwork(network);    //deprecated (but works even in Android P)
            try
            {

                connection_manager.BindProcessToNetwork(network);           //this works in Android P
                connection_manager.UnregisterNetworkCallback(this);
            }
            catch(IOException ex)
            {
                System.Console.WriteLine(ex.ToString());
            }
           
        }
    }

   
}
