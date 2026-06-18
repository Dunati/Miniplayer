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

     function inject_css(id, css) {
        const existing = document.getElementById(id)
        if (existing) {
            existing.textContent = css
            return
        }
        const target = document.head || document.documentElement
        if (target) {
            const style = document.createElement('style')
            style.id = id
            style.textContent = css
            target.appendChild(style)
        } else {
            requestAnimationFrame(function () { inject_css(id, css) })
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

    function dump_dom() {
        function serializeNode(node, indent) {
            if (node.nodeType === Node.TEXT_NODE) {
                var text = node.textContent.replace(/\s+/g, ' ').trim()
                return text ? indent + text + '\n' : ''
            }
            if (node.nodeType === Node.COMMENT_NODE) {
                return indent + '<!--' + node.textContent.trim() + '-->\n'
            }
            if (node.nodeType !== Node.ELEMENT_NODE) return ''

            var tag = node.tagName.toLowerCase()
            var attrs = ''
            for (var i = 0; i < node.attributes.length; i++) {
                var a = node.attributes[i]
                attrs += ' ' + a.name + '="' + a.value + '"'
            }

            if (tag === 'script' || tag === 'style' || tag === 'svg') {
                return indent + '<' + tag + attrs + '>...</' + tag + '>\n'
            }

            var out = indent + '<' + tag + attrs + '>\n'
            if (node.shadowRoot) {
                out += indent + '  #shadow-root\n'
                var sChildren = node.shadowRoot.childNodes
                for (var s = 0; s < sChildren.length; s++) {
                    out += serializeNode(sChildren[s], indent + '    ')
                }
            }
            for (var c = 0; c < node.childNodes.length; c++) {
                out += serializeNode(node.childNodes[c], indent + '  ')
            }
            out += indent + '</' + tag + '>\n'
            return out
        }
        return serializeNode(document.documentElement, '')
    }

    // ... existing exports ...
    window.miniplayer.set_properties = set_properties

    window.miniplayer.find_element = find_element
    window.miniplayer.click_element = click_element
    window.miniplayer.inject = inject
    window.miniplayer.inject_css = inject_css
    window.miniplayer.dump_dom = dump_dom

    window.addEventListener('wheel', function (e) {
        if (e.ctrlKey) {
            e.preventDefault();
            window.chrome.webview.postMessage(e.deltaY.toString());
        }
    }, { passive: false });
})()