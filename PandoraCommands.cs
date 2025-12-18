using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;
using static System.Windows.Forms.Design.AxImporter;

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
            //  await InjectionFunctions.ClickElementAsync(webView, "[data-qa='play_button']", "[data-qa='pause_button']");
        }

        public override async void Previous()
        {
            await InjectionFunctions.ClickElementAsync(webView, "[data-qa='replay_button']");
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
            }


        }

        private async Task<bool> BottomBar()
        {
            var element = await InjectionFunctions.FindElement(webView, "BottomBar", @"[class=""region-bottomBar region-bottomBar--rightRail""]");
            if ((element & CachedElementState.Found) != 0)
            {
                float scale = 0.75f;
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
                await InjectionFunctions.SetProperty(webView, "BottomBar", jsonString);
                return true;
            }
            return false;
        }

        private async Task<bool> MainWindow()
        {
            var element = await InjectionFunctions.FindElement(webView, "MainWindow", @"[class=""region-main region-main--rightRail""]");
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
                await InjectionFunctions.SetProperty(webView, "MainWindow", jsonString);
                return true;
            }
            return false;
        }

            /*      float scale = 0.75f;
               var properties = new
               {
                   style = new
                   {
                       transform = $"scale({scale})",
                       padding = "0px",
                       transformOrigin = "0 100%",
                       width = $"{100 / scale}%"
                   },
               };
               string jsonString = JsonSerializer.Serialize(properties, options);

               await InjectionFunctions.SetProperty(webView, "PlayerOuterDiv", jsonString);


               var properties2 = new
               {
                   style = new
                   {
                       padding = "0px",
                   },
               };
               jsonString = JsonSerializer.Serialize(properties2, options);
               element = await InjectionFunctions.FindElement(webView, "PlayerDiv", @"[class=""_333T0bVoft6GGOqUYjsnIA  ""]");
               await InjectionFunctions.SetProperty(webView, "PlayerDiv", jsonString);

               element = await InjectionFunctions.FindElement(webView, "Play", @"[aria-label=""play""]");
               if ((element & CachedElementState.Found) != 0)
               {
                   await InjectionFunctions.ClickElementAsync(webView, "Play");
                   return;
               }

               return;
       
        }*/
    }
}

