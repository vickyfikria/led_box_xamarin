using ledbox;
using ledbox.iOS.Render;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(WebViewer), typeof(WebViewRender))]
namespace ledbox.iOS.Render
{
    using System.Threading.Tasks;
    using Foundation;
    using UIKit;
    using WebKit;
    using Xamarin.Forms.Platform.iOS;
  

    public class WebViewRender :ViewRenderer<WebViewer, WKWebView>, IWKScriptMessageHandler
    {
        const string JavaScriptFunction = "function invokeCSharpAction(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
        WKUserContentController userController;

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
                var hybridWebView = e.OldElement as WebViewer;
                hybridWebView.Cleanup();
            }
            if (e.NewElement != null)
            {
                Control.LoadRequest(new NSUrlRequest(new NSUrl(Element.Uri, false)));

                e.NewElement.EvalJavascript = async (js) =>
                {
                    var result= await Control.EvaluateJavaScriptAsync(js);
                    return "";
                }; 


            }


        }



        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {

            Element.InvokeAction(message.Body.ToString());
        }

    }

}