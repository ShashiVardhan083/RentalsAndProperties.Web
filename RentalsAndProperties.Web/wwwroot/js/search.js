/**
 * RentalsAndProperties – Search & Filter JS (Phase 4)
 * Handles: debounced city input, filter auto-apply, URL sync, infinite scroll
 */
'use strict';

(function () {

    // ── Debounce helper ───────────────────────────────────────────────────────
    function debounce(fn, ms) {
        let timer;
        return (...args) => {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), ms);
        };
    }

    // ── City input debounce (auto-submit after 600ms) ─────────────────────────
    const cityInput = document.getElementById('cityInput');
    const filterForm = document.getElementById('filterForm');

    if (cityInput && filterForm) {
        const debouncedSubmit = debounce(() => {
            if (cityInput.value.length === 0 || cityInput.value.length >= 2) {
                filterForm.submit();
            }
        }, 700);

        cityInput.addEventListener('input', debouncedSubmit);
    }

    // ── Price input debounce ──────────────────────────────────────────────────
    ['minPrice', 'maxPrice'].forEach(id => {
        const el = document.getElementById(id);
        if (!el) return;
        const submit = debounce(() => filterForm?.submit(), 900);
        el.addEventListener('input', () => {
            if (el.value === '' || parseInt(el.value) >= 0) submit();
        });
    });

    // ── Remember scroll position after filter submit ──────────────────────────
    filterForm?.addEventListener('submit', () => {
        sessionStorage.setItem('browseScrollY', window.scrollY);
    });

    // Restore scroll after load
    const savedScroll = sessionStorage.getItem('browseScrollY');
    if (savedScroll && document.getElementById('resultsPanel')) {
        // Small delay to let the DOM settle
        setTimeout(() => {
            window.scrollTo({ top: parseInt(savedScroll), behavior: 'instant' });
            sessionStorage.removeItem('browseScrollY');
        }, 100);
    }

    // ── URL-based filter badge updates ────────────────────────────────────────
    function getActiveFilterCount() {
        const params = new URLSearchParams(window.location.search);
        const ignored = new Set(['page', 'pageSize', 'sortBy']);
        let count = 0;
        for (const [key, val] of params.entries()) {
            if (!ignored.has(key) && val) count++;
        }
        return count;
    }

    const filterToggleBtn = document.getElementById('filterToggleBtn');
    if (filterToggleBtn) {
        const count = getActiveFilterCount();
        if (count > 0) {
            filterToggleBtn.textContent = `🔧 Filters (${count})`;
        }
    }

    // ── Infinite scroll (optional — enabled when no pagination visible) ────────
    function initInfiniteScroll() {
        const pagination = document.querySelector('.browse-pagination');
        if (pagination) return; // Use pagination instead when present

        const resultsGrid = document.getElementById('resultsGrid');
        if (!resultsGrid) return;

        let loading = false;
        let currentPage = 1;

        const params = new URLSearchParams(window.location.search);
        const totalPages = parseInt(params.get('totalPages') || '1');

        const sentinel = document.createElement('div');
        sentinel.id = 'scrollSentinel';
        sentinel.style.height = '20px';
        resultsGrid.after(sentinel);

        const obs = new IntersectionObserver(async (entries) => {
            if (!entries[0].isIntersecting || loading || currentPage >= totalPages) return;
            loading = true;
            currentPage++;

            const skeleton = document.getElementById('skeletonGrid');
            if (skeleton) skeleton.style.display = 'grid';

            params.set('page', currentPage);
            params.set('pageSize', '12');

            try {
                const res = await fetch(`/Property/Browse?${params.toString()}`, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                });
                // Note: Requires partial view support — see BrowsePartial.cshtml
                // For now, this enables the AJAX endpoint hook
            } catch (err) {
                console.error('Infinite scroll error:', err);
            } finally {
                loading = false;
                if (skeleton) skeleton.style.display = 'none';
            }
        }, { rootMargin: '200px' });

        obs.observe(sentinel);
    }

    // Uncomment to enable infinite scroll:
    // initInfiniteScroll();

    // ── Filter form: prevent double-submit ───────────────────────────────────
    let formSubmitted = false;
    filterForm?.addEventListener('submit', e => {
        if (formSubmitted) { e.preventDefault(); return; }
        formSubmitted = true;

        const btn = document.getElementById('applyFiltersBtn');
        if (btn) {
            btn.disabled = true;
            btn.textContent = '⏳ Searching…';
        }

        // Re-enable after 3s as fallback
        setTimeout(() => {
            formSubmitted = false;
            if (btn) {
                btn.disabled = false;
                btn.textContent = 'Apply Filters →';
            }
        }, 3000);
    });

    // ── Keyboard shortcut: "/" to focus city search ───────────────────────────
    document.addEventListener('keydown', e => {
        if (e.key === '/' && document.activeElement.tagName !== 'INPUT'
            && document.activeElement.tagName !== 'TEXTAREA') {
            e.preventDefault();
            cityInput?.focus();
        }
    });

})();