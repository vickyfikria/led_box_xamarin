using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using ledbox.Resources;
using Xamarin.Forms;

namespace ledbox
{
    public partial class CustomView : ContentPage
    {


        public CustomText customText;
        public CustomViewModel cvm;

        public CustomView(CustomText customText, bool isNew = false)
        {
            InitializeComponent();

            //popolamento testi nei picker
            fontcolor.Items.Add(AppResources.white);
            fontcolor.Items.Add(AppResources.red);
            fontcolor.Items.Add(AppResources.yellow);
            fontcolor.Items.Add(AppResources.blue);
            fontcolor.Items.Add(AppResources.orange);
            fontcolor.Items.Add(AppResources.purple);
            fontcolor.Items.Add(AppResources.green);
            animation.Items.Add(AppResources.none);
            animation.Items.Add(AppResources.scroller_left_right);
            animation.Items.Add(AppResources.scroller_top_bottom);
            animation.Items.Add(AppResources.scroller_right_left);
            animation.Items.Add(AppResources.scroller_bottom_top);
            animation.Items.Add(AppResources.blinking);

            cvm = new CustomViewModel(this.Navigation, customText, isNew);
            this.BindingContext = cvm;
            this.customText = customText;


        }
        /*
        void Bt_Send_Clicked(object sender, System.EventArgs e)
        {

           
            MessagingCenter.Subscribe<APILedbox, string>(App.api, "layout_loaded", ((senderApi, layout) =>
            {

                if (layout == "custom_text")
                {
                    if (customText.status == CustomText.STATUS_PLAY)
                    {
                        customText.status = CustomText.STATUS_STOP;
                        App.conn.SendMessage(App.api.createLayoutMessage("waiting"));

                    }
                    else
                    {



                        APILedbox.section[] sections = new APILedbox.section[6];

                        sections[0] = new APILedbox.section();
                        sections[0].name = "custom";
                        sections[0].value = new APILedbox.section_value() { attrib = "text", value = textarea.Text };

                        sections[1] = new APILedbox.section();
                        sections[1].name = "custom";
                        sections[1].value = new APILedbox.section_value() { attrib = "fontsize", value = fontsize.Items[fontsize.SelectedIndex] };

                        string animation_value = "";
                        switch (animation.SelectedIndex)
                        {
                            case 0:
                                animation_value = "";
                                break;
                            case 1:
                                animation_value = "scroller_x";
                                break;
                            case 2:
                                animation_value = "scroller_y";
                                break;
                        }

                        sections[2] = new APILedbox.section();
                        sections[2].name = "custom";
                        sections[2].value = new APILedbox.section_value() { attrib = "animation", value = animation_value };

                        sections[3] = new APILedbox.section();
                        sections[3].name = "custom";
                        sections[3].value = new APILedbox.section_value() { attrib = "x", value = "0" };

                        sections[4] = new APILedbox.section();
                        sections[4].name = "custom";
                        sections[4].value = new APILedbox.section_value() { attrib = "y", value = "0" };

                        sections[5] = new APILedbox.section();
                        sections[5].name = "custom";
                        sections[5].value = new APILedbox.section_value() { attrib = "color", value = customText.color };


                        App.conn.SendMessage(App.api.createSectionsMessage(sections));

                        customText.status = CustomText.STATUS_PLAY;
                    }
                }

                MessagingCenter.Unsubscribe<APILedbox, string>(App.api, "layout_loaded");
            }));


            App.conn.SendMessage(App.api.createLayoutMessage("custom_text"));


        }*/

        async void ChangeTitle_Clicked(object sender, System.EventArgs e)
        {

            PromptResult pResult = await UserDialogs.Instance.PromptAsync(new PromptConfig
            {
                InputType = InputType.Name,
                OkText = AppResources.ok,
                CancelText = AppResources.cancel,
                Title = AppResources.insert_title,
                Text = cvm.customText.Title
            });

            if (pResult.Ok && !string.IsNullOrWhiteSpace(pResult.Text))
            {
                cvm.customText.Title = pResult.Text;
                this.Title = cvm.customText.Title;

                
            }




        }


    }
}
