///**
// * RentalsAndProperties – Main Client JS
// * Handles: OTP flow, form validation, navbar, toasts, animations
// */

//'use strict';

//// ═══════════════════════════════════════════════════════════════════
//// Toast Notifications
//// ═══════════════════════════════════════════════════════════════════

//const Toast = (() => {
//    let container;

//    function getContainer() {
//        if (!container) {
//            container = document.createElement('div');
//            container.className = 'toast-container';
//            document.body.appendChild(container);
//        }
//        return container;
//    }

//    function show(message, type = 'info', duration = 4000) {
//        const icons = { success: '✓', error: '✕', info: 'ℹ' };
//        const toast = document.createElement('div');
//        toast.className = `toast toast-${type}`;
//        toast.innerHTML = `
//      <span class="toast-icon">${icons[type]}</span>
//      <span>${message}</span>
//    `;
//        getContainer().appendChild(toast);

//        setTimeout(() => {
//            toast.classList.add('hiding');
//            setTimeout(() => toast.remove(), 320);
//        }, duration);
//    }

//    return { show, success: m => show(m, 'success'), error: m => show(m, 'error'), info: m => show(m, 'info') };
//})();

//// ═══════════════════════════════════════════════════════════════════
//// Form Validation
//// ═══════════════════════════════════════════════════════════════════

///* ═══════════════════════════════════════════════════════════════════
//   PATCH for site.js — replace the existing Validator IIFE block
//   (the const Validator = (() => { ... })(); section)
//   with this version. Key fixes:
//     • clearError() now hides the feedback span immediately
//     • showValid() also clears the feedback message text
//     • initPasswordToggles() skips inputs whose wrapper already
//       has a .toggle-password rendered by the partial
//   ═══════════════════════════════════════════════════════════════════ */

//const Validator = (() => {

//    const rules = {
//        phoneNumber: {
//            test: v => /^\+?[1-9]\d{9,14}$/.test(v.replace(/[\s\-()]/g, '')),
//            message: 'Enter a valid phone number (e.g. +919876543210)'
//        },
//        email: {
//            test: v => v === '' || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v),
//            message: 'Enter a valid email address'
//        },
//        fullName: {
//            test: v => v.trim().length >= 2 && v.trim().length <= 100,
//            message: 'Name must be 2–100 characters'
//        },
//        password: {
//            test: v => v.length >= 6,
//            message: 'Password must be at least 6 characters'
//        },
//        confirmPassword: {
//            test: (v, form) => {
//                const pwd = form?.querySelector('[name="Password"], [id$="Password"]:not([id*="Confirm"])');
//                return v === (pwd?.value ?? v);
//            },
//            message: 'Passwords do not match'
//        }
//    };

//    function getFieldRule(input) {
//        const name = input.name || input.id || '';
//        if (/phone/i.test(name)) return rules.phoneNumber;
//        if (/email/i.test(name)) return rules.email;
//        if (/fullname|name/i.test(name) && !/last|first/i.test(name)) return rules.fullName;
//        if (/confirm.*pass|pass.*confirm/i.test(name)) return rules.confirmPassword;
//        if (/password/i.test(name)) return rules.password;
//        return null;
//    }

//    function validate(input) {
//        const rule = getFieldRule(input);
//        const value = input.value.trim();
//        const required = input.required || input.dataset.required === 'true';

//        clearError(input);   // always wipe first

//        if (required && value === '') {
//            showError(input, input.dataset.requiredMsg || 'This field is required.');
//            return false;
//        }

//        if (value !== '' && rule && !rule.test(value, input.closest('form'))) {
//            showError(input, rule.message);
//            return false;
//        }

//        if (value !== '') showValid(input);
//        return true;
//    }

//    function showError(input, message) {
//        input.classList.remove('is-valid');
//        input.classList.add('is-invalid');

//        const feedback = _getFeedback(input);
//        if (feedback) {
//            feedback.textContent = '⚠ ' + message;
//            feedback.style.display = 'flex';   // force visible
//        }
//    }

//    function showValid(input) {
//        input.classList.remove('is-invalid');
//        input.classList.add('is-valid');

//        const feedback = _getFeedback(input);
//        if (feedback) {
//            feedback.textContent = '';
//            feedback.style.display = 'none';   // hide immediately
//        }
//    }

//    function clearError(input) {
//        input.classList.remove('is-invalid', 'is-valid');

//        const feedback = _getFeedback(input);
//        if (feedback) {
//            feedback.textContent = '';
//            feedback.style.display = 'none';   // hide immediately
//        }
//    }

//    /** Walk up to find the nearest .invalid-feedback for this input */
//    function _getFeedback(input) {
//        // Direct sibling (normal inputs)
//        let fb = input.nextElementSibling;
//        if (fb && fb.classList.contains('invalid-feedback')) return fb;

//        // Sibling of wrapper (password wrapper case)
//        const wrapper = input.closest('.form-control-wrapper');
//        if (wrapper) {
//            fb = wrapper.nextElementSibling;
//            if (fb && fb.classList.contains('invalid-feedback')) return fb;
//        }

//        // Fallback: closest form-group
//        return input.closest('.form-group')?.querySelector('.invalid-feedback') ?? null;
//    }

//    function validateForm(form) {
//        const inputs = form.querySelectorAll('.form-control[required], .form-control[data-required="true"]');
//        let isValid = true;
//        inputs.forEach(input => { if (!validate(input)) isValid = false; });
//        return isValid;
//    }

//    function attachLiveValidation(container = document) {
//        container.querySelectorAll('.form-control').forEach(input => {

//            function syncHasValue() {
//                input.classList.toggle('has-value', input.value !== '' && input.value !== '0');
//            }

//            function liveRevalidate() {
//                // Only re-validate if the field has already been touched (avoids
//                // showing errors on pristine fields as the user types elsewhere)
//                if (input.classList.contains('is-invalid') || input.classList.contains('is-valid')) {
//                    validate(input);
//                }
//                syncHasValue();
//            }

//            input.addEventListener('blur', () => validate(input));
//            input.addEventListener('input', liveRevalidate);
//            input.addEventListener('change', liveRevalidate);

//            syncHasValue();
//        });
//    }

//    return { validate, validateForm, attachLiveValidation, showError, showValid, clearError };
//})();


///* ═══════════════════════════════════════════════════════════════════
//   PATCH — replace initPasswordToggles() in site.js with this version.
//   Skips inputs whose .form-control-wrapper already contains a
//   .toggle-password rendered server-side by the partial, preventing
//   a second eye icon from being injected.
//   ═══════════════════════════════════════════════════════════════════ */
//// In wwwroot/js/site.js — replace initPasswordToggles:
//function initPasswordToggles() {
//    document.querySelectorAll('.toggle-password').forEach(btn => {
//        // Prevent duplicate listeners
//        if (btn.dataset.bound === 'true') return;
//        btn.dataset.bound = 'true';

//        btn.addEventListener('click', function (e) {
//            e.preventDefault();
//            const targetId = this.dataset.target;
//            const input = document.getElementById(targetId);
//            if (!input) return;

//            const isPassword = input.type === 'password';
//            input.type = isPassword ? 'text' : 'password';

//            const eyeOpen = this.querySelector('.eye-open');
//            const eyeClosed = this.querySelector('.eye-closed');
//            if (eyeOpen) eyeOpen.style.display = isPassword ? 'block' : 'none';
//            if (eyeClosed) eyeClosed.style.display = isPassword ? 'none' : 'block';
//        });
//    });
//}

//document.addEventListener('DOMContentLoaded', initPasswordToggles);
//// ═══════════════════════════════════════════════════════════════════
//// Password Strength Meter
//// ═══════════════════════════════════════════════════════════════════

//function initPasswordStrength(inputId, barId, labelId) {
//    const input = document.getElementById(inputId);
//    const bar = document.getElementById(barId);
//    const label = document.getElementById(labelId);
//    if (!input || !bar) return;

//    function getStrength(pwd) {
//        let score = 0;
//        if (pwd.length >= 8) score++;
//        if (pwd.length >= 12) score++;
//        if (/[A-Z]/.test(pwd)) score++;
//        if (/[0-9]/.test(pwd)) score++;
//        if (/[^A-Za-z0-9]/.test(pwd)) score++;
//        return score;
//    }

//    const segments = bar.querySelectorAll('.strength-segment');
//    const levels = ['', 'weak', 'fair', 'good', 'strong', 'strong'];
//    const labels = ['', 'Weak', 'Fair', 'Good', 'Strong', 'Very Strong'];
//    const colors = ['', '#ef4444', '#f59e0b', '#3b82f6', '#43e97b', '#43e97b'];

//    input.addEventListener('input', () => {
//        const score = getStrength(input.value);
//        segments.forEach((seg, i) => {
//            seg.className = 'strength-segment';
//            if (i < score) seg.classList.add(levels[score]);
//        });
//        if (label) {
//            label.textContent = input.value ? labels[score] : '';
//            label.style.color = colors[score];
//        }
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Password Toggle (show/hide)
//// ═══════════════════════════════════════════════════════════════════

//// In wwwroot/js/site.js — replace initPasswordToggles:
//function initPasswordToggles() {
//    document.querySelectorAll('.toggle-password').forEach(btn => {
//        // Prevent duplicate listeners
//        if (btn.dataset.bound === 'true') return;
//        btn.dataset.bound = 'true';

//        btn.addEventListener('click', function (e) {
//            e.preventDefault();
//            const targetId = this.dataset.target;
//            const input = document.getElementById(targetId);
//            if (!input) return;

//            const isPassword = input.type === 'password';
//            input.type = isPassword ? 'text' : 'password';

//            const eyeOpen = this.querySelector('.eye-open');
//            const eyeClosed = this.querySelector('.eye-closed');
//            if (eyeOpen) eyeOpen.style.display = isPassword ? 'block' : 'none';
//            if (eyeClosed) eyeClosed.style.display = isPassword ? 'none' : 'block';
//        });
//    });
//}

//document.addEventListener('DOMContentLoaded', initPasswordToggles);

//// ═══════════════════════════════════════════════════════════════════
//// OTP Input – Auto-focus, backspace, paste
//// ═══════════════════════════════════════════════════════════════════

//function initOtpInputs(containerSelector, hiddenInputId) {
//    const container = document.querySelector(containerSelector);
//    const hiddenInput = document.getElementById(hiddenInputId);
//    if (!container || !hiddenInput) return;

//    const digits = Array.from(container.querySelectorAll('.otp-digit'));

//    function syncHidden() {
//        hiddenInput.value = digits.map(d => d.value).join('');
//        hiddenInput.classList.toggle('has-value', hiddenInput.value.length === digits.length);
//    }

//    digits.forEach((digit, index) => {
//        digit.addEventListener('input', e => {
//            // Only allow single digit
//            const val = e.target.value.replace(/\D/g, '').slice(-1);
//            digit.value = val;

//            if (val) {
//                digit.classList.add('filled');
//                digit.classList.remove('error');
//                if (index < digits.length - 1) digits[index + 1].focus();
//            } else {
//                digit.classList.remove('filled');
//            }
//            syncHidden();
//        });

//        digit.addEventListener('keydown', e => {
//            if (e.key === 'Backspace' && !digit.value && index > 0) {
//                digits[index - 1].value = '';
//                digits[index - 1].classList.remove('filled');
//                digits[index - 1].focus();
//                syncHidden();
//            }
//            if (e.key === 'ArrowLeft' && index > 0) digits[index - 1].focus();
//            if (e.key === 'ArrowRight' && index < digits.length - 1) digits[index + 1].focus();
//        });

//        // Prevent non-numeric
//        digit.addEventListener('keypress', e => {
//            if (!/\d/.test(e.key)) e.preventDefault();
//        });
//    });

//    // Paste handler on first digit
//    digits[0].addEventListener('paste', e => {
//        e.preventDefault();
//        const text = (e.clipboardData || window.clipboardData).getData('text').replace(/\D/g, '');
//        text.split('').slice(0, digits.length).forEach((ch, i) => {
//            digits[i].value = ch;
//            digits[i].classList.add('filled');
//        });
//        // Focus last filled or last digit
//        const lastIdx = Math.min(text.length, digits.length) - 1;
//        digits[lastIdx]?.focus();
//        syncHidden();
//    });

//    // Auto-fill from DevOTP hint on click (dev convenience)
//    document.querySelectorAll('.dev-otp-code').forEach(el => {
//        el.style.cursor = 'pointer';
//        el.title = 'Click to auto-fill';
//        el.addEventListener('click', () => {
//            const code = el.textContent.trim().replace(/\D/g, '');
//            code.split('').slice(0, digits.length).forEach((ch, i) => {
//                digits[i].value = ch;
//                digits[i].classList.add('filled');
//            });
//            digits[Math.min(code.length, digits.length) - 1]?.focus();
//            syncHidden();
//            Toast.info('OTP auto-filled! (Dev mode)');
//        });
//    });

//    return {
//        shake() {
//            digits.forEach(d => {
//                d.classList.add('error');
//                setTimeout(() => d.classList.remove('error'), 600);
//            });
//        },
//        clear() {
//            digits.forEach(d => { d.value = ''; d.classList.remove('filled', 'error'); });
//            syncHidden();
//            digits[0]?.focus();
//        }
//    };
//}

//// ═══════════════════════════════════════════════════════════════════
//// Resend OTP Countdown Timer
//// ═══════════════════════════════════════════════════════════════════

//function startResendTimer(btnSelector, countdownSelector, seconds = 60) {
//    const btn = document.querySelector(btnSelector);
//    const countdown = document.querySelector(countdownSelector);
//    if (!btn) return;

//    let remaining = seconds;

//    function tick() {
//        if (countdown) countdown.textContent = remaining;
//        btn.disabled = true;

//        if (remaining <= 0) {
//            btn.disabled = false;
//            if (countdown) countdown.closest('.resend-timer').innerHTML =
//                `<button class="resend-link" id="resendOtpBtn">Resend OTP</button>`;
//            // Re-attach click for new element
//            document.getElementById('resendOtpBtn')?.addEventListener('click', handleResend);
//            return;
//        }

//        remaining--;
//        setTimeout(tick, 1000);
//    }

//    tick();
//}

//// ═══════════════════════════════════════════════════════════════════
//// Button Loading State
//// ═══════════════════════════════════════════════════════════════════

//function setButtonLoading(btn, loading) {
//    if (!btn) return;
//    if (loading) {
//        btn.classList.add('btn-loading');
//        btn.disabled = true;
//        if (!btn.querySelector('.btn-spinner')) {
//            const s = document.createElement('span');
//            s.className = 'btn-spinner';
//            btn.appendChild(s);
//        }
//        btn.querySelector('.btn-spinner').style.display = 'block';
//    } else {
//        btn.classList.remove('btn-loading');
//        btn.disabled = false;
//        const s = btn.querySelector('.btn-spinner');
//        if (s) s.style.display = 'none';
//    }
//}

//// ═══════════════════════════════════════════════════════════════════
//// Navbar Scroll Effect + Profile Dropdown
//// ═══════════════════════════════════════════════════════════════════

//function initNavbar() {
//    const navbar = document.querySelector('.navbar');
//    if (!navbar) return;

//    // Scroll class
//    window.addEventListener('scroll', () => {
//        navbar.classList.toggle('scrolled', window.scrollY > 20);
//    }, { passive: true });

//    // Profile dropdown toggle
//    const profileDropdown = navbar.querySelector('.profile-dropdown');
//    const profileTrigger = navbar.querySelector('.profile-trigger');

//    if (profileTrigger) {
//        profileTrigger.addEventListener('click', e => {
//            e.stopPropagation();
//            profileDropdown.classList.toggle('open');
//        });

//        document.addEventListener('click', () => profileDropdown?.classList.remove('open'));
//    }
//}

//// ═══════════════════════════════════════════════════════════════════
//// Intersection Observer – fade in cards on scroll
//// ═══════════════════════════════════════════════════════════════════

//function initScrollAnimations() {
//    const obs = new IntersectionObserver(entries => {
//        entries.forEach(entry => {
//            if (entry.isIntersecting) {
//                entry.target.style.opacity = '1';
//                entry.target.style.transform = 'translateY(0)';
//            }
//        });
//    }, { threshold: 0.1 });

//    document.querySelectorAll('.property-card, .stat-item').forEach((el, i) => {
//        el.style.opacity = '0';
//        el.style.transform = 'translateY(32px)';
//        el.style.transition = `opacity .5s ${i * 0.08}s ease, transform .5s ${i * 0.08}s ease`;
//        obs.observe(el);
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Protected Link Handler – redirect guests to login
//// ═══════════════════════════════════════════════════════════════════

//function initProtectedLinks() {
//    document.querySelectorAll('[data-protected="true"]').forEach(el => {
//        el.addEventListener('click', e => {
//            const isAuth = document.body.dataset.authenticated === 'true';
//            if (!isAuth) {
//                e.preventDefault();
//                const returnUrl = el.dataset.returnUrl || el.href || window.location.href;
//                window.location.href = `/Auth/Login?returnUrl=${encodeURIComponent(returnUrl)}`;
//            }
//        });
//    });
//}

//// ═══════════════════════════════════════════════════════════════════
//// Init on DOM Ready
//// ═══════════════════════════════════════════════════════════════════

//document.addEventListener('DOMContentLoaded', () => {
//    initNavbar();
//    initScrollAnimations();
//    initProtectedLinks();
//    Validator.attachLiveValidation();
//    initPasswordToggles();

//    // Show welcome toast if present
//    const welcome = document.getElementById('welcomeMessage');
//    if (welcome) {
//        Toast.success(welcome.dataset.message || 'Welcome!');
//        welcome.remove();
//    }
//});

//// Expose globally for inline use
//window.Toast = Toast;
//window.Validator = Validator;
//window.setButtonLoading = setButtonLoading;
//window.initOtpInputs = initOtpInputs;
//window.startResendTimer = startResendTimer;
//window.initPasswordStrength = initPasswordStrength;