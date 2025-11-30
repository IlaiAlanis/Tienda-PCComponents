let userData = null;
let wishlistItems = [];

async function initWishlistPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadUserData();
    await loadWishlist();
}

async function loadUserData() {
    try {
        const response = await API.users.getProfile();
        
        if (response.success && response.data) {
            userData = response.data;
            populateUserInfo();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function populateUserInfo() {
    const fullName = `${userData.nombre} ${userData.apellido || ''}`.trim();
    document.getElementById('userName').textContent = fullName;
    document.getElementById('userEmail').textContent = userData.correo;
    
    const avatarUrl = userData.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff`;
    document.getElementById('userAvatar').src = avatarUrl;
}

async function loadWishlist() {
    const container = document.getElementById('wishlistContainer');
    container.innerHTML = '<div class="flex justify-center py-8"><div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></div>';

    try {
        const response = await API.wishlist.get();

        if (response.success && response.data) {
            wishlistItems = response.data.items || [];
            
            document.getElementById('itemCount').textContent = `${wishlistItems.length} Producto${wishlistItems.length !== 1 ? 's' : ''}`;
            
            if (wishlistItems.length === 0) {
                container.innerHTML = `
                    <div class="text-center py-12">
                        <span class="material-symbols-outlined text-6xl text-gray-400 mb-4">favorite_border</span>
                        <p class="text-gray-500 dark:text-gray-400 mb-4">No tienes productos en tu lista de deseos</p>
                        <a href="../pages/catalog.html" class="inline-block px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover">
                            Ver Productos
                        </a>
                    </div>
                `;
            } else {
                container.innerHTML = wishlistItems.map(item => createWishlistItem(item)).join('');
            }
        } else {
            ErrorHandler.handleApiError(response);
            container.innerHTML = `
                <div class="text-center py-12 text-red-500">
                    <span class="material-symbols-outlined text-6xl mb-4">error_outline</span>
                    <p class="text-lg">No se pudo cargar la lista de deseos</p>
                </div>
            `;
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        container.innerHTML = `
            <div class="text-center py-12 text-red-500">
                <span class="material-symbols-outlined text-6xl mb-4">wifi_off</span>
                <p class="text-lg">Error de conexión</p>
            </div>
        `;
    }
}

function createWishlistItem(item) {
    //  Backend returns FLAT properties, not nested under item.producto
    // Backend WishlistItemDto structure:
    // {
    //   idItem: 1,
    //   productoId: 123,
    //   nombreProducto: "Mouse",
    //   imagenUrl: "/img.jpg",
    //   precio: 599.99,
    //   enStock: true,
    //   fechaAgregado: "2024-11-27..."
    // }
    
    const imageUrl = item.imagenUrl || 'https://via.placeholder.com/80x80?text=Producto';
    const price = item.precio || 0;
    const inStock = item.enStock || false;
    const productName = item.nombreProducto || 'Producto sin nombre';
    const productId = item.productoId;
    
    // Determine stock status
    const stockStatus = inStock ? 'En stock' : 'Sin stock';
    const stockColor = inStock ? 'bg-green-500/20 text-green-400' : 'bg-red-500/20 text-red-400';

    return `
        <div class="py-4 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div class="flex items-center gap-4 flex-1">
                <img src="${imageUrl}" 
                     alt="${productName}" 
                     class="w-20 h-20 object-cover rounded-lg flex-shrink-0 cursor-pointer hover:opacity-80 transition-opacity"
                     onclick="window.location.href='../pages/product.html?id=${productId}'"
                     onerror="this.src='https://via.placeholder.com/80x80?text=Producto'"/>
                <div class="flex-1">
                    <h3 class="font-medium leading-tight mb-1 cursor-pointer hover:text-primary transition-colors"
                        onclick="window.location.href='../pages/product.html?id=${productId}'">
                        ${productName}
                    </h3>
                    <div class="flex items-center gap-2">
                        <p class="text-primary text-lg font-bold">$${price.toFixed(2)}</p>
                    </div>
                    <span class="inline-block mt-2 ${stockColor} text-xs font-medium px-2 py-1 rounded-full">
                        ${stockStatus}
                    </span>
                </div>
            </div>
            <div class="flex items-center gap-4 w-full sm:w-auto">
                <button onclick="addToCart(${productId})" 
                        ${!inStock ? 'disabled' : ''}
                        class="flex-1 sm:flex-none flex items-center justify-center gap-2 px-4 py-2 text-sm font-bold bg-primary hover:bg-primary-hover rounded-lg text-white transition-colors ${!inStock ? 'opacity-50 cursor-not-allowed' : ''}">
                    <span class="material-symbols-outlined text-base">add_shopping_cart</span>
                    Añadir
                </button>
                <button onclick="removeFromWishlist(${productId})" 
                        class="text-muted-light dark:text-muted-dark hover:text-red-500 transition-colors">
                    <span class="material-symbols-outlined">delete</span>
                </button>
            </div>
        </div>
    `;
}

async function addToCart(productId) {
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

async function removeFromWishlist(productId) {
    try {
        const response = await API.wishlist.removeItem(productId);

        if (response.success) {
            ErrorHandler.showToast('Eliminado de favoritos', 'info');
            await loadWishlist();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function addAllToCart() {
    if (wishlistItems.length === 0) {
        ErrorHandler.showToast('No hay productos en la lista', 'warning');
        return;
    }

    let successCount = 0;
    let failCount = 0;

    for (const item of wishlistItems) {
        if (item.enStock) {
            try {
                const response = await API.cart.addItem({ 
                    productoId: item.productoId, 
                    cantidad: 1 
                });
                
                if (response.success) {
                    successCount++;
                } else {
                    failCount++;
                }
            } catch (error) {
                failCount++;
            }
        }
    }

    if (successCount > 0) {
        ErrorHandler.showToast(`${successCount} producto${successCount !== 1 ? 's' : ''} agregado${successCount !== 1 ? 's' : ''} al carrito`, 'success');
        Components.updateCartBadge();
    }

    if (failCount > 0) {
        ErrorHandler.showToast(`${failCount} producto${failCount !== 1 ? 's' : ''} no se pudo${failCount !== 1 ? 'ieron' : ''} agregar`, 'warning');
    }
}

function openMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
    const currentPath = window.location.pathname;
    
    const menuItems = [
        { href: '../pages/user_profile.html', icon: 'person', label: 'Mi Perfil' },
        { href: '../pages/user_orders.html', icon: 'receipt_long', label: 'Mis Pedidos' },
        { href: '../pages/user_wishlist.html', icon: 'favorite', label: 'Mi Lista de Deseo' },
        { href: '../pages/user_address.html', icon: 'home', label: 'Direcciones' },
        { href: '../pages/user_payment_methods.html', icon: 'credit_card', label: 'Métodos de Pago' },
        { href: '../pages/user_notifications.html', icon: 'notifications', label: 'Mis Notificaciones' },
        { href: '../pages/user_preferences.html', icon: 'tune', label: 'Preferencias' }
    ];
    
    modal.innerHTML = `
        <div class="absolute inset-x-0 bottom-0 bg-surface-light dark:bg-surface-dark rounded-t-2xl max-h-[85vh] overflow-y-auto animate-slide-up">
            <div class="sticky top-0 bg-surface-light dark:bg-surface-dark border-b border-border-light dark:border-border-dark p-4 flex justify-between items-center">
                <h3 class="text-xl font-bold">Mi Cuenta</h3>
                <button onclick="closeMobileProfileMenu()" class="p-2 hover:bg-gray-100 dark:hover:bg-gray-700/50 rounded-lg">
                    <span class="material-symbols-outlined">close</span>
                </button>
            </div>

            <div class="p-4">
                <div class="flex items-center space-x-4 mb-6 pb-6 border-b border-border-light dark:border-border-dark">
                    <img id="modalUserAvatar" alt="Avatar" class="h-12 w-12 rounded-full object-cover"/>
                    <div>
                        <h4 id="modalUserName" class="font-semibold text-gray-900 dark:text-white"></h4>
                        <p id="modalUserEmail" class="text-sm text-muted-light dark:text-muted-dark"></p>
                    </div>
                </div>

                <nav class="space-y-2">
                    ${menuItems.map(item => {
                        const isActive = currentPath.includes(item.href.split('/').pop().split('.')[0]);
                        return `
                            <a href="${item.href}" 
                               class="flex items-center space-x-3 px-4 py-2.5 rounded-md ${isActive ? 'bg-primary text-white' : 'hover:bg-gray-100 dark:hover:bg-gray-700/50 text-muted-light dark:text-muted-dark hover:text-gray-900 dark:hover:text-white'} text-sm font-medium">
                                <span class="material-symbols-outlined text-base">${item.icon}</span>
                                <span>${item.label}</span>
                            </a>
                        `;
                    }).join('')}
                </nav>

                <div class="border-t border-border-light dark:border-border-dark mt-6 pt-4">
                    <button onclick="logout()" class="flex items-center space-x-3 px-4 py-2.5 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-danger text-sm font-medium w-full">
                        <span class="material-symbols-outlined text-base">logout</span>
                        <span>Cerrar Sesión</span>
                    </button>
                </div>
            </div>
        </div>
    `;
    
    if (userData) {
        const fullName = `${userData.nombre} ${userData.apellido || ''}`.trim();
        const avatarUrl = userData.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff`;
        document.getElementById('modalUserAvatar').src = avatarUrl;
        document.getElementById('modalUserName').textContent = fullName;
        document.getElementById('modalUserEmail').textContent = userData.correo;
    }
    
    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden';
    
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeMobileProfileMenu();
    });
}

function closeMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
    modal.classList.add('hidden');
    document.body.style.overflow = '';
}

function logout() {
    if (confirm('¿Estás seguro de cerrar sesión?')) {
        Auth.logout();
    }
}

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) {
        closeMobileProfileMenu();
    }
});

// Initialize page
initWishlistPage();