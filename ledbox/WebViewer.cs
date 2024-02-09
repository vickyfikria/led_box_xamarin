using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ledbox
{
    public class WebViewer : WebView
    {

        Action<string> action;



        public static readonly BindableProperty UriProperty = BindableProperty.Create(
    propertyName: "Uri",
    returnType: typeof(string),
    declaringType: typeof(WebViewer),
    defaultValue: default(string));

        public string Uri
        {
            get { return (string)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        

        public static BindableProperty EvaluateJavascriptProperty =
    BindableProperty.Create(nameof(EvalJavascript), typeof(Func<string,Task<string>>), typeof(WebViewer), null, BindingMode.OneWayToSource);
        public Func<string,Task<string>> EvalJavascript
        {
            get
            {
                return (Func<string,Task<string>>)GetValue(EvaluateJavascriptProperty);
            }
            set
            {
                SetValue(EvaluateJavascriptProperty, value);
            }
        }

        public static BindableProperty AllowFileAccessProperty =
                     BindableProperty.Create(nameof(AllowFileAccess),
                     typeof(bool),
                     typeof(WebViewer),
                     null);

        public static BindableProperty AllowContentAccessProperty =
                        BindableProperty.Create(nameof(AllowContentAccess),
                        typeof(bool),
                        typeof(WebViewer),
                        null);


        public bool AllowContentAccess
        {
            get => (bool)GetValue(AllowContentAccessProperty);
            set => SetValue(AllowContentAccessProperty, value);
        }


        public Action Refresh
        {
            get { return (Action)GetValue(RefreshProperty); }
            set { SetValue(RefreshProperty, value); }
        }

        public static BindableProperty RefreshProperty =
        BindableProperty.Create(nameof(Refresh), typeof(Action), typeof(WebViewer), null, BindingMode.OneWayToSource);

        public static BindableProperty GoBackProperty =
        BindableProperty.Create(nameof(GoBack), typeof(Action), typeof(WebViewer), null, BindingMode.OneWayToSource);

        public Action GoBack
        {
            get { return (Action)GetValue(GoBackProperty); }
            set { SetValue(GoBackProperty, value); }
        }

        public static BindableProperty CanGoBackFunctionProperty =
        BindableProperty.Create(nameof(CanGoBackFunction), typeof(Func<bool>), typeof(WebViewer), null, BindingMode.OneWayToSource);

        public Func<bool> CanGoBackFunction
        {
            get { return (Func<bool>)GetValue(CanGoBackFunctionProperty); }
            set { SetValue(CanGoBackFunctionProperty, value); }
        }

        public static BindableProperty GoBackNavigationProperty =
        BindableProperty.Create(nameof(GoBackNavigation), typeof(Action), typeof(WebViewer), null, BindingMode.OneWay);

        public Action GoBackNavigation
        {
            get { return (Action)GetValue(GoBackNavigationProperty); }
            set { SetValue(GoBackNavigationProperty, value); }
        }

        public void RegisterAction(Action<string> callback)
        {
            action = callback;
        }

      
        public void Cleanup()
        {
            action = null;
        }

        public void InvokeAction(string data)
        {
            if (action == null || data == null)
            {
                return;
            }
            action.Invoke(data);

            

            
        }

        public bool AllowFileAccess
        {
            get => (bool)GetValue(AllowFileAccessProperty);
            set => SetValue(AllowFileAccessProperty, value);
        }

    }
}