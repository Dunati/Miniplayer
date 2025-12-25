using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.Design.AxImporter;

namespace MiniPlayer
{

    public class SpotifyCommands : StationCommands
    {
        private const string uri = "https://www.spotify.com/";

        static void Register()
        {
            Register(uri, (WebView2 view) => new SpotifyCommands(view));
        }
        public override String Uri { get; } = uri;
        public SpotifyCommands(WebView2 webView) : base(webView)
        {
            Color = Color.FromArgb(12, 76, 34);
        }
        public override async void Dislike()
        {
            const string element = "div:has(> div > a[draggable=\"true\"])   > span > div > button:nth-child(1)";
            if (await FindElement(element))
            {
                await ClickElement(element);
                await Task.Delay(100);

                await ClickElement("#context-menu > ul > li:nth-child(4) > button");
            }
        }
        public override async void Like()
        {
            const string element = "div:has(> div > a[draggable=\"true\"])   > span > div > button:nth-child(1)";
            if (await FindElement(element))
            {
                await ClickElement(element);
                await Task.Delay(100);

                await ClickElement("#context-menu > ul > li:nth-child(2) > button");                
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



        public override async Task AdjustStyle()
        {
            await base.AdjustStyle();


            for (int i = 0; i < 10; i++)
            {
                if (!await BottomBar())
                {
                    await Task.Delay(500);
                    continue;
                }

                await InjectionFunctions.ClickElementAsync(webView, @"[aria-label=Play]");
                return;
            }


        }

        private async Task<bool> BottomBar()
        {
            string selector = @"[aria-label=""Now playing bar""]";
            var element = await InjectionFunctions.FindElement(webView, selector);
            if ((element & CachedElementState.Found) != 0)
            {
                float scale = Zoom;
                var properties = new
                {
                    style = new
                    {
                        position = "fixed",
                        bottom = "0",
                        left = "0",
                        backgroundColor = $"rgb({Color.R}, {Color.G}, {Color.B})",
                        zIndex = 9999,
                        transform = $"scale({scale})",
                        width = $"{100 / scale}%",
                        transformOrigin = "0 100%",
                    }
                };
                string jsonString = JsonSerializer.Serialize(properties, options);
                await InjectionFunctions.SetProperty(webView, jsonString, selector);
                return true;
            }
            return false;
        }

    }
}

