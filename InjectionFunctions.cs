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
        case "FoundAndCached":
            return CachedElementState.Cached | CachedElementState.Found;
        case "Cached":
            return CachedElementState.Cached;
        }
        return CachedElementState.None;
    }
    public static async Task<CachedElementState> FindElement(WebView2 webView, string cacheName, string selector)
    {
        string result = await webView.ExecuteScriptAsync($"window.miniplayer.find_element('{selector}', '{cacheName}')");
        return ParseElementState(result[1..^1]);
    }

    public static async Task SetProperty(WebView2 webView, string cacheName, string props)
    {
        await webView.ExecuteScriptAsync($"window.miniplayer.set_properties('{cacheName}', {props})");
    }

    public static async Task ClickElementAsync(WebView2 webView, string cacheName)
    {
        await webView.ExecuteScriptAsync($"window.miniplayer.click_element('{cacheName}')");
    }
}

[Flags]
enum CachedElementState
{
    None,
    Found = (1 << 0),
    Cached = (1 << 1),
}