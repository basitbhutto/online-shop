// Shopwala - Wishlist (guest + logged-in), Cart, Chat
var GUEST_WISHLIST_KEY = 'shopwala_guest_wishlist';

window.getGuestWishlist = function () {
    try {
        var j = localStorage.getItem(GUEST_WISHLIST_KEY);
        return j ? JSON.parse(j) : [];
    } catch (e) { return []; }
};

window.setGuestWishlist = function (arr) {
    try {
        localStorage.setItem(GUEST_WISHLIST_KEY, JSON.stringify(arr));
    } catch (e) {}
};

window.updateWishlistCount = function () {
    var badge = document.getElementById('wishlistCountBadge');
    if (!badge) return;
    fetch('/api/wishlist/count', { credentials: 'include' })
        .then(function (r) {
            if (r.status === 401) {
                var g = getGuestWishlist();
                badge.textContent = g.length;
                badge.style.display = g.length > 0 ? '' : 'none';
                return null;
            }
            return r.json();
        })
        .then(function (d) {
            if (d) {
                var c = d.count || 0;
                badge.textContent = c;
                badge.style.display = c > 0 ? '' : 'none';
            }
        })
        .catch(function () {
            var g = getGuestWishlist();
            badge.textContent = g.length;
            badge.style.display = g.length > 0 ? '' : 'none';
        });
};

// Wishlist toggle - guests use localStorage only (no API); logged-in use API
window.wishlistToggle = function (btn, productId, inWishlist) {
    var pid = parseInt(productId, 10) || productId;
    var isGuest = document.body.getAttribute('data-user-authenticated') !== 'true';

    function updateButton(nowInWishlist) {
        btn.dataset.inWishlist = nowInWishlist ? 'true' : 'false';
        btn.classList.toggle('btn-danger', nowInWishlist);
        btn.classList.toggle('btn-outline-danger', !nowInWishlist);
        var icon = btn.querySelector('i');
        if (icon) {
            icon.classList.toggle('bi-heart-fill', nowInWishlist);
            icon.classList.toggle('bi-heart', !nowInWishlist);
        }
        updateWishlistCount();
    }

    if (isGuest) {
        var g = getGuestWishlist().map(function (id) { return parseInt(id, 10) || id; });
        if (inWishlist) {
            g = g.filter(function (id) { return id !== pid; });
        } else {
            if (g.indexOf(pid) === -1) g.push(pid);
        }
        setGuestWishlist(g);
        updateButton(!inWishlist);
        return;
    }

    var method = inWishlist ? 'DELETE' : 'POST';
    fetch('/api/wishlist/' + pid, { method: method, credentials: 'include' })
        .then(function (r) {
            if (r.status === 401) {
                var g = getGuestWishlist().map(function (id) { return parseInt(id, 10) || id; });
                if (inWishlist) {
                    g = g.filter(function (id) { return id !== pid; });
                } else {
                    if (g.indexOf(pid) === -1) g.push(pid);
                }
                setGuestWishlist(g);
                updateButton(!inWishlist);
                return null;
            }
            return r.json();
        })
        .then(function (d) {
            if (!d) return;
            updateButton(!inWishlist);
        })
        .catch(function () {
            updateButton(inWishlist);
        });
};

// Add to Cart: submit via API with JSON to avoid 415 Unsupported Media Type
$(document).on('submit', '.js-add-to-cart-form', function (e) {
    e.preventDefault();
    var form = e.target;
    var productId = parseInt(form.getAttribute('data-product-id'), 10) || parseInt(form.querySelector('input[name="productId"]')?.value, 10);
    var quantity = parseInt(form.getAttribute('data-quantity'), 10) || parseInt(form.querySelector('input[name="quantity"]')?.value, 10) || 1;
    if (!productId) return;
    var btn = form.querySelector('button[type="submit"]');
    if (btn) { btn.disabled = true; btn.textContent = 'Adding...'; }
    fetch('/api/cart', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productId: productId, variantId: null, quantity: quantity }),
        credentials: 'include'
    })
        .then(function (r) {
            if (r.status === 401) {
                window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
                return null;
            }
            if (!r.ok) throw new Error('Add to cart failed');
            return r.json();
        })
        .then(function (d) {
            if (!d) return;
            var badge = document.getElementById('cartCountBadge');
            if (badge) badge.textContent = d.cartCount != null ? d.cartCount : (d.count != null ? d.count : (parseInt(badge.textContent, 10) || 0) + 1);
            if (typeof loadCartDrawer === 'function') loadCartDrawer();
            var container = document.getElementById('toastContainer');
            if (container && typeof bootstrap !== 'undefined') {
                var toastEl = document.createElement('div');
                toastEl.className = 'toast align-items-center text-bg-success border-0';
                toastEl.setAttribute('role', 'alert');
                toastEl.innerHTML = '<div class="d-flex"><div class="toast-body">Added to cart.</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div>';
                container.appendChild(toastEl);
                var toast = new bootstrap.Toast(toastEl, { autohide: true, delay: 2500 });
                toast.show();
                toastEl.addEventListener('hidden.bs.toast', function () { toastEl.remove(); });
            }
            var modal = form.closest('.modal');
            if (modal && bootstrap.Modal.getInstance(modal)) bootstrap.Modal.getInstance(modal).hide();
        })
        .catch(function () {
            if (btn) { btn.disabled = false; btn.textContent = 'Add to Cart'; }
            alert('Could not add to cart. Please try again.');
        })
        .finally(function () {
            if (btn && !btn.disabled) { btn.disabled = false; btn.textContent = 'Add to Cart'; }
        });
});

// Global click handler for wishlist buttons (cards + detail modal) - works on every page, guest or logged-in
$(document).on('click', '.wishlist-btn, .wishlist-btn-detail', function (e) {
    e.preventDefault();
    e.stopPropagation();
    var btn = this;
    var productId = parseInt(btn.getAttribute('data-product-id'), 10) || btn.getAttribute('data-product-id');
    if (!productId) return;
    var inWishlist = btn.getAttribute('data-in-wishlist') === 'true';
    if (typeof wishlistToggle === 'function') wishlistToggle(btn, productId, inWishlist);
});

// Sync guest wishlist to server after login
window.syncGuestWishlist = function () {
    var g = getGuestWishlist();
    if (g.length === 0) return;
    fetch('/api/wishlist/sync', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ productIds: g }),
        credentials: 'include'
    }).then(function () {
        setGuestWishlist([]);
        updateWishlistCount();
    }).catch(function () {});
};

// Live search: show matching products dropdown
(function () {
    var input = document.getElementById('navSearchInput');
    var wrap = document.querySelector('.nav-search-wrap');
    var dropdown = document.getElementById('searchSuggestions');
    var form = document.getElementById('navSearchForm');
    if (!input || !dropdown) return;
    var debounceTimer;
    input.addEventListener('input', function () {
        var q = (input.value || '').trim();
        clearTimeout(debounceTimer);
        if (q.length < 2) {
            dropdown.style.display = 'none';
            return;
        }
        debounceTimer = setTimeout(function () {
            dropdown.innerHTML = '<div class="suggestion-loading">Searching...</div>';
            dropdown.style.display = 'block';
            fetch('/api/shop/search?q=' + encodeURIComponent(q) + '&take=8', { credentials: 'include' })
                .then(function (r) { return r.json(); })
                .then(function (list) {
                    if (!list || list.length === 0) {
                        dropdown.innerHTML = '<div class="suggestion-empty">No matching products.</div>';
                        return;
                    }
                    var html = '';
                    list.forEach(function (p) {
                        var img = (p.mainImageUrl || '/images/placeholder.svg');
                        var url = '/Product/Details/' + (p.id || '');
                        html += '<a class="suggestion-item d-block" href="' + url + '">' +
                            '<img src="' + img + '" alt="" onerror="this.src=\'/images/placeholder.svg\'" />' +
                            '<div class="flex-grow-1"><div class="suggestion-name">' + (p.name || '').replace(/</g, '&lt;') + '</div>' +
                            '<div class="suggestion-price">Rs ' + (p.displayPrice != null ? Number(p.displayPrice).toLocaleString() : '') + '</div></div></a>';
                    });
                    dropdown.innerHTML = html;
                })
                .catch(function () {
                    dropdown.innerHTML = '<div class="suggestion-empty">Search unavailable.</div>';
                });
        }, 280);
    });
    input.addEventListener('focus', function () {
        if ((input.value || '').trim().length >= 2 && dropdown.innerHTML && !dropdown.querySelector('.suggestion-loading'))
            dropdown.style.display = 'block';
    });
    input.addEventListener('blur', function () {
        setTimeout(function () { dropdown.style.display = 'none'; }, 180);
    });
    document.addEventListener('click', function (e) {
        if (wrap && !wrap.contains(e.target)) dropdown.style.display = 'none';
    });
})();

// Product detail modal: open on product card / Get Quote click
$(document).on('click', '[data-product-detail-id]', function (e) {
    e.preventDefault();
    var id = $(this).data('product-detail-id');
    if (!id) return;
    var modal = document.getElementById('productDetailModal');
    var body = document.getElementById('productDetailModalBody');
    if (!modal || !body) return;
    body.innerHTML = '<div class="text-center py-5 text-muted"><span class="spinner-border spinner-border-sm me-2"></span>Loading...</div>';
    var bsModal = bootstrap.Modal.getOrCreateInstance(modal);
    bsModal.show();
    fetch('/Product/DetailsPartial/' + id, { credentials: 'include', headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(function (r) { return r.text(); })
        .then(function (html) {
            body.innerHTML = html;
            var carouselEl = body.querySelector('[data-bs-ride="carousel"]');
            if (carouselEl && typeof bootstrap !== 'undefined') {
                try { new bootstrap.Carousel(carouselEl); } catch (e) {}
            }
            var script = body.querySelector('script');
            if (script) { eval(script.textContent); }
            $(body).find('.wishlist-btn-detail').each(function () {
                var el = this;
                var pid = parseInt(el.getAttribute('data-product-id'), 10);
                if (typeof getGuestWishlist === 'function' && !isNaN(pid)) {
                    var g = getGuestWishlist();
                    if (g.indexOf(pid) >= 0) {
                        el.setAttribute('data-in-wishlist', 'true');
                        el.classList.remove('btn-outline-danger'); el.classList.add('btn-danger');
                        var icon = el.querySelector('i'); if (icon) { icon.classList.remove('bi-heart'); icon.classList.add('bi-heart-fill'); }
                    }
                }
            });
        })
        .catch(function () {
            body.innerHTML = '<div class="text-center py-5 text-danger">Failed to load product.</div>';
        });
});

// Cart drawer: load items when opened and after add-to-cart
function loadCartDrawer() {
    var container = document.getElementById('cartDrawerContent');
    if (!container) return;
    container.innerHTML = '<p class="text-muted text-center"><span class="spinner-border spinner-border-sm me-2"></span>Loading cart...</p>';
    fetch('/api/cart/items', { credentials: 'include' })
        .then(function (r) {
            if (r.status === 401) { container.innerHTML = '<p class="text-muted text-center">Please log in to view cart.</p>'; return null; }
            return r.json();
        })
        .then(function (items) {
            if (items == null) return;
            if (!items.length) {
                container.innerHTML = '<p class="text-muted text-center mb-0">Your cart is empty.</p>';
                return;
            }
            var html = '<div class="cart-drawer-items">';
            items.forEach(function (it) {
                var img = (it.imageUrl || '/images/placeholder.svg');
                var name = (it.productName || '').replace(/</g, '&lt;');
                var price = (it.price != null ? Number(it.price).toLocaleString() : '');
                var subtotal = (it.quantity * (it.price || 0)).toLocaleString();
                html += '<div class="d-flex align-items-center gap-2 mb-3 pb-3 border-bottom">' +
                    '<img src="' + img + '" alt="" class="rounded" style="width:50px;height:50px;object-fit:cover" onerror="this.src=\'/images/placeholder.svg\'" />' +
                    '<div class="flex-grow-1 min-width-0">' +
                    '<a href="/Product/Details/' + it.productId + '" class="text-dark text-decoration-none text-truncate d-block small fw-medium">' + name + '</a>' +
                    (it.variantCombination ? '<small class="text-muted d-block">' + (it.variantCombination || '').replace(/</g, '&lt;') + '</small>' : '') +
                    '<span class="small text-muted">Rs ' + price + ' × ' + it.quantity + ' = Rs ' + subtotal + '</span>' +
                    '</div></div>';
            });
            html += '</div>';
            container.innerHTML = html;
        })
        .catch(function () {
            container.innerHTML = '<p class="text-muted text-center">Could not load cart.</p>';
        });
}

$(function () {
    var cartDrawer = document.getElementById('cartDrawer');
    if (cartDrawer) {
        cartDrawer.addEventListener('shown.bs.offcanvas', function () { loadCartDrawer(); });
    }
    var cartBadge = document.getElementById('cartCountBadge');
    if (cartBadge) {
        fetch('/api/cart/count', { credentials: 'include' })
            .then(function (r) { return r.json(); })
            .then(function (d) { cartBadge.textContent = d.count || 0; })
            .catch(function () {});
    }
    var wishlistBadge = document.getElementById('wishlistCountBadge');
    if (wishlistBadge) {
        updateWishlistCount(); /* works for both guest (localStorage count) and logged-in (API count) */
        if (document.body.dataset.userAuthenticated === 'true') syncGuestWishlist();
    }
    var chatBadge = document.getElementById('chatUnreadBadge');
    if (chatBadge) {
        fetch('/api/chat/unread-count', { credentials: 'include' })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                var c = d.count || 0;
                chatBadge.textContent = c;
                chatBadge.style.display = c > 0 ? '' : 'none';
            })
            .catch(function () {});
    }
});
