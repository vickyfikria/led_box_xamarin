using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ledbox
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PracticeTabbedView : TabbedPage
    {

        public PracticeTabbedView(string title, Color backgroundColor, int id_category)
        {
            InitializeComponent();

            this.Title = title;

            this.Children.Add(new PracticeView("sul tuo dispositivo", backgroundColor, id_category));
            this.Children.Add(new PracticeLedboxView("sul LEDbox", backgroundColor, id_category));

        }


       
    }
}
