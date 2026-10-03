// Site-wide JavaScript functionality for Book Store

// Global variables
let cart = [];

// Initialize when document is ready
$(document).ready(function () {
    initializeSite();
    loadCartFromStorage();
    setupEventHandlers();
});

function initializeSite() {
    // Enable Bootstrap tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

    // Enable Bootstrap popovers
    const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
    const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl));

    // Set current year in footer
    $('#current-year').text(new Date().getFullYear());

    // Initialize image lazy loading
    initializeLazyLoading();

    // Initialize quantity controls
    initializeQuantityControls();
}

function initializeLazyLoading() {
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove('lazy');
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img.lazy').forEach(img => imageObserver.observe(img));
    } else {
        // Fallback for browsers without IntersectionObserver
        document.querySelectorAll('img.lazy').forEach(img => {
            img.src = img.dataset.src;
            img.classList.remove('lazy');
        });
    }
}

function initializeQuantityControls() {
    // Quantity increment/decrement
    $('.quantity-btn').on('click', function () {
        const $input = $(this).siblings('.quantity-input');
        let value = parseInt($input.val()) || 1;

        if ($(this).hasClass('decrement') && value > 1) {
            value--;
        } else if ($(this).hasClass('increment')) {
            value++;
        }

        $input.val(value).trigger('change');
    });

    // Quantity input validation
    $('.quantity-input').on('change', function () {
        let value = parseInt($(this).val()) || 1;
        if (value < 1) {
            value = 1;
            $(this).val(value);
        }
    });
}

function setupEventHandlers() {
    // Product search
    $('#search-form').on('submit', function (e) {
        e.preventDefault();
        const query = $('#search-input').val().trim();
        if (query) {
            window.location.href = `/Shop?search=${encodeURIComponent(query)}`;
        }
    });

    // Category filter
    $('.category-filter').on('click', function (e) {
        e.preventDefault();
        const categoryId = $(this).data('category-id');
        window.location.href = `/Shop?categoryId=${categoryId}`;
    });

    // Price range filter
    $('#price-range').on('input', function () {
        $('#price-value').text('$' + $(this).val());
    });

    // Sort by
    $('#sort-by').on('change', function () {
        const sortValue = $(this).val();
        const url = new URL(window.location.href);
        url.searchParams.set('sort', sortValue);
        window.location.href = url.toString();
    });

    // Add to wishlist
    $('.wishlist-btn').on('click', function () {
        const productId = $(this).data('product-id');
        toggleWishlist(productId, $(this));
    });

    // Quick view modal
    $('.quick-view-btn').on('click', function () {
        const productId = $(this).data('product-id');
        showQuickView(productId);
    });
}

// Cart functions
function loadCartFromStorage() {
    const savedCart = localStorage.getItem('bookstore_cart');
    if (savedCart) {
        try {
            cart = JSON.parse(savedCart);
            updateCartUI();
        } catch (e) {
            console.error('Error parsing cart from storage:', e);
            cart = [];
        }
    }
}

function saveCartToStorage() {
    localStorage.setItem('bookstore_cart', JSON.stringify(cart));
}

function updateCartUI() {
    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    const totalPrice = cart.reduce((sum, item) => sum + (item.price * item.quantity), 0);

    // Update cart count badge
    $('.cart-count').text(totalItems);

    // Update cart sidebar if open
    if ($('#cart-sidebar').length) {
        renderCartSidebar();
    }

    // Update cart page if open
    if ($('#cart-page').length) {
        renderCartPage();
    }
}

function addToCartLocal(productId, productName, price, imageUrl, quantity = 1) {
    const existingItem = cart.find(item => item.id === productId);

    if (existingItem) {
        existingItem.quantity += quantity;
    } else {
        cart.push({
            id: productId,
            name: productName,
            price: price,
            image: imageUrl,
            quantity: quantity
        });
    }

    saveCartToStorage();
    updateCartUI();
    showToast('Added to cart!', 'success');
}

function removeFromCart(productId) {
    cart = cart.filter(item => item.id !== productId);
    saveCartToStorage();
    updateCartUI();
    showToast('Removed from cart', 'info');
}

function updateCartQuantity(productId, quantity) {
    const item = cart.find(item => item.id === productId);
    if (item) {
        if (quantity <= 0) {
            removeFromCart(productId);
        } else {
            item.quantity = quantity;
            saveCartToStorage();
            updateCartUI();
        }
    }
}

function clearCart() {
    cart = [];
    saveCartToStorage();
    updateCartUI();
    showToast('Cart cleared', 'info');
}

// Wishlist functions
function toggleWishlist(productId, button) {
    let wishlist = JSON.parse(localStorage.getItem('bookstore_wishlist') || '[]');
    const index = wishlist.indexOf(productId);

    if (index === -1) {
        wishlist.push(productId);
        button.addClass('active').html('<i class="fas fa-heart"></i>');
        showToast('Added to wishlist', 'success');
    } else {
        wishlist.splice(index, 1);
        button.removeClass('active').html('<i class="far fa-heart"></i>');
        showToast('Removed from wishlist', 'info');
    }

    localStorage.setItem('bookstore_wishlist', JSON.stringify(wishlist));
    $('.wishlist-count').text(wishlist.length);
}

// UI Helper functions
function showToast(message, type = 'info') {
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-bg-${type} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;

    $('#toast-container').append(toastHtml);
    const toast = new bootstrap.Toast(document.getElementById(toastId));
    toast.show();

    // Remove toast after it's hidden
    document.getElementById(toastId).addEventListener('hidden.bs.toast', function () {
        this.remove();
    });
}

function showLoading(show = true) {
    if (show) {
        $('#loading-overlay').fadeIn(200);
    } else {
        $('#loading-overlay').fadeOut(200);
    }
}

// Form validation
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function validatePhone(phone) {
    const re = /^[\+]?[1-9][\d]{0,15}$/;
    return re.test(phone);
}

// API Helper functions
async function makeApiRequest(url, method = 'GET', data = null) {
    showLoading(true);
    
    try {
        const options = {
            method: method,
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            }
        };

        if (data && (method === 'POST' || method === 'PUT')) {
            options.body = JSON.stringify(data);
        }

        const response = await fetch(url, options);
        const result = await response.json();

        if (!response.ok) {
            throw new Error(result.message || 'Request failed');
        }

        return result;
    } catch (error) {
        console.error('API Request error:', error);
        showToast(error.message || 'An error occurred', 'danger');
        throw error;
    } finally {
        showLoading(false);
    }
}

// Export functions for use in other scripts
window.BookStore = {
    addToCart: addToCartLocal,
    removeFromCart: removeFromCart,
    updateCartQuantity: updateCartQuantity,
    clearCart: clearCart,
    toggleWishlist: toggleWishlist,
    showToast: showToast,
    validateEmail: validateEmail,
    validatePhone: validatePhone,
    makeApiRequest: makeApiRequest
};