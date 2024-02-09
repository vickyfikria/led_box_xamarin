using System;
using Android.Webkit;
using Java.Interop;

namespace ledbox.Droid
{
	public class JSBridge : Java.Lang.Object
	{
		readonly WeakReference<WebViewRender> hybridWebViewRenderer;

		public JSBridge (WebViewRender hybridRenderer)
		{
			hybridWebViewRenderer = new WeakReference <WebViewRender> (hybridRenderer);
		}

		[JavascriptInterface]
		[Export ("invokeAction")]
		public void InvokeAction (string data)
		{
            WebViewRender hybridRenderer;

			if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget (out hybridRenderer)) 
            {
				hybridRenderer.Element.InvokeAction (data);
			}
		}

        
    }
}

