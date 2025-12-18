using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;

namespace MiniPlayer
{
    public class AmazonCommands : StationCommands
    {
        public AmazonCommands(WebView2 webView) : base(webView)
        {

        }
        public override async void Dislike()
        {
            var element = await InjectionFunctions.FindElement(webView, "ContextMenu", @"[aria-label=""Context Menu""]");
            if ((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, "ContextMenu");
                await Task.Delay(200);
                element = await InjectionFunctions.FindElement(webView, "Dislike", @"[primary-text=""Dislike""]");
                await InjectionFunctions.ClickElementAsync(webView, "Dislike");
            }
        }

        public override async void Like()
        {
            var  element = await InjectionFunctions.FindElement(webView, "Like", @"[aria-label=""Like""]");
            if((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, "Like");
                return;
            }
            element = await InjectionFunctions.FindElement(webView, "Unlike", @"[aria-label=""Unlike""]");
            if ((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, "Unlike");

            }
        }

        public override async void Next()
        {
        }

        public override async void Play()
        {

        }

        public override async void Previous()
        {
        }

        public override async void AdjustStyle()
        {
            await base.AdjustStyle();

            await InjectionFunctions.FindElement(webView, "ContextMenu", @"[aria-label=""Context Menu""]");

        }
    }
}
