(function () {
    window.miniplayer = window.miniplayer || {}

    function deepQuery(selector, root) {
        root = root || document;
        const selectors = Array.isArray(selector) ? selector : [selector];
        let currentRoot = root;

        for (let i = 0; i < selectors.length; i++) {
            const selector = selectors[i];

            function findDeep(sel, startNode) {
                var element = startNode.querySelector(sel);
                if (element) {
                    return element;
                }

                var elements = startNode.querySelectorAll('*');
                for (var j = 0; j < elements.length; j++) {
                    if (elements[j].shadowRoot) {
                        var found = findDeep(sel, elements[j].shadowRoot);
                        if (found) {
                            return found;
                        }
                    }
                }
                return null;
            }

            currentRoot = findDeep(selector, currentRoot);

            if (!currentRoot) return null;
        }

        return currentRoot;
    }
    function find_element(selector) {
        var el = deepQuery(selector)
        if (el) {
            return 'Found'
        }
        return 'NotFound'
    }

    function click_element(selector) {
        var el = deepQuery(selector)
        if (el) {
            el.click()
            return true
        }
        return false
    }

    function inject(css) {
        const target = document.head || document.documentElement
        if (target) {
            const style = document.createElement('style')
            style.innerHTML = css
            target.appendChild(style)
        } else {
            requestAnimationFrame(function () { inject(css) })
        }
    }

    const hide_scrollbars = `
        ::-webkit-scrollbar { 
            display: none !important; 
            width: 0px !important; 
            height: 0px !important; 
        }
            
        html, body { 
            scrollbar-width: none !important; 
            -ms-overflow-style: none !important; 
            overflow: auto !important; 
        }
    `
    inject(hide_scrollbars)

    function set_properties(selector, props) {
        var el = deepQuery(selector)
        if (!el) return false
        for (var key in props) {
            if (key === 'style' && typeof props[key] === 'object') {
                Object.assign(el.style, props[key])
            } else {
                el[key] = props[key]
            }
        }
        return true
    }

    // ... existing exports ...
    window.miniplayer.set_properties = set_properties

    window.miniplayer.find_element = find_element
    window.miniplayer.click_element = click_element
    window.miniplayer.inject = inject

    window.addEventListener('wheel', function (e) {
        if (e.ctrlKey) {
            e.preventDefault();
            window.chrome.webview.postMessage(e.deltaY.toString());
        }
    }, { passive: false });
})()