// Client-side deterrent only: determined users can still open DevTools or inspect downloaded assets.
(function () {
    'use strict';

    let lastNoticeAt = 0;

    function showNotice() {
        const now = Date.now();
        if (now - lastNoticeAt < 2500) return;
        lastNoticeAt = now;

        const existing = document.getElementById('security-deterrent-notice');
        if (existing) existing.remove();

        const notice = document.createElement('div');
        notice.id = 'security-deterrent-notice';
        notice.setAttribute('role', 'status');
        notice.textContent = 'Thao tác này đã bị hạn chế trên website.';
        notice.style.cssText = [
            'position:fixed', 'z-index:2147483647', 'top:16px', 'left:50%',
            'transform:translateX(-50%)', 'padding:11px 18px', 'border-radius:8px',
            'background:#242424', 'color:#fff', 'font:14px/1.4 sans-serif',
            'box-shadow:0 4px 18px rgba(0,0,0,.25)'
        ].join(';');

        document.body.appendChild(notice);
        window.setTimeout(function () { notice.remove(); }, 2200);
    }

    document.addEventListener('keydown', function (event) {
        const key = (event.key || '').toLowerCase();
        const isDevToolsShortcut = key === 'f12' ||
            ((event.ctrlKey || event.metaKey) && event.shiftKey && ['i', 'j', 'c', 'm'].includes(key)) ||
            ((event.ctrlKey || event.metaKey) && key === 'u');

        if (isDevToolsShortcut) {
            event.preventDefault();
            event.stopImmediatePropagation();
            showNotice();
        }
    }, true);

    document.addEventListener('contextmenu', function (event) {
        event.preventDefault();
        showNotice();
    }, true);
})();
