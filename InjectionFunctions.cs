using Microsoft.Web.WebView2.WinForms;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.ComponentModel.Design.ObjectSelectorEditor;

class InjectionFunctions
{
    static CachedElementState ParseElementState(string state)
    {
        switch (state)
        {
        case "Found":
            return CachedElementState.Found;
        }
        return CachedElementState.None;
    }
    public static async Task<CachedElementState> FindElement(WebView2 webView, params string[] selector)
    {
        string jsonSelector = JsonSerializer.Serialize(selector);
        string result = await webView.ExecuteScriptAsync($"window.miniplayer.find_element({jsonSelector})");
        return ParseElementState(result[1..^1]);
    }

    public static async Task SetProperty(WebView2 webView, string props, params string[] selector)
    {
        string jsonSelector = JsonSerializer.Serialize(selector);
        await webView.ExecuteScriptAsync($"window.miniplayer.set_properties({jsonSelector}, {props})");
    }

    public static async Task ClickElementAsync(WebView2 webView, params string[] selector)
    {
        string jsonSelector = JsonSerializer.Serialize(selector);
        await webView.ExecuteScriptAsync($"window.miniplayer.click_element({jsonSelector})");
    }

    public static async Task InjectCss(WebView2 webView, string id, string css)
    {
        string jsId = JsonSerializer.Serialize(id);
        string jsCss = JsonSerializer.Serialize(css);
        await webView.ExecuteScriptAsync($"window.miniplayer.inject_css({jsId}, {jsCss})");
    }

    public static async Task<string> DumpDom(WebView2 webView)
    {
        string result = await webView.ExecuteScriptAsync("window.miniplayer.dump_dom()");
        return JsonSerializer.Deserialize<string>(result) ?? "";
    }
}

[Flags]
enum CachedElementState
{
    None,
    Found = (1 << 0)
}