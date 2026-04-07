// ============================================================
// ShopVN - Main JavaScript
// ============================================================

document.addEventListener('DOMContentLoaded', function () {

    // Load cart count on page load
    loadCartCount();

    // ── CART COUNT ──
    function loadCartCount() {
        fetch('/Cart/GetCount')
            .then(r => r.json())
            .then(count => {
                const badge = document.getElementById('cartBadge');
                if (badge) {
                    badge.textContent = count;
                    badge.style.display = count > 0 ? 'flex' : 'none';
                }
            })
            .catch(() => {});
    }

    // ── TOAST NOTIFICATION ──
    window.showToast = function (message, type = 'success') {
        let container = document.querySelector('.toast-container-custom');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container-custom';
            document.body.appendChild(container);
        }

        const toast = document.createElement('div');
        toast.className = 'toast-custom';
        const icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';
        const color = type === 'success' ? '#198754' : '#dc3545';
        toast.style.borderLeftColor = color;
        toast.innerHTML = `
            <i class="fas ${icon} toast-icon" style="color:${color}"></i>
            <span class="toast-msg">${message}</span>
        `;
        container.appendChild(toast);
        setTimeout(() => {
            toast.style.animation = 'slideIn .3s ease reverse';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    };

    // ── ADD TO CART ──
    document.querySelectorAll('.btn-add-cart, .btn-add-cart-detail').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            const productID = this.dataset.productId;
            const qtyInput = document.getElementById('qtyInput');
            const quantity = qtyInput ? parseInt(qtyInput.value) || 1 : 1;

            fetch('/Cart/AddToCart', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `productID=${productID}&quantity=${quantity}`
            })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    showToast(data.message);
                    const badge = document.getElementById('cartBadge');
                    if (badge) {
                        badge.textContent = data.cartCount;
                        badge.style.display = 'flex';
                    }
                    // Animate button
                    const btn = e.target.closest('button');
                    if (btn) {
                        btn.innerHTML = '<i class="fas fa-check me-1"></i>Đã thêm!';
                        btn.style.background = '#198754';
                        setTimeout(() => {
                            btn.innerHTML = '<i class="fas fa-cart-plus me-1"></i>Thêm vào giỏ';
                            btn.style.background = '';
                        }, 2000);
                    }
                } else {
                    showToast(data.message, 'error');
                }
            });
        });
    });

    // ── CART QUANTITY UPDATE ──
    document.querySelectorAll('.qty-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const productID = this.dataset.productId;
            const input = document.querySelector(`.qty-input[data-product-id="${productID}"]`);
            if (!input) return;
            let qty = parseInt(input.value);
            if (this.dataset.action === 'plus') qty++;
            else qty = Math.max(1, qty - 1);
            input.value = qty;
            updateCartQuantity(productID, qty);
        });
    });

    document.querySelectorAll('.qty-input').forEach(input => {
        input.addEventListener('change', function () {
            const productID = this.dataset.productId;
            const qty = parseInt(this.value);
            if (qty < 1) { this.value = 1; return; }
            updateCartQuantity(productID, qty);
        });
    });

    function updateCartQuantity(productID, quantity) {
        fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `productID=${productID}&quantity=${quantity}`
        })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                // Update subtotal
                const subtotalEl = document.querySelector(`.item-subtotal[data-product-id="${productID}"]`);
                if (subtotalEl) {
                    // Recalculate
                    const price = parseFloat(document.querySelector(`.item-price[data-product-id="${productID}"]`)?.dataset.price || 0);
                    subtotalEl.textContent = formatCurrency(price * quantity);
                }
                // Update total
                const totalEl = document.getElementById('cartTotal');
                if (totalEl) totalEl.textContent = data.total + ' ₫';
                const badge = document.getElementById('cartBadge');
                if (badge) { badge.textContent = data.count; }
            }
        });
    }

    // ── REMOVE CART ITEM ──
    document.querySelectorAll('.btn-remove-item').forEach(btn => {
        btn.addEventListener('click', function () {
            const productID = this.dataset.productId;
            const row = this.closest('tr');
            fetch('/Cart/RemoveItem', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: `productID=${productID}`
            })
            .then(r => r.json())
            .then(data => {
                if (data.success) {
                    if (row) row.remove();
                    location.reload();
                }
            });
        });
    });

    // ── PRODUCT QUANTITY (DETAIL PAGE) ──
    const qtyInput = document.getElementById('qtyInput');
    if (qtyInput) {
        document.getElementById('qtyMinus')?.addEventListener('click', () => {
            if (qtyInput.value > 1) qtyInput.value--;
        });
        document.getElementById('qtyPlus')?.addEventListener('click', () => {
            qtyInput.value++;
        });
    }

    // ── FORMAT CURRENCY ──
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount);
    }

    // ── FILTER PRICE RANGE ──
    const priceForm = document.getElementById('filterForm');
    if (priceForm) {
        document.querySelectorAll('.filter-label input').forEach(radio => {
            radio.addEventListener('change', () => priceForm.submit());
        });
    }

    // ── IMAGE PLACEHOLDER ──
    document.querySelectorAll('img').forEach(img => {
        img.addEventListener('error', function () {
            this.src = 'https://via.placeholder.com/300x300/f8f9fa/aaa?text=No+Image';
        });
    });
});
