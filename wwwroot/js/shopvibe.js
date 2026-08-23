(function (window) {
    var Brand = 'ShopVibe';

    function hasSwal() {
        return typeof Swal !== 'undefined';
    }

    function toast(message, type) {
        type = type || 'success';
        if (hasSwal()) {
            var icon = type === 'danger' || type === 'error' ? 'error' : (type === 'warning' ? 'warning' : (type === 'info' ? 'info' : 'success'));
            Swal.fire({
                toast: true,
                position: 'top-end',
                icon: icon,
                title: message,
                showConfirmButton: false,
                timer: 2800,
                timerProgressBar: true
            });
            return;
        }
        var container = document.getElementById('toastContainer');
        if (!container || typeof bootstrap === 'undefined') return;
        var toastEl = document.createElement('div');
        toastEl.className = 'toast align-items-center text-bg-' + (type === 'error' ? 'danger' : type) + ' border-0';
        toastEl.setAttribute('role', 'alert');
        toastEl.innerHTML = '<div class="d-flex"><div class="toast-body">' + (message || '').replace(/</g, '&lt;') + '</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
        container.appendChild(toastEl);
        var t = new bootstrap.Toast(toastEl, { autohide: true, delay: 2500 });
        t.show();
        toastEl.addEventListener('hidden.bs.toast', function () { toastEl.remove(); });
    }

    function success(message) {
        if (hasSwal()) {
            return Swal.fire({ icon: 'success', title: message || 'Saved successfully', confirmButtonText: 'OK' });
        }
        toast(message || 'Saved successfully', 'success');
    }

    function error(message) {
        if (hasSwal()) {
            return Swal.fire({ icon: 'error', title: 'Something went wrong', text: message || 'Please try again.', confirmButtonText: 'OK' });
        }
        toast(message || 'Something went wrong', 'error');
    }

    function confirm(options) {
        options = options || {};
        var title = options.title || 'Are you sure?';
        var text = options.text || '';
        var confirmText = options.confirmText || 'Confirm';
        var danger = options.danger !== false && (options.danger || /delete|logout|cancel|remove/i.test(confirmText + title));
        if (hasSwal()) {
            return Swal.fire({
                title: title,
                text: text,
                icon: options.icon || (danger ? 'warning' : 'question'),
                showCancelButton: true,
                confirmButtonText: confirmText,
                cancelButtonText: options.cancelText || 'Cancel',
                confirmButtonColor: danger ? '#DC2626' : '#05192D',
                reverseButtons: true
            }).then(function (r) { return !!r.isConfirmed; });
        }
        return Promise.resolve(window.confirm(title + (text ? '\n' + text : '')));
    }

    function setLoading(btn, loading, loadingText) {
        if (!btn) return;
        if (loading) {
            btn.dataset.svOrig = btn.innerHTML;
            btn.disabled = true;
            btn.setAttribute('aria-busy', 'true');
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>' + (loadingText || 'Please wait...');
        } else {
            btn.disabled = false;
            btn.removeAttribute('aria-busy');
            if (btn.dataset.svOrig) btn.innerHTML = btn.dataset.svOrig;
        }
    }

    function bindConfirmForms() {
        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!(form instanceof HTMLFormElement)) return;
            if (form.dataset.svConfirmed === '1') return;
            var btn = form.querySelector('[data-sv-confirm], .js-sv-confirm');
            var attr = form.getAttribute('data-sv-confirm');
            if (!btn && !attr) return;
            e.preventDefault();
            var title = (btn && btn.getAttribute('data-sv-title')) || form.getAttribute('data-sv-title') || 'Are you sure?';
            var text = (btn && btn.getAttribute('data-sv-text')) || form.getAttribute('data-sv-text') || '';
            var confirmText = (btn && btn.getAttribute('data-sv-ok')) || form.getAttribute('data-sv-ok') || 'Confirm';
            confirm({ title: title, text: text, confirmText: confirmText, danger: true }).then(function (ok) {
                if (!ok) return;
                form.dataset.svConfirmed = '1';
                form.submit();
            });
        });
    }

    function bindSubmitLoading() {
        document.addEventListener('submit', function (e) {
            var form = e.target;
            if (!(form instanceof HTMLFormElement)) return;
            if (form.dataset.svNoLoading === '1') return;
            if (form.getAttribute('data-sv-confirm') || form.querySelector('[data-sv-confirm], .js-sv-confirm, [data-sv-logout]')) return;
            var btn = form.querySelector('button[type="submit"], input[type="submit"]');
            if (!btn || btn.disabled) return;
            setLoading(btn, true, btn.getAttribute('data-loading-text') || 'Saving...');
        });
    }

    function bindPasswordToggles() {
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.sv-toggle-password');
            if (!btn) return;
            var wrap = btn.closest('.input-icon-wrap, .sv-password-wrap');
            var input = wrap ? wrap.querySelector('input') : document.getElementById(btn.getAttribute('data-target') || '');
            if (!input) return;
            var show = input.type === 'password';
            input.type = show ? 'text' : 'password';
            var icon = btn.querySelector('i');
            if (icon) {
                icon.classList.toggle('bi-eye', !show);
                icon.classList.toggle('bi-eye-slash', show);
            }
        });
    }

    function bindLogout() {
        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-sv-logout]');
            if (!trigger) return;
            e.preventDefault();
            var form = trigger.closest('form') || document.getElementById(trigger.getAttribute('data-sv-logout'));
            confirm({
                title: 'Log out of ShopVibe?',
                text: 'You will need to sign in again to access your account.',
                confirmText: 'Logout',
                danger: true,
                icon: 'question'
            }).then(function (ok) {
                if (ok && form) form.submit();
            });
        });
    }

    function unlockUi() {
        if (document.querySelector('.modal.show') || document.querySelector('.swal2-container')) return;
        document.querySelectorAll('.modal-backdrop').forEach(function (el) { el.remove(); });
        document.body.classList.remove('modal-open');
        document.body.style.removeProperty('overflow');
        document.body.style.removeProperty('padding-right');
    }

    function bindTempData() {
        var successMsg = document.body.getAttribute('data-sv-success');
        var errorMsg = document.body.getAttribute('data-sv-error');
        if (successMsg) toast(successMsg, 'success');
        if (errorMsg) toast(errorMsg, 'error');
    }

    function bindAdminSidebar() {
        var toggle = document.querySelector('[data-sv-sidebar-toggle]');
        var sidebar = document.querySelector('.sv-sidebar');
        var backdrop = document.querySelector('.sv-sidebar-backdrop');
        function close() {
            if (sidebar) sidebar.classList.remove('is-open');
            if (backdrop) backdrop.classList.remove('is-open');
        }
        if (toggle && sidebar) {
            toggle.addEventListener('click', function () {
                sidebar.classList.toggle('is-open');
                if (backdrop) backdrop.classList.toggle('is-open');
            });
        }
        if (backdrop) backdrop.addEventListener('click', close);
    }

    function previewImages(input, targetId) {
        var target = document.getElementById(targetId);
        if (!input || !target) return;
        target.innerHTML = '';
        Array.prototype.forEach.call(input.files || [], function (file, i) {
            if (!file.type || file.type.indexOf('image/') !== 0) return;
            var url = URL.createObjectURL(file);
            var wrap = document.createElement('div');
            wrap.className = 'sv-upload-thumb';
            wrap.innerHTML = '<img src="' + url + '" alt="Preview ' + (i + 1) + '" /><button type="button" aria-label="Remove">&times;</button>';
            wrap.querySelector('button').addEventListener('click', function () {
                wrap.remove();
            });
            target.appendChild(wrap);
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        bindConfirmForms();
        bindSubmitLoading();
        bindPasswordToggles();
        bindLogout();
        bindTempData();
        bindAdminSidebar();
        document.addEventListener('hidden.bs.modal', function () { setTimeout(unlockUi, 80); });
        document.addEventListener('click', function () {
            if (!document.querySelector('.modal.show') && document.querySelector('.modal-backdrop')) unlockUi();
        }, true);
    });

    window.ShopVibe = {
        brand: Brand,
        toast: toast,
        success: success,
        error: error,
        confirm: confirm,
        setLoading: setLoading,
        previewImages: previewImages,
        unlockUi: unlockUi
    };
    window.showToast = function (message, type) { toast(message, type); };
})(window);
