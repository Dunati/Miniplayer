using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Windows.Forms.Design.AxImporter;

namespace MiniPlayer
{

    public class PandoraCommands : StationCommands
    {
        private const string uri = "https://www.pandora.com/";

        static void Register()
        {
            Register(uri, (WebView2 view) => new PandoraCommands(view));
        }
        public override String Uri { get; } = uri;
        public PandoraCommands(WebView2 webView) : base(webView)
        {
            Color = Color.FromArgb(34, 64, 153);
        }
        public override async void Dislike()
        {
            const string element = @"[data-qa=""thumbs_down_button""]";
            if (await FindElement(element))
            {
                await ClickElement(element);
            }
        }
        public override async void Like()
        {
            const string element = @"[data-qa=""thumbs_up_button""]";
            if(await FindElement(element))
            {
                await ClickElement(element);
            }
        }

        public override async void Next()
        {
            const string element = @"[data-qa=""skip_button""]";
            if (await FindElement(element))
            {
                await ClickElement(element);
            }
        }

        public override async void Play()
        {
            const string alt_element = @"[data-qa=""pause_button""]";
            const string     element = @"[data-qa=""play_button""]";

            var state = await InjectionFunctions.FindElement(webView, element);
            if ((state & CachedElementState.Found) != 0)
            {
                await ClickElement(element);
            }
            else if(await FindElement(alt_element))
            {
                await ClickElement(alt_element);
            }
        }

        public override async void Previous()
        {
            const string element = @"[data-qa=""replay_button""]";
            if (await FindElement(element))
            {
                await ClickElement(element);
            }
        }



        public override async Task AdjustStyle()
        {
            await base.AdjustStyle();


            for (int i = 0; i < 10; i++)
            {
                if (!await MainWindow())
                {
                    await Task.Delay(500);
                    continue;
                }
                await BottomBar();
                return;
            }


        }

        private async Task<bool> BottomBar()
        {
            string selector = @"[class=""region-bottomBar region-bottomBar--rightRail""]";
            var element = await InjectionFunctions.FindElement(webView, selector);
            if ((element & CachedElementState.Found) != 0)
            {
                float scale = Zoom;
                var properties = new
                {
                    style = new
                    {
                        height = $"{3.0}rem",
                        transform = $"scale({scale})",
                        padding = "0px",
                        transformOrigin = "0 100%",
                        width = $"{100 / scale}%"
                    },
                }
            ;
                string jsonString = JsonSerializer.Serialize(properties, options);
                await InjectionFunctions.SetProperty(webView, jsonString, selector);
                return true;
            }
            return false;
        }

        private async Task<bool> MainWindow()
        {
            string selector = @"[class=""region-main region-main--rightRail""]";
            var element = await InjectionFunctions.FindElement(webView, selector);
            if ((element & CachedElementState.Found) != 0)
            {
                var properties = new
                {
                    style = new
                    {
                        width = $"{100}%"
                    },
                }
            ;
                string jsonString = JsonSerializer.Serialize(properties, options);
                await InjectionFunctions.SetProperty(webView, jsonString, selector);
                return true;
            }
            return false;
        }
    }
}

