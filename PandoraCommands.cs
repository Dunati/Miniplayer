using Microsoft.Web.WebView2.WinForms;

namespace MiniPlayer
{

    public class PandoraCommands : StationCommands
    {
        public PandoraCommands(WebView2 webView) : base(webView)
        {

        }
        public override async void Dislike()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='thumbs_down_button']");
        }

        public override async void Like()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='thumbs_up_button']");
        }

        public override async void Next()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='skip_button']");
        }

        public override async void Play()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='play_button']", "[data-qa='pause_button']");
        }

        public override async void Previous()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='replay_button']");
        }
    }
}
