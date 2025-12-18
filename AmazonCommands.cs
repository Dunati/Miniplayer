using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

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

        JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public override async Task AdjustStyle()
        {
            await base.AdjustStyle();

            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(500);
                var element = await InjectionFunctions.FindElement(webView, "PlayerOuterDiv", @"[class=""BAibzabUKijQgULVQbqCf       ""]");
                if ((element & CachedElementState.Found) != 0)
                {
                    float scale = 0.75f;
                    var properties = new
                    {
                        style = new
                        {
                            transform = $"scale({scale})",
                            padding = "0px",
                            transformOrigin = "0 100%",
                            width = $"{100/scale}%"
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
                }
            }



        }
    }
}
/*
transform: 'scale({.5})',
                            transformOrigin: '0 0',
                            width: '{100 / .5}%',  // Optional: expands the container so contents fill the space
                            height: '{100 / .5}%' // Optional: expands the container so contents fill the space
*/