using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace MiniPlayer
{
    public abstract class StationCommands
    {
        protected WebView2 webView;
        protected StationCommands(WebView2 webView)
        {
            this.webView = webView;
        }
        public abstract void Play();
        public abstract void Next();
        public abstract void Previous();
        public abstract void Like();
        public abstract void Dislike();

        public  virtual async Task AdjustStyle()
        {

        }
        public string BrowserArguments { get; protected set; } = "--autoplay-policy=no-user-gesture-required";


        protected JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Color Color { get; set; }

    }



}
