using Microsoft.Web.WebView2.WinForms;

namespace MiniPlayer
{
    public class AmazonCommands : StationCommands
    {
        public AmazonCommands(WebView2 webView) : base(webView)
        {

        }
        public override async void Dislike()
        {
            await InjectionFunctions.ClickElementAsync(webView, @"[data-qa='thumbs_down_button""]");
        }

        public override async void Like()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[aria-label='Like']", "[aria-label='Unlike']");
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
    }
}
