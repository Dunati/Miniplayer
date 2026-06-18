using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using System.Xml.Linq;

namespace MiniPlayer
{
    public class AmazonCommands : StationCommands
    {
        private const string uri = "https://music.amazon.com/";

        static void Register()
        {
            Register(uri, (WebView2 view) => new AmazonCommands(view));
        }
        public override String Uri { get; } = uri;
        public AmazonCommands(WebView2 webView) : base(webView)
        {
            Color = Color.FromArgb(25, 25, 25);
        }
        public override async void Dislike()
        {
            const string selector = @"[aria-label=""Context Menu""]";
            var element = await InjectionFunctions.FindElement(webView, selector);
            if ((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, selector);
                await Task.Delay(200);
                element = await _dislike();
            }
        }

        private async Task<CachedElementState> _dislike()
        {
            const string selector = @"[primary-text=""Dislike""]";
            CachedElementState element = await InjectionFunctions.FindElement(webView, selector);
            await InjectionFunctions.ClickElementAsync(webView, selector);
            return element;
        }

        public override async void Like()
        {
            var element = await InjectionFunctions.FindElement(webView, "#overlay", @"[aria-label=""Unlike""]");
            if ((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, "#overlay", @"[aria-label=""Unlike""]");
                return;
            }
            await _like();
        }

        private async Task _like()
        {
            CachedElementState element = await InjectionFunctions.FindElement(webView, "#overlay", @"[aria-label=""Like""]");
            if ((element & CachedElementState.Found) != 0)
            {
                await InjectionFunctions.ClickElementAsync(webView, "#overlay", @"[aria-label=""Like""]");
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
        private static string? likeOverlayJs;

        public override async Task AdjustStyle()
        {
            await base.AdjustStyle();

            await webView.ExecuteScriptAsync($"window.__mpZoom = {Zoom.ToString(System.Globalization.CultureInfo.InvariantCulture)};");
            likeOverlayJs ??= ResourceLoader.ReadText("amazon.js");
            await webView.ExecuteScriptAsync(likeOverlayJs);

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(500);
                var element = await InjectionFunctions.FindElement(webView, @"#overlay", @":scope > div", @":scope > div:nth-child(3)", @"[aria-label=play]");
                if ((element & CachedElementState.Found) == 0)
                {
                    element = await InjectionFunctions.FindElement(webView, @"#overlay", @":scope > div", @":scope > div:nth-child(3)", @"[aria-label=Pause]");
                }
                if ((element & CachedElementState.Found) != 0)
                {
                    await AutoPlay();
                    return;
                }
            }
        }

        private async Task AutoPlay()
        {
            await InjectionFunctions.ClickElementAsync(webView, @"[aria-label=play]");

        }
    }
}