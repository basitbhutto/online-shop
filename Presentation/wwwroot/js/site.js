// Shopwala - Cart count, Wishlist count and utilities
window.updateWishlistCount = function () {
    var badge = document.getElementById('wishlistCountBadge');
    if (!badge) return;
    fetch('/api/wishlist/count', { credentials: 'include' })
        .then(r => r.json())
        .then(d => { badge.textContent = d.count || 0; })
        .catch(() => {});
};
$(function () {
    var cartBadge = document.getElementById('cartCountBadge');
    if (cartBadge) {
        fetch('/api/cart/count', { credentials: 'include' })
            .then(r => r.json())
            .then(d => { cartBadge.textContent = d.count || 0; })
            .catch(() => {});
    }
    if (document.getElementById('wishlistCountBadge')) {
        updateWishlistCount();
    }
});
