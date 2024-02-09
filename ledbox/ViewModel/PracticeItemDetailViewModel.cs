using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.IO;

namespace ledbox
{
    public class PracticeItemDetailViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public ICommand DoneEditingCommand { get; private set; }
       

        private INavigation Navigation;
        private bool isNew = false;
        public ItemPractice itemPractice { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;


        public PracticeItemDetailViewModel(INavigation navigation, ItemPractice itemPractice, bool isNew)
        {
            this.isNew = isNew;
            if (itemPractice == null)
            {
                itemPractice = new ItemPractice();
               
            }

            this.itemPractice = itemPractice;

             DoneEditingCommand = new Command(DoneEditing);



            this.Navigation = navigation;
        }


        private async void DoneEditing()
        {
            if (isNew)
                MessagingCenter.Send<ItemPractice>(this.itemPractice, "added");
            else
                MessagingCenter.Send<ItemPractice>(this.itemPractice, "edited");

           
            await this.Navigation.PopAsync(false);
            App.storage.saveFile();



        }


        public void getPreset()
        {

            PracticePresetsView ppv = new PracticePresetsView();


            ppv.Disappearing += (o, s) =>
            {

                string path = ppv.presentSelected.File;
                if (path=="" )
                    return;

                itemPractice.Type = ItemPractice.TYPE_IMAGE;
                itemPractice.Filename = ppv.presentSelected.file;
                itemPractice.Filepath = path;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("itemPractice"));
                itemPractice.is_changed = true;
            };

            this.Navigation.PushAsync(ppv);

            
        }



        public void AddPhoto()
        {

            helper.AddPhoto((path) =>
            {

                if (path != "")
                {
                    itemPractice.Type = ItemPractice.TYPE_IMAGE;
                    itemPractice.Filename = Path.GetFileName(path);
                    itemPractice.Filepath = path;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("itemPractice"));
                    itemPractice.is_changed = true;
                }
            });

        }

       

        /*
        public async void AddVideo()
        {

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                Console.WriteLine("Error");
                return;
            }

            MediaFile file = await CrossMedia.Current.PickVideoAsync();
            if (file != null)
            {
                FilePlaylist fp = new FilePlaylist();
                itemPractice.Type = FilePlaylist.TYPE_VIDEO;
                itemPractice.Filename = Path.GetFileName(file.Path);
                itemPractice.Filepath = file.Path;


            }
        }

        public async void Camera()
        {

            await CrossMedia.Current.Initialize();
            if (!CrossMedia.Current.IsCameraAvailable)
            {
                Console.WriteLine("Error");
                return;
            }


            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());
            if (file != null)
            {
                itemPractice.Type = FilePlaylist.TYPE_VIDEO;
                itemPractice.Filename = Path.GetFileName(file.Path);
                itemPractice.Filepath = file.Path;

            }
        }*/



    }
}
