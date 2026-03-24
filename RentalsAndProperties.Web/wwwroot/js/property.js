///**
// * RentalsAndProperties – Phase 2 Property JS
// * Handles: image gallery, lazy loading, delete confirmations
// */

//'use strict';

//// ═══════════════════════════════════════════════════════════════════
//// Lazy Image Loading with IntersectionObserver
//// ═══════════════════════════════════════════════════════════════════

//function initLazyImages() {
//    const imgs = document.querySelectorAll('img[loading="lazy"]');
//    if ('IntersectionObserver' in window) {
//        const obs = new IntersectionObserver((entries) => {
//            entries.forEach(e => {
//                if (e.isIntersecting) {
//                    const img = e.target;
//                    if (img.dataset.src) {
//                        img.src = img.dataset.src;
//                        img.removeAttribute('data-src');
//                    }
//                    obs.unobserve(img);
//                }
//            });
//        }, { rootMargin: '200px' });
//        imgs.forEach(img => obs.observe(img));
//    }
//}

//// ═══════════════════════════════════════════════════════════════════
//// Property Image Gallery (Details page)
//// ═══════════════════════════════════════════════════════════════════

//function initGallery() {
//    const mainImg = document.getElementById('mainImg');
//    if (!mainImg) return;

//    document.querySelectorAll('.thumb').forEach(thumb => {
//        thumb.addEventListener('click', () => {
//            mainImg.style.opacity = '0';
//            setTimeout(() => {
//                mainImg.src = thumb.src;
//                mainImg.style.opacity = '1';
//            }, 150);
//            document.querySelectorAll('.thumb').forEach(t => t.classList.remove('active'));
//            thumb.classList.add('active');
//        });
//    });

//    // Keyboard navigation for gallery thumbs
//    document.querySelectorAll('.thumb').forEach((thumb, i, all) => {
//        thumb.setAttribute('tabindex', '0');
//        thumb.addEventListener('keydown', e => {
//            if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); thumb.click(); }
//            if (e.key === 'ArrowRight' && all[i + 1]) all[i + 1].focus();
//            if (e.key === 'ArrowLeft' && all[i - 1]) all[i - 1].focus();
//        });
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Confirm Delete with Property Title
//// ═══════════════════════════════════════════════════════════════════

//function initDeleteConfirm() {
//    document.querySelectorAll('[data-confirm]').forEach(el => {
//        el.addEventListener('click', e => {
//            const msg = el.dataset.confirm || 'Are you sure?';
//            if (!confirm(msg)) e.preventDefault();
//        });
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Select Floating Label Fix (for property form dropdowns)
//// ═══════════════════════════════════════════════════════════════════

//function initSelectLabels() {
//    document.querySelectorAll('.form-select').forEach(sel => {
//        function update() {
//            sel.classList.toggle('has-value', sel.value !== '' && sel.value !== '0');
//        }
//        sel.addEventListener('change', update);
//        update();
//    });

//    // Date/number inputs
//    document.querySelectorAll('input[type="date"], input[type="number"]').forEach(inp => {
//        function update() { inp.classList.toggle('has-value', inp.value !== ''); }
//        inp.addEventListener('input', update);
//        inp.addEventListener('change', update);
//        update();
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Admin: Inline Reject Modal (optional enhancement)
//// ═══════════════════════════════════════════════════════════════════

//function initAdminActions() {
//    // Approve button ripple
//    document.querySelectorAll('.btn-approve, .btn-reject').forEach(btn => {
//        btn.addEventListener('mousedown', function (e) {
//            const ripple = document.createElement('span');
//            ripple.style.cssText = `
//                position:absolute;border-radius:50%;
//                background:rgba(255,255,255,.3);
//                width:0;height:0;
//                left:${e.offsetX}px;top:${e.offsetY}px;
//                transform:translate(-50%,-50%);
//                animation:ripple .4s ease-out;
//                pointer-events:none;
//            `;
//            this.style.position = 'relative';
//            this.style.overflow = 'hidden';
//            this.appendChild(ripple);
//            setTimeout(() => ripple.remove(), 400);
//        });
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Property Card Hover 3D tilt (subtle)
//// ═══════════════════════════════════════════════════════════════════

//function initCardTilt() {
//    document.querySelectorAll('.property-card, .admin-card').forEach(card => {
//        card.addEventListener('mousemove', e => {
//            const rect = card.getBoundingClientRect();
//            const x = (e.clientX - rect.left) / rect.width - 0.5;
//            const y = (e.clientY - rect.top) / rect.height - 0.5;
//            card.style.transform = `translateY(-6px) rotateY(${x * 4}deg) rotateX(${-y * 4}deg)`;
//        });
//        card.addEventListener('mouseleave', () => {
//            card.style.transform = '';
//        });
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Ripple keyframe injection
//// ═══════════════════════════════════════════════════════════════════

//(function injectRippleKeyframe() {
//    const style = document.createElement('style');
//    style.textContent = `
//        @keyframes ripple {
//            to { width:120px; height:120px; opacity:0; }
//        }
//    `;
//    document.head.appendChild(style);
//})();

//// ═══════════════════════════════════════════════════════════════════
//// Init
//// ═══════════════════════════════════════════════════════════════════

//document.addEventListener('DOMContentLoaded', () => {
//    initLazyImages();
//    initGallery();
//    initDeleteConfirm();
//    initSelectLabels();
//    initAdminActions();
//    initCardTilt();
//});

//        // ═══════════════════════════════════════════════════════════════════
//// ADDITIONS to property.js — append to the bottom of the existing file
//// These additions support Phase 3 (lazy load) and Phase 4 (search UX)
//// ═══════════════════════════════════════════════════════════════════

//// ── Enhanced Lazy Image Observer ────────────────────────────────────────────
//(function initLazyImageFadeIn() {
//    if (!('IntersectionObserver' in window)) return;

//    const obs = new IntersectionObserver((entries) => {
//        entries.forEach(entry => {
//            if (entry.isIntersecting) {
//                const img = entry.target;
//                // If it has a data-src, set src from it
//                if (img.dataset.src) {
//                    img.src = img.dataset.src;
//                    delete img.dataset.src;
//                }
//                // Trigger fade-in once loaded
//                if (img.complete) {
//                    img.classList.add('lazy-loaded');
//                } else {
//                    img.addEventListener('load', () => img.classList.add('lazy-loaded'), { once: true });
//                    img.addEventListener('error', () => img.classList.add('lazy-loaded'), { once: true });
//                }
//                obs.unobserve(img);
//            }
//        });
//    }, { rootMargin: '300px' });

//    document.querySelectorAll('img[loading="lazy"]').forEach(img => obs.observe(img));
//})();

//// ── Upload page: drag zone accessible via keyboard ───────────────────────────
//(function initDropZoneA11y() {
//    document.querySelectorAll('.drop-zone').forEach(zone => {
//        zone.addEventListener('keydown', e => {
//            if (e.key === 'Enter' || e.key === ' ') {
//                e.preventDefault();
//                zone.querySelector('input[type="file"]')?.click();
//            }
//        });
//    });
//})();

//// ── Search: highlight filter chips that match URL params ────────────────────
//(function syncChipsFromUrl() {
//    const params = new URLSearchParams(window.location.search);

//    document.querySelectorAll('.filter-chip-group').forEach(group => {
//        const inputs = group.querySelectorAll('input[type="radio"]');
//        inputs.forEach(input => {
//            const paramVal = params.get(input.name) ?? '';
//            if (input.value === paramVal) {
//                input.checked = true;
//                input.closest('.filter-chip')?.classList.add('active');
//            }
//        });
//    });
//})();