using System;
using System.Collections.Generic;
using System.IO;
using ledbox.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xam.Plugin.WebView;
using Xamarin.Forms;

namespace ledbox
{
    public partial class InterfaceView : ContentPage
    {

       

        StoreItem interfaceFile;


        public InterfaceView(StoreItem interfaceFile,bool isTest=false)
        {

            if((App.conn==null || !App.conn.isConnected()) && !isTest)
            {
               
                return;
            }

            this.interfaceFile = interfaceFile;

            InitializeComponent();

            
            wview.Uri = interfaceFile.local_file+"?language="+App.lang.ToString()+"&type_connection="+(App.conn!=null?App.conn.getType():"")+"&address="+(App.conn!=null?App.conn.getAddress():"");
            //this.Title = interfaceFile.name;
            //this.TitleInterface.Text= interfaceFile.name;
            wview.AllowFileAccess = true;
            wview.AllowContentAccess = true;
            this.Title = interfaceFile.label_title;
            this.TitleInterface.Text= interfaceFile.label_title;



            wview.RegisterAction((data) => {

                JObject jo = JsonConvert.DeserializeObject<JObject>(data);

                if (jo.ContainsKey("cmd"))
                {
                    if (jo["cmd"].ToString() == "local")
                    {
                        
                        switch (jo["value"].ToString())
                        {
                            case "openAdv":
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    openAdv();
                                });
                                break;
                            case "selectImageFile":
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    selectImageFile(jo["name"].ToString());
                                });
                                break;
                            case "uploadToLedbox":
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    uploadToLedbox(jo["name"].ToString());
                                });
                                break;
                            case "back":
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    Navigation.PopAsync();
                                });
                                break;
                        }

                        return;
                    }
                }
                if(App.conn!=null)
                    App.conn.SendMessage(data);
            });


           



        }

        private async void refresh(object sender, EventArgs e)
        {
            if (wview != null)
                wview.EvalJavascript("openLayout(default_layout);sleep(200);refreshValue();");
        }


        async void openAdv()
        {
            await Navigation.PushAsync(new PlaylistView(""), false);

        }

        async void selectImageFile(string name)
        {
            helper.AddPhoto((path) => {

                string filename = Path.GetFileName(path);

                //copia il file nella cartella dell'interfaccia
                string dir_tmp = interfaceFile.executive_directory + "/tmp";
                if (!Directory.Exists(dir_tmp))
                    Directory.CreateDirectory(dir_tmp);

                DependencyService.Get<IDirectory>().copyFile(path, dir_tmp + "/" + filename, false, true);

                APILedbox.file file = new APILedbox.file();
                file.filepath = "tmp/" + filename;
                file.filename = filename;
                //file.alias = App.alias;
                

                processMessage(App.api.createFileImageSelected(file,name));

            });
        }

        async void uploadToLedbox(string name)
        {
            name = name.Replace("tmp/", "");
            App.conn.startUploadFile(name, interfaceFile.executive_directory + "/tmp/" + name, App.alias, (isFinish) =>
            {
                processMessage(App.api.createUplodedMessage(isFinish));
            },"media");
        }






        protected override void OnAppearing()
        {
            base.OnAppearing();
            MessagingCenter.Subscribe<APILedbox, string>(App.api, "messageReceived", (sender, message) => {
                processMessage(message);

            });

        }



        async void BackButton(object sender, EventArgs e)
        {
            if (App.conn == null || !App.conn.isConnected())
            {
                await Navigation.PopAsync();
                return;
            }

            if (wview != null)
                wview.EvalJavascript("Back();");

            
        }

        void processMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    string messageCorrect = message.Replace("'", "\\'");

                    if (wview != null)
                        wview.EvalJavascript("ws.processMessage('" + messageCorrect + "');");
                }
                catch
                {

                }
            });
        }


       
    }
}
