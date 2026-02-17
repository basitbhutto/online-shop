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
            badge.classList.toggle('d-none', g.length === 0);
        });
};

// Wishlist toggle - works for guests (localStorage) and logged-in (API)
window.wishlistToggle = function (btn, productId, inWishlist) {
    var pid = parseInt(productId, 10) || productId;
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
                btn.dataset.inWishlist = inWishlist ? 'false' : 'true';
                btn.classList.toggle('btn-danger', !inWishlist);
                btn.classList.toggle('btn-outline-danger', inWishlist);
                var icon = btn.querySelector('i');
                if (icon) { icon.classList.toggle('bi-heart-fill', !inWishlist); icon.classList.toggle('bi-heart', inWishlist); }
                updateWishlistCount();
                return null;
            }
            return r.json();
        })
        .then(function (d) {
            if (!d) return;
            btn.dataset.inWishlist = inWishlist ? 'false' : 'true';
            btn.classList.toggle('btn-danger', !inWishlist);
            btn.classList.toggle('btn-outline-danger', inWishlist);
            var icon = btn.querySelector('i');
            if (icon) { icon.classList.toggle('bi-heart-fill', !inWishlist); icon.classList.toggle('bi-heart', inWishlist); }
            updateWishlistCount();
        });
};

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

$(function () {
    var cartBadge = document.getElementById('cartCountBadge');
    if (cartBadge) {
        fetch('/api/cart/count', { credentials: 'include' })
            .then(function (r) { return r.json(); })
            .then(function (d) { cartBadge.textContent = d.count || 0; })
            .catch(function () {});
    }
    var wishlistBadge = document.getElementById('wishlistCountBadge');
    if (wishlistBadge) {
        updateWishlistCount();
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
