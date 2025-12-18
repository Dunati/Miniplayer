(function () {
    window.miniplayer = window.miniplayer || {}
    window.miniplayer.cache = window.miniplayer.cache || {}

    function deepQuery(selector, root) {
        root = root || document
        var element = root.querySelector(selector)
        if (element) return element
        var elements = root.querySelectorAll('*')
        for (var i = 0; i < elements.length; i++) {
            if (elements[i].shadowRoot) {
                var found = deepQuery(selector, elements[i].shadowRoot)
                if (found) return found
            }
        }
        return null
    }


    function find_element(selector, cache_name) {
        var el = window.miniplayer.cache[cache_name]

        if (el) {
            if (el.isConnected) {
                return 'FoundAndCached'
            }
            return 'Cached'
        }

        el = deepQuery(selector)
        if (el) {
            window.miniplayer.cache[cache_name] = el
            return 'FoundAndCached'
        }
        return 'NotFound'
    }

    function click_element(cache_name) {
        var el = window.miniplayer.cache[cache_name]
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

    window.miniplayer.find_element = find_element
    window.miniplayer.click_element = click_element
    window.miniplayer.inject = inject
})()