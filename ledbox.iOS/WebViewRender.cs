using ledbox;
using ledbox.iOS;

using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace ledbox.iOS
{
    public class WebViewRender : ViewRenderer<WebViewer, WKWebView>, IWKScriptMessageHandler
    {
        //function invokeLocalAction(data){jsBridge.invokeLocalCommand(data);}
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);} connected();";
        WKUserContentController userController;

        /*
        public WebViewRender() : this(new WKWebViewConfiguration())
        {
            
        }

        public WebViewRender(WKWebViewConfiguration config) : base(config)
        {
            userController = config.UserContentController;
            var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
            userController.AddUserScript(script);
            userController.AddScriptMessageHandler(this, "invokeAction");
            
        }

       
        */
        protected override void OnElementChanged(ElementChangedEventArgs<WebViewer> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                userController = new WKUserContentController();
                var script = new WKUserScript(new NSString(JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
                userController.AddUserScript(script);
                userController.AddScriptMessageHandler(this, "invokeAction");

                var config = new WKWebViewConfiguration { UserContentController = userController };
                var webView = new WKWebView(Frame, config);
                
                SetNativeControl(webView);
            }

            if (e.OldElement != null)
            {
                userController.RemoveAllUserScripts();
                userController.RemoveScriptMessageHandler("invokeAction");
                WebViewer hybridWebView = e.OldElement as WebViewer;
                hybridWebView.Cleanup();
                
            }

            if (e.NewElement != null)
            {

               
                string filename = ((WebViewer)Element).Uri;

                string querystring = filename.Substring(filename.IndexOf("?")+1, filename.Length - filename.IndexOf("?")-1);

                filename =filename.Replace("?"+querystring,"");

                var request = new NSMutableUrlRequest(new NSUrl("file://"+filename+"?"+querystring));
                
                request.Body = querystring;
                request.HttpMethod = "GET";
                Control.LoadRequest(request);

                e.NewElement.EvalJavascript = async (js) =>
                {
                    Control.EvaluateJavaScript(js, null);
                    return "";
                };


               
                
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            ((WebViewer)Element).InvokeAction(message.Body.ToString());
        }

        

        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((WebViewer)Element).Cleanup();
            }
            base.Dispose(disposing);
        }
    }
}
