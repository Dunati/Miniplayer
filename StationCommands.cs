using Microsoft.Web.WebView2.WinForms;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;

namespace MiniPlayer
{
    public abstract class StationCommands
    {
        const float ZoomStep = 0.05f;
        const float MinZoom = ZoomStep;
        const float MaxZoom = 2;
        private float _zoom = 1.0f;
        private static Dictionary<string, Func<WebView2, StationCommands>> commands = new();
        protected static void Register(string uri, Func<WebView2, StationCommands> create)
        {
            commands[uri] = create;
        }

        public static StationCommands? Get(string uri, WebView2 webView)
        {
            if (commands.TryGetValue(uri, out var val))
            {
                return val(webView);
            }
            return null;
        }

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

        public virtual async Task AdjustStyle()
        {

        }
        public string BrowserArguments { get; protected set; } = "--autoplay-policy=no-user-gesture-required";


        protected JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Color Color { get; set; }
        public float Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = Math.Clamp(MinZoom, value, MaxZoom);
            }
        }

        public abstract String Uri { get; }

        public virtual async Task ZoomIn()
        {
            if (_zoom < 2.0f)
            {
                _zoom += 0.05f;
                await AdjustStyle();
            }
        }
        public virtual async Task ZoomOut()
        {
            if (_zoom > 0.05f)
            {
                _zoom -= 0.05f;
                await AdjustStyle();
            }
        }

        protected async Task<bool> FindElement(string selector)
        {
            var state = await InjectionFunctions.FindElement(webView, selector);
            return (state & CachedElementState.Found) != 0;
        }
        protected async Task ClickElement(string selector)
        {
            await InjectionFunctions.ClickElementAsync(webView, selector);
        }

        public static void RegisterCommands()
        {
            var commandType = typeof(StationCommands);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(commandType) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in types)
                {
                    var method = type.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static);
                    method?.Invoke(null, null);
                }
            }
        }
    }
}
