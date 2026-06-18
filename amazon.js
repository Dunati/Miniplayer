(function () {
    function deepQuery(selectors, root) {
        root = root || document;
        selectors = Array.isArray(selectors) ? selectors : [selectors];
        var cur = root;
        for (var i = 0; i < selectors.length; i++) {
            cur = findDeep(selectors[i], cur);
            if (!cur) return null;
        }
        return cur;
        function findDeep(sel, node) {
            var el = node.querySelector(sel);
            if (el) return el;
            var all = node.querySelectorAll('*');
            for (var j = 0; j < all.length; j++) {
                if (all[j].shadowRoot) {
                    var f = findDeep(sel, all[j].shadowRoot);
                    if (f) return f;
                }
            }
            return null;
        }
    }

    function host() { return document.querySelector('#transport music-button[icon-name^=favorite]'); }
    function isLiked() { var h = host(); return !!h && h.getAttribute('icon-name') === 'favoriteon'; }
    function realButton() {
        return deepQuery(['#transport', '[aria-label=Unlike]']) || deepQuery(['#transport', '[aria-label=Like]']);
    }

    var prev = null;

    function getHeart() {
        var h = document.getElementById('mp-like-btn');
        if (h) return h;
        h = document.createElement('div');
        h.id = 'mp-like-btn';
        h.style.cssText = 'position:fixed;display:none;align-items:center;justify-content:center;width:18px;height:18px;cursor:pointer;z-index:2147483647;transition:transform .15s ease;';
        h.innerHTML = `<svg id='mp-like-svg' width='18' height='18' viewBox='0 0 24 24'><path d='M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z'/></svg>`;
        h.addEventListener('click', function () {
            var b = realButton();
            if (b) { b.click(); setTimeout(sync, 150); }
        });
        document.body.appendChild(h);
        return h;
    }

    function reposition() {
        var heart = getHeart();
        var nextEl = document.querySelector('#nextButton');
        var playEl = document.querySelector('#transport music-button[icon-name=pause], #transport music-button[icon-name=play]');
        if (!nextEl || !playEl) { heart.style.display = 'none'; return; }
        var n = nextEl.getBoundingClientRect();
        var p = playEl.getBoundingClientRect();
        if (!n.width || !p.width) { heart.style.display = 'none'; return; }
        var icon = nextEl.shadowRoot ? nextEl.shadowRoot.querySelector('svg') : null;
        var ir = icon ? icon.getBoundingClientRect() : n;
        var s = Math.round(ir.height * 0.8);
        if (s < 6) s = 16;
        var svg = document.getElementById('mp-like-svg');
        if (svg) { svg.setAttribute('width', s); svg.setAttribute('height', s); }
        heart.style.width = s + 'px';
        heart.style.height = s + 'px';
        heart.style.left = ((p.right + n.left) / 2 - s / 2) + 'px';
        heart.style.top = ((ir.top + ir.bottom) / 2 - s / 2) + 'px';
        heart.style.display = 'flex';
    }

    function flash(text) {
        var label = document.getElementById('mp-like-label');
        if (!label) {
            label = document.createElement('div');
            label.id = 'mp-like-label';
            label.style.cssText = 'position:fixed;left:6px;bottom:6px;z-index:2147483647;background:rgba(0,0,0,0.7);color:#fff;font-family:sans-serif;font-size:12px;line-height:1;padding:4px 8px;border-radius:10px;opacity:0;transition:opacity .2s ease;pointer-events:none;white-space:nowrap;';
            document.body.appendChild(label);
        }
        label.textContent = text;
        label.style.opacity = '1';
        clearTimeout(label.__t);
        label.__t = setTimeout(function () { label.style.opacity = '0'; }, 1400);
    }

    function sync() {
        var liked = isLiked();
        var path = document.querySelector('#mp-like-svg path');
        var heart = document.getElementById('mp-like-btn');
        if (path) path.setAttribute('fill', liked ? '#41c5f4' : 'rgba(255,255,255,0.45)');
        if (prev !== null && prev !== liked && heart) {
            heart.style.transform = 'scale(1.35)';
            setTimeout(function () { heart.style.transform = 'scale(1)'; }, 160);
            flash(liked ? 'Added to Liked' : 'Removed');
        }
        prev = liked;
    }

    function applyCrop() {
        var transport = document.querySelector('#transport');
        if (!transport || !transport.parentElement || !transport.parentElement.parentElement) return;
        var player = transport.parentElement.parentElement;
        var scale = window.__mpZoom || 1;
        player.style.transform = 'scale(' + scale + ')';
        player.style.width = (100 / scale) + '%';
        player.style.transformOrigin = '0 100%';
        player.style.padding = '0px';
        transport.style.padding = '0px';
    }

    function tick() { applyCrop(); reposition(); sync(); }

    if (window.__mpLikeTimer) clearInterval(window.__mpLikeTimer);
    if (window.__mpResize) window.removeEventListener('resize', window.__mpResize);
    window.__mpResize = reposition;
    window.addEventListener('resize', window.__mpResize);
    tick();
    window.__mpLikeTimer = setInterval(tick, 1000);
})();
