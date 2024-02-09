using ledbox;
using ledbox.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using System;
using Android.Webkit;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace ledbox.Droid
{
    public class WebViewRender : ViewRenderer<WebViewer, Android.Webkit.WebView>
    {
        //function invokeLocalAction(data){jsBridge.invokeLocalCommand(data);}
        const string JavascriptFunction = "function invokeCSharpAction(data){jsBridge.invokeAction(data);} connected();";
        Context _context;

        public WebViewRender(Context context) : base(context)
        {
            _context = context;
        }

        [Obsolete]
        protected override void OnElementChanged(ElementChangedEventArgs<WebViewer> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                var webView = new Android.Webkit.WebView(_context);
                webView.SetWebChromeClient(new WebChromeClient());
                webView.LayoutParameters = new Android.Widget.AbsoluteLayout.LayoutParams(
                                                    LayoutParams.MatchParent,
                                                    LayoutParams.MatchParent,
                                                    0,
                                                    0
                                           );

                webView.Settings.JavaScriptEnabled = true;
               
                webView.Settings.DomStorageEnabled = true;
                webView.Settings.LoadWithOverviewMode = false;
                webView.Settings.AllowContentAccess = true;
                webView.Settings.AllowFileAccess = true;
                webView.Settings.AllowFileAccessFromFileURLs = true;
                webView.Settings.AllowUniversalAccessFromFileURLs = true;
                webView.Settings.CacheMode = CacheModes.NoCache;
                webView.ClearCache(true);
                //webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: {JavascriptFunction}"));
                webView.SetWebViewClient(new JavascriptWebViewClient($"javascript: "+JavascriptFunction));

                SetNativeControl(webView);
            }
            if (e.OldElement != null)
            {
                Control.RemoveJavascriptInterface("jsBridge");
                var hybridWebView = e.OldElement as WebViewer;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {
                Control.AddJavascriptInterface(new JSBridge(this), "jsBridge");

                string p = "file://" + Element.Uri;
                Control.LoadUrl(p);
                
               

                e.NewElement.EvalJavascript = async (js) =>
                {
                    Control.EvaluateJavascript(js,null);
                    return "";
                };
            }
        }
    }
}
