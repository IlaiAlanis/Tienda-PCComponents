async function initHomePage() {
    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    if (Auth.isAuthenticated()) {
        Components.updateCartBadge();
    }

    await loadFeaturedProducts();
    await loadSpecialOffers();
    await loadCategories();
}

async function loadFeaturedProducts() {
    const container = document.getElementById('featuredProducts');
    container.innerHTML = createLoadingSpinner();
    
    try {
        const response = await API.products.getFeatured();
        
        if (response.success && response.data) {
            const products = (response.data.items || response.data).slice(0, 8);
            
            if (products.length > 0) {
                container.innerHTML = products.map(product => Components.productCard(product)).join('');
            } else {
                container.innerHTML = createEmptyState('No hay productos destacados disponibles');
            }
        } else {
            ErrorHandler.handleApiError(response);
            container.innerHTML = createEmptyState('No hay productos destacados disponibles');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        container.innerHTML = createErrorState('Error cargando productos destacados');
    }
}

async function loadSpecialOffers() {
    const container = document.getElementById('specialOffers');
    container.innerHTML = createLoadingSpinner();
    
    try {
        const response = await API.products.search({ 
            page: 1, 
            pageSize: 8,
            sortBy: 'precio',
            order: 'desc'
        });
        
        if (response.success && response.data) {
            const products = (response.data.items || response.data).slice(0, 8);
            
            if (products.length > 0) {
                container.innerHTML = products.map(product => Components.productCard(product)).join('');
            } else {
                container.innerHTML = createEmptyState('No hay ofertas especiales disponibles');
            }
        } else {
            ErrorHandler.handleApiError(response);
            container.innerHTML = createEmptyState('No hay ofertas disponibles');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        container.innerHTML = createErrorState('Error cargando ofertas');
    }
}

async function loadCategories() {
    const container = document.getElementById('categoriesGrid');
    
    try {
        const response = await API.categories.getAll();
        
        if (response.success && response.data) {
            const categories = response.data.slice(0, 6);
            container.innerHTML = categories.map(cat => createCategoryCard(cat)).join('');
        }
    } catch (error) {
        console.error('Error loading categories:', error);
        // Categories are optional, so we don't show error to user
    }
}

function createCategoryCard(category) {
    const icons = {
        'CPU': 'memory',
        'GPU': 'videogame_asset',
        'Motherboard': 'developer_board',
        'RAM': 'storage',
        'Storage': 'save',
        'Power Supply': 'power',
        'Case': 'computer',
        'Cooling': 'ac_unit'
    };

    const icon = icons[category.nombreCategoria] || 'category';

    return `
        <a href="../pages/catalog.html?category=${encodeURIComponent(category.nombreCategoria)}" 
           class="group bg-white dark:bg-gray-800 rounded-xl p-6 hover:shadow-xl transition-all duration-300 border border-gray-200 dark:border-gray-700 hover:border-primary">
            <div class="flex items-center gap-4">
                <div class="w-12 h-12 rounded-lg bg-primary/10 flex items-center justify-center group-hover:bg-primary group-hover:scale-110 transition-all">
                    <span class="material-icons text-primary group-hover:text-white text-2xl">${icon}</span>
                </div>
                <div>
                    <h3 class="font-semibold text-lg group-hover:text-primary transition-colors">${category.nombreCategoria}</h3>
                    <p class="text-sm text-gray-500 dark:text-gray-400">${category.descripcion || 'Ver productos'}</p>
                </div>
            </div>
        </a>
    `;
}

// Add to cart from home page
async function addToCart(productId) {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión para agregar al carrito', 'warning');
        setTimeout(() => {
            window.location.href = `../pages/login.html?redirect=${encodeURIComponent(window.location.pathname)}`;
        }, 1500);
        return;
    }

    try {
        const response = await API.cart.addItem({ productoId: productId, cantidad: 1 });

        if (response.success) {
            ErrorHandler.showToast('Producto agregado al carrito', 'success');
            Components.updateCartBadge();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

// Toggle wishlist from home page
async function toggleWishlist(productId) {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión para agregar a favoritos', 'warning');
        setTimeout(() => {
            window.location.href = `../pages/login.html?redirect=${encodeURIComponent(window.location.pathname)}`;
        }, 1500);
        return;
    }

    try {
        const response = await API.wishlist.addItem(productId);

        if (response.success) {
            ErrorHandler.showToast('Agregado a favoritos', 'success');
        } else {
            // If already in wishlist, try to remove
            const removeResponse = await API.wishlist.removeItem(productId);
            if (removeResponse.success) {
                ErrorHandler.showToast('Eliminado de favoritos', 'info');
            } else {
                ErrorHandler.handleApiError(response);
            }
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

// Newsletter subscription
async function subscribeNewsletter(e) {
    e.preventDefault();
    
    const emailInput = document.getElementById('newsletterEmail');
    const email = emailInput.value.trim();

    if (!email) {
        ErrorHandler.showToast('Ingresa tu correo electrónico', 'warning');
        return;
    }

    try {
        const response = await API.newsletter.subscribe(email);

        if (response.success) {
            ErrorHandler.showToast('¡Suscripción exitosa! Revisa tu correo.', 'success');
            emailInput.value = '';
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

// Search functionality
function handleSearch(e) {
    if (e.key === 'Enter') {
        const query = e.target.value.trim();
        if (query) {
            window.location.href = `../pages/catalog.html?search=${encodeURIComponent(query)}`;
        }
    }
}

// Initialize page
initHomePage();

// Add newsletter form listener if exists
document.getElementById('newsletterForm')?.addEventListener('submit', subscribeNewsletter);