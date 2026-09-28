(function () {
    var vv = window.visualViewport;
    var realWidth = Object.getOwnPropertyDescriptor(Object.getPrototypeOf(vv), 'width').get;
    var minLayoutWidth = 1000;

    function zoom() { return window.__mpZoom || 1; }
    function spoofedWidth() { return Math.max(Math.round(realWidth.call(vv) / zoom()), minLayoutWidth); }

    Object.defineProperty(vv, 'width', { configurable: true, get: spoofedWidth });

    function likeButton() { return document.querySelector('[data-testid*="MiniPlayer_Follow"]'); }
    function isLiked() { var b = likeButton(); return !!b && b.getAttribute('data-testid').indexOf('Unfollow') !== -1; }

    var prev = null;
    var prevTitle = null;

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
        var button = likeButton();
        if (!button) { prev = null; return; }
        var liked = isLiked();
        var titleEl = document.querySelector('[data-testid="MiniPlayer_Title"]');
        var title = titleEl ? titleEl.getAttribute('aria-label') : null;
        if (prev !== null && prev !== liked && title === prevTitle) {
            flash(liked ? 'Added to Liked' : 'Removed');
        }
        prev = liked;
        prevTitle = title;
    }

    function player() {
        var el = document.querySelector('[data-testid*="MiniPlayer_ProgressSlider"]');
        while (el && getComputedStyle(el).position !== 'absolute') el = el.parentElement;
        return el;
    }

    var compactCss = [
        '[data-mp=player], [data-mp=player] > div, [data-mp=column] { height: auto !important; margin-top: 0 !important; gap: 0 !important; }',
        '[data-mp=player] [data-testid*=MiniPlayer_ProgressSlider] > div { padding-top: 1px !important; padding-bottom: 1px !important; }',
        '[data-mp=player] [data-testid=ProgressBar_Container] { margin-top: 0 !important; margin-bottom: 0 !important; }',
        '[data-mp=row] { gap: 1px !important; padding: 1px !important; }',
        '[data-mp=meta] [data-testid=ImageryThumbnail] { width: 44px !important; height: 44px !important; }',
        '[data-mp=text] { gap: 0 !important; }',
        '[data-mp=text] > * { margin-top: 0 !important; }',
        '[data-mp=text] span { line-height: 1.15 !important; }',
        '[data-mp=left], [data-mp=meta], [data-mp=text] { min-width: 0 !important; overflow: hidden; }',
        '[data-mp=meta] { gap: 4px !important; }',
        '[data-mp=actions], [data-mp=actions] *, [data-mp=center], [data-mp=center] * { gap: 1px !important; }',
        '[data-mp=actions], [data-mp=center] { flex: none !important; }',
        '[data-mp=right] { display: none !important; }',
        '[data-mp=row] button[data-testid^="IconButton,MiniPlayer"] > div > div { width: auto !important; height: auto !important; border-width: 0 !important; }',
        '[data-mp=row] button[data-testid^="IconButton,MiniPlayer"] [data-testid=Icon] { padding: 1px !important; }'
    ].join('\n');

    function tag(el, name) {
        if (el && el.getAttribute('data-mp') !== name) el.setAttribute('data-mp', name);
    }

    function childContaining(parent, el) {
        while (el && el.parentElement !== parent) el = el.parentElement;
        return el;
    }

    function tagLayout(p) {
        var next = p.querySelector('[data-testid*="MiniPlayer_NextButton"]');
        var title = p.querySelector('[data-testid="MiniPlayer_Title"]');
        if (!next || !title) return;
        var center = next;
        while (center.parentElement && !center.parentElement.contains(title)) center = center.parentElement;
        var row = center.parentElement;
        var left = childContaining(row, title);
        tag(p, 'player');
        tag(row.parentElement, 'column');
        tag(row, 'row');
        tag(center, 'center');
        tag(left, 'left');
        for (var i = 0; i < row.children.length; i++) {
            var c = row.children[i];
            if (c !== left && c !== center) tag(c, 'right');
        }
        var meta = childContaining(left, title);
        tag(meta, 'meta');
        for (var j = 0; j < left.children.length; j++) {
            if (left.children[j] !== meta) tag(left.children[j], 'actions');
        }
        tag(childContaining(meta, title), 'text');
    }

    function applyCrop() {
        var p = player();
        if (!p) return;
        window.miniplayer.inject_css('mp-amazon-compact', compactCss);
        tagLayout(p);
        var scale = zoom();
        p.style.transformOrigin = '0 100%';
        p.style.transform = 'scale(' + scale + ')';
        p.style.right = 'auto';
        p.style.width = (window.innerWidth / scale) + 'px';
    }

    function tick() { applyCrop(); sync(); }

    window.dispatchEvent(new Event('resize'));
    vv.dispatchEvent(new Event('resize'));
    if (window.__mpLikeTimer) clearInterval(window.__mpLikeTimer);
    tick();
    window.__mpLikeTimer = setInterval(tick, 1000);
})();
