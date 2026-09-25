/* UniSkill Hub - small site-wide behaviours (plain JavaScript, no libraries) */
(function () {
    'use strict';

    // 1. Highlight the navigation link that matches the current page
    function markActiveNavLink() {
        var here = window.location.pathname.replace(/\/+$/, '').toLowerCase() || '/';
        var links = document.querySelectorAll('.site-nav .nav-link');

        for (var i = 0; i < links.length; i++) {
            var link = links[i];
            // A link can name the whole section it belongs to (data-match="/resources"),
            // so the list page AND its details page both highlight it.
            var path = (link.getAttribute('data-match') || link.pathname).replace(/\/+$/, '').toLowerCase() || '/';
            var isHome = (path === '/');

            // Home only matches "/" exactly; other links match their own folder/page.
            var active = isHome ? (here === '/' || here === '/default')
                                : (here === path || here.indexOf(path + '/') === 0);
            if (active) {
                link.classList.add('is-active');
                link.setAttribute('aria-current', 'page');
            }
        }
    }

    // 2. Add a shadow under the header once the page has been scrolled
    function initHeaderShadow() {
        var header = document.querySelector('.site-header');
        if (!header) { return; }
        function update() { header.classList.toggle('is-scrolled', window.scrollY > 8); }
        window.addEventListener('scroll', update, { passive: true });
        update();
    }

    // 3. Fade elements in as they scroll into view
    function initReveal() {
        var items = document.querySelectorAll('.reveal');
        if (!items.length) { return; }

        if (!('IntersectionObserver' in window)) {
            for (var i = 0; i < items.length; i++) { items[i].classList.add('is-visible'); }
            return;
        }

        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });

        items.forEach(function (el) { observer.observe(el); });
    }

    // 4. Soft spotlight that follows the pointer on feature cards
    function initCardSpotlight() {
        var cards = document.querySelectorAll('.feature-card');
        cards.forEach(function (card) {
            card.addEventListener('pointermove', function (e) {
                var rect = card.getBoundingClientRect();
                card.style.setProperty('--mx', (e.clientX - rect.left) + 'px');
                card.style.setProperty('--my', (e.clientY - rect.top) + 'px');
            });
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        markActiveNavLink();
        initHeaderShadow();
        initReveal();
        initCardSpotlight();
    });
})();
