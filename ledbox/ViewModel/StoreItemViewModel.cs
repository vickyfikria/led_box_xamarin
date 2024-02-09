using System;
using MvvmHelpers;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using System.Collections.ObjectModel;


namespace ledbox
{
    public class StoreItemViewModel : BaseViewModel, INotifyPropertyChanged
    {


       
        private INavigation Navigation;
        public StoreItem storeItem { get; private set; }

       
        public StoreItemViewModel(INavigation navigation, StoreItem storeItem)
        {
            if (storeItem == null)
                storeItem = new StoreItem();

            this.storeItem = storeItem;


            this.Navigation = navigation;
        }
 

    }
}
