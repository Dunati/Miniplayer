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
        private const string contextMenu = @"[data-testid*=""MiniPlayer_ContextMenu""]";
        private const string dislikeItem = @"[data-testid=""OverflowMenu_Option_Dislike""] [role=menuitem]";
        private const string likeButton = @"[data-testid*=""MiniPlayer_Follow""]";
        private const string playButton = @"[data-testid$=""MiniPlayer_Play""]";
        private const string pauseButton = @"[data-testid$=""MiniPlayer_Pause""]";

        public override async void Dislike()
        {
            if (!await FindElement(contextMenu))
            {
                return;
            }
            await ClickElement(contextMenu);
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(100);
                if (await FindElement(dislikeItem))
                {
                    await ClickElement(dislikeItem);
                    return;
                }
            }
        }

        public override async void Like()
        {
            await ClickElement(likeButton);
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
                if (await FindElement(pauseButton))
                {
                    return;
                }
                if (await FindElement(playButton))
                {
                    await ClickElement(playButton);
                    return;
                }
            }
        }
    }
}