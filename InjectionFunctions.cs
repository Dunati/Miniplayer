using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace MiniPlayer
{
    class InjectionFunctions
    {
        public enum Actions
        {
            Click, 
            Enable,
            Disable,
        }

        public static async Task<bool> PerformActionAsync(WebView2 webView, Actions action, params string[] selectors)
        {
            string jsonSelectors = JsonSerializer.Serialize(selectors);
            string actionString = action.ToString();

            string jsCode = $@"
        (function(selectorList, actionType) {{
            function deepQuery(selector, root) {{
                root = root || document;
                var element = root.querySelector(selector);
                if (element) return element;
                var elements = root.querySelectorAll('*');
                for (var i = 0; i < elements.length; i++) {{
                    if (elements[i].shadowRoot) {{
                        var found = deepQuery(selector, elements[i].shadowRoot);
                        if (found) return found;
                    }}
                }}
                return null;
            }}

            for (var i = 0; i < selectorList.length; i++) {{
                var el = deepQuery(selectorList[i]);
                if (el) {{
                    if (actionType === 'Click') {{
                        el.click();
                    }} 
                    else if (actionType === 'Disable') {{
                        el.disabled = true;
                        el.setAttribute('disabled', 'true');
                    }} 
                    else if (actionType === 'Enable') {{
                        el.disabled = false;
                        el.removeAttribute('disabled');
                    }}
                    return true; // Found and acted
                }}
            }}
            return false; // Not found
        }})({jsonSelectors}, '{actionString}');
    ";

            string resultJson = await webView.ExecuteScriptAsync(jsCode);

            if (resultJson == "true") return true;
            if (resultJson == "false") return false;
            return false;
        }

        public static async Task ClickElementAsync(WebView2 webView, params string[] selectors)
        {
            await PerformActionAsync(webView, Actions.Click, selectors);
        }
    }
}
