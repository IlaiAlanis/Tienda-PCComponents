const Components = {
    // NAVBAR
    navbar() {
        const user = Auth.getUser();
        const isAuth = Auth.isAuthenticated();

        return `
        <header class="bg-background-light/80 dark:bg-background-dark/80 backdrop-blur-sm sticky top-0 z-50 border-b border-gray-200 dark:border-gray-800">
            <nav class="container mx-auto px-4 lg:px-8 py-4">
                <div class="flex justify-between items-center">
                    <!-- Logo & Search -->
                    <div class="flex items-center gap-8">
                        <a class="flex items-center gap-2" href="index.html">
                            <span class="material-icons text-primary text-3xl">desktop_windows</span>
                            <span class="text-xl font-bold text-gray-900 dark:text-white">PC Components</span>
                        </a>
                        
                        <!-- UPDATED SEARCH BAR -->
                        <div class="hidden lg:flex relative w-80">
                            <span class="material-icons absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500">search</span>
                            <input 
                                class="w-full bg-gray-200 dark:bg-gray-800 border-transparent focus:border-primary focus:ring-primary rounded-lg pl-10 pr-12 py-2 text-sm" 
                                placeholder="Buscar productos..." 
                                type="text" 
                                id="globalSearch" 
                                onkeypress="handleGlobalSearch(event)"
                            />
                            <!-- Search button -->
                            <button 
                                onclick="executeGlobalSearch()"
                                class="absolute right-2 top-1/2 -translate-y-1/2 text-primary hover:text-primary-hover transition-colors"
                                title="Buscar"
                            >
                                <span class="material-icons">search</span>
                            </button>
                        </div>
                    </div>
                    
                    <!-- Desktop Menu -->
                    <div class="hidden lg:flex items-center gap-6 text-sm font-medium text-gray-600 dark:text-gray-300">
                        <a class="hover:text-primary transition-colors" href="catalog.html?category=CPU">CPUs</a>
                        <a class="hover:text-primary transition-colors" href="catalog.html?category=GPU">GPUs</a>
                        <a class="hover:text-primary transition-colors" href="catalog.html?category=Motherboard">Placas Madre</a>
                        <a class="hover:text-primary transition-colors" href="catalog.html?category=Storage">Almacenamiento</a>
                        <a class="hover:text-primary transition-colors" href="catalog.html?category=RAM">RAM</a>
                        <a class="hover:text-primary transition-colors" href="faq.html">FAQ</a>
                        
                        <!-- Cart -->
                        <a class="relative hover:text-primary transition-colors" href="cart.html">
                            <span class="material-icons">shopping_cart</span>
                            <span id="cartBadge" class="hidden absolute -top-2 -right-2 bg-primary text-white text-xs font-bold rounded-full w-5 h-5 flex items-center justify-center"></span>
                        </a>
                        
                        <!-- User Menu -->
                        ${isAuth ? this.userDropdown(user) : this.loginButton()}
                    </div>
                    
                    <!-- Mobile Menu Button -->
                    <button onclick="toggleMobileMenu()" class="lg:hidden text-gray-700 dark:text-gray-300">
                        <span class="material-icons">menu</span>
                    </button>
                </div>
                
                <!-- Mobile Search Bar -->
                <div class="lg:hidden mt-4">
                    <div class="relative">
                        <span class="material-icons absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 dark:text-gray-500">search</span>
                        <input 
                            class="w-full bg-gray-200 dark:bg-gray-800 border-transparent focus:border-primary focus:ring-primary rounded-lg pl-10 pr-12 py-2 text-sm" 
                            placeholder="Buscar productos..." 
                            type="text" 
                            id="mobileSearch" 
                            onkeypress="handleGlobalSearch(event)"
                        />
                        <button 
                            onclick="executeGlobalSearch()"
                            class="absolute right-2 top-1/2 -translate-y-1/2 text-primary hover:text-primary-hover transition-colors"
                        >
                            <span class="material-icons">search</span>
                        </button>
                    </div>
                </div>
            </nav>
        </header>
    `;
    },

    
    //  User Dropdown with full name + apellidoMaterno support
    userDropdown(user) {
        // Build full name with apellidoMaterno support
        const nameParts = [
            user.nombre || user.nombreUsuario,
            user.apellido || user.apellidoPaterno,
            user.apellidoMaterno
        ].filter(Boolean);

        const fullName = nameParts.join(' ') || user.nombreUsuario || 'Usuario';
        const initials = this.getInitials(fullName);

        // Avatar URL (if available) or use initials
        const avatarUrl = user.avatarUrl || user.imagenPerfil;

        return `
            <div class="relative group">
                <button class="flex items-center gap-2 hover:text-primary transition-colors">
                    ${avatarUrl ? `
                        <img src="${avatarUrl}" alt="${fullName}" class="w-10 h-10 rounded-full object-cover border-2 border-primary"/>
                    ` : `
                        <div class="w-10 h-10 rounded-full bg-primary flex items-center justify-center">
                            <span class="text-white font-bold text-sm">${initials}</span>
                        </div>
                    `}
                    <span class="hidden xl:block font-medium text-gray-900 dark:text-white">${fullName}</span>
                    <span class="material-icons text-sm">expand_more</span>
                </button>
                <div class="absolute right-0 mt-2 w-64 bg-white dark:bg-surface-dark border border-gray-200 dark:border-border-dark rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
                    <div class="px-4 py-3 border-b border-gray-200 dark:border-border-dark">
                        <p class="font-semibold text-gray-900 dark:text-white">${fullName}</p>
                        <p class="text-sm text-gray-500 dark:text-muted-dark">${user.correo}</p>
                        ${user.correoVerificado === false ? `
                            <span class="inline-block mt-1 text-xs bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-400 px-2 py-0.5 rounded">
                                Email no verificado
                            </span>
                        ` : ''}
                    </div>
                    <a href="../pages/user_profile.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">person</span>
                            <span>Mi Perfil</span>
                        </div>
                    </a>
                    <a href="../pages/user_orders.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">receipt_long</span>
                            <span>Mis Pedidos</span>
                        </div>
                    </a>
                    <a href="../pages/user_wishlist.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">favorite</span>
                            <span>Lista de Deseos</span>
                        </div>
                    </a>
                    <a href="../pages/user_addresses.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">home</span>
                            <span>Direcciones</span>
                        </div>
                    </a>
                    <a href="../pages/payment_methods.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">credit_card</span>
                            <span>Métodos de Pago</span>
                        </div>
                    </a>
                    <a href="../pages/user_notifications.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-primary text-base">notifications</span>
                            <span>Notificaciones</span>
                        </div>
                    </a>
                    ${user.rol === 'Admin' || user.rol === 'ADMIN' ? `
                        <div class="border-t border-gray-200 dark:border-border-dark"></div>
                        <a href="../pages/admin/users_dashboard.html" class="block px-4 py-3 hover:bg-gray-100 dark:hover:bg-gray-700/50 transition-colors text-gray-700 dark:text-gray-300">
                            <div class="flex items-center gap-3">
                                <span class="material-icons text-primary text-base">admin_panel_settings</span>
                                <span>Panel Admin</span>
                            </div>
                        </a>
                    ` : ''}
                    <div class="border-t border-gray-200 dark:border-border-dark"></div>
                    <button onclick="Auth.logout()" class="w-full text-left px-4 py-3 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors text-red-600 dark:text-red-400">
                        <div class="flex items-center gap-3">
                            <span class="material-icons text-base">logout</span>
                            <span>Cerrar Sesión</span>
                        </div>
                    </button>
                </div>
            </div>
        `;
    },

    //  NEW: Get initials from name
    getInitials(name) {
        if (!name) return 'U';
        const parts = name.trim().split(' ').filter(Boolean);
        if (parts.length >= 2) {
            return (parts[0][0] + parts[1][0]).toUpperCase();
        }
        return name.substring(0, 2).toUpperCase();
    },

    // Login Button (for navbar)
    loginButton() {
        return `
            <a href="../pages/login.html" class="flex items-center gap-2 bg-gray-200 dark:bg-gray-800 px-4 py-2 rounded-lg hover:bg-gray-300 dark:hover:bg-gray-700 transition-colors">
                <span class="material-icons text-base">person</span>
                <span>Iniciar Sesión</span>
            </a>
        `;
    },

    // FOOTER
    footer() {
        return `
            <footer class="bg-gray-100 dark:bg-gray-950 border-t border-gray-200 dark:border-gray-800">
                <div class="container mx-auto px-4 lg:px-8 py-12">
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                        <div>
                            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">PC Components</h3>
                            <p class="text-sm text-gray-600 dark:text-gray-400">Tu tienda de confianza para los mejores componentes de PC. Potencia, rendimiento y calidad al mejor precio.</p>
                        </div>
                        <div>
                            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Enlaces Rápidos</h3>
                            <ul class="space-y-2 text-sm">
                                <li><a class="text-gray-600 dark:text-gray-400 hover:text-primary transition-colors" href="../pages/about.html">Quiénes Somos</a></li>
                                <li><a class="text-gray-600 dark:text-gray-400 hover:text-primary transition-colors" href="../pages/faq.html">Preguntas Frecuentes</a></li>
                                <li><a class="text-gray-600 dark:text-gray-400 hover:text-primary transition-colors" href="#">Términos y Condiciones</a></li>
                                <li><a class="text-gray-600 dark:text-gray-400 hover:text-primary transition-colors" href="#">Política de Envíos</a></li>
                            </ul>
                        </div>
                        <div>
                            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Contacto</h3>
                            <p class="text-sm text-gray-600 dark:text-gray-400">Email: contacto@pccomponents.com</p>
                            <div class="flex gap-4 mt-4">
                                <a class="text-gray-500 dark:text-gray-400 hover:text-primary transition-colors" href="#"><svg aria-hidden="true" class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path d="M8.29 20.251c7.547 0 11.675-6.253 11.675-11.675 0-.178 0-.355-.012-.53A8.348 8.348 0 0022 5.92a8.19 8.19 0 01-2.357.646 4.118 4.118 0 001.804-2.27 8.224 8.224 0 01-2.605.996 4.107 4.107 0 00-6.993 3.743 11.65 11.65 0 01-8.457-4.287 4.106 4.106 0 001.27 5.477A4.072 4.072 0 012.8 9.71v.052a4.105 4.105 0 003.292 4.022 4.095 4.095 0 01-1.853.07 4.108 4.108 0 003.834 2.85A8.233 8.233 0 012 18.407a11.616 11.616 0 006.29 1.84"/></svg></a>
                                <a class="text-gray-500 dark:text-gray-400 hover:text-primary transition-colors" href="#"><svg aria-hidden="true" class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path clip-rule="evenodd" d="M12 2C6.477 2 2 6.477 2 12c0 4.242 2.766 7.828 6.539 9.01.478.088.653-.207.653-.46 0-.226-.009-.824-.014-1.618-2.685.584-3.25-1.296-3.25-1.296-.434-1.103-1.06-1.396-1.06-1.396-.867-.593.065-.58.065-.58.958.067 1.462 1.004 1.462 1.004.851 1.46.223 1.037 2.775.793.087-.616.333-1.037.608-1.275-2.119-.24-4.347-1.06-4.347-4.714 0-1.04.37-1.892 1.003-2.558-.1-.242-.435-1.21.096-2.522 0 0 .79-.256 2.625.975A9.15 9.15 0 0112 6.848c.81.003 1.62.11 2.378.325 1.83-1.23 2.62-.975 2.62-.975.534 1.312.198 2.28.098 2.522.635.666 1.003 1.518 1.003 2.558 0 3.664-2.23 4.47-4.357 4.704.342.295.65.878.65 1.77 0 1.275-.012 2.302-.012 2.613 0 .256.173.553.657.459A10.025 10.025 0 0022 12c0-5.523-4.477-10-10-10z" fill-rule="evenodd"/></svg></a>
                            </div>
                        </div>
                    </div>
                    <div class="mt-8 border-t border-gray-200 dark:border-gray-800 pt-8 text-center text-sm text-gray-500 dark:text-gray-400">
                        <p>© 2025 PC Components. Todos los derechos reservados.</p>
                    </div>
                </div>
            </footer>
        `;
    },

    // MOBILE MENU
    mobileMenu() {
        return `
            <div class="fixed inset-0 bg-black/50 z-50 hidden" onclick="closeMobileMenu()">
                <div class="fixed right-0 top-0 h-full w-64 bg-surface-dark shadow-xl" onclick="event.stopPropagation()">
                    <div class="p-6">
                        <button onclick="closeMobileMenu()" class="text-white mb-8">
                            <span class="material-icons">close</span>
                        </button>
                        <nav class="space-y-4">
                            <a href="../pages/catalog.html?category=CPU" class="block text-white hover:text-primary py-2">CPUs</a>
                            <a href="../pages/catalog.html?category=GPU" class="block text-white hover:text-primary py-2">GPUs</a>
                            <a href="../pages/catalog.html?category=Motherboard" class="block text-white hover:text-primary py-2">Placas Madre</a>
                            <a href="../pages/catalog.html?category=Storage" class="block text-white hover:text-primary py-2">Almacenamiento</a>
                            <a href="../pages/catalog.html?category=RAM" class="block text-white hover:text-primary py-2">RAM</a>
                            <a href="../pages/faq.html" class="block text-white hover:text-primary py-2">FAQ</a>
                        </nav>
                    </div>
                </div>
            </div>
        `;
    },

    //  : Product card component (SHARED - used by both index and catalog)
    productCard(product) {
        const price = product.precioPromocional || product.precioBase;
        const hasDiscount = product.precioPromocional && product.precioPromocional < product.precioBase;

        // Handle different image formats
        let imageUrl = 'https://via.placeholder.com/300x300?text=Producto';
        if (product.imagenes && product.imagenes.length > 0) {
            imageUrl = product.imagenes[0].urlImagen || product.imagenes[0];
        } else if (product.imagenUrl) {
            imageUrl = product.imagenUrl;
        }

        const inStock = (product.stock || product.cantidadStock || 0) > 0;
        const stockAmount = product.stock || product.cantidadStock || 0;
        const discountPercent = hasDiscount ? Math.round(((product.precioBase - product.precioPromocional) / product.precioBase) * 100) : 0;
        const productId = product.idProducto;
        const productName = product.nombre || product.nombreProducto;
        const category = product.categoria || product.nombreCategoria || 'Sin categoría';

        return `
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm overflow-hidden hover:shadow-xl transition-shadow duration-300 group">
            <!-- Image Container -->
            <div class="relative bg-gray-100 dark:bg-gray-900 p-4 h-64">
                <a href="../pages/product_details.html?id=${productId}" class="block h-full">
                    <img 
                        src="${imageUrl}" 
                        alt="${productName}"
                        class="w-full h-full object-contain group-hover:scale-105 transition-transform duration-300"
                        onerror="this.src='https://via.placeholder.com/300x300?text=Sin+Imagen'"
                    />
                </a>
                
                <!-- Discount Badge -->
                ${hasDiscount ? `
                    <span class="absolute top-2 right-2 bg-red-500 text-white text-xs font-bold px-2 py-1 rounded-full shadow-lg">
                        -${discountPercent}%
                    </span>
                ` : ''}
                
                <!-- Wishlist Button -->
                ${Auth.isAuthenticated() ? `
                    <button 
                        onclick="toggleWishlist(${productId}); event.preventDefault(); event.stopPropagation();"
                        class="absolute top-2 left-2 bg-white dark:bg-gray-800 p-2 rounded-full shadow-md hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                        title="Agregar a favoritos"
                    >
                        <span class="material-icons text-gray-600 dark:text-gray-400 text-xl">favorite_border</span>
                    </button>
                ` : ''}
                
                <!-- Stock Badge -->
                <div class="absolute bottom-2 left-2">
                    ${inStock ?
                stockAmount < 5 ?
                    '<span class="bg-orange-500/90 text-white text-xs font-medium px-2 py-1 rounded shadow">Últimas unidades</span>' :
                    '<span class="bg-green-500/90 text-white text-xs font-medium px-2 py-1 rounded shadow">En stock</span>'
                : '<span class="bg-red-500/90 text-white text-xs font-medium px-2 py-1 rounded shadow">Agotado</span>'
            }
                </div>
            </div>

            <!-- Product Info -->
            <div class="p-4">
                <!-- Category -->
                <p class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">
                    ${category}
                </p>

                <!-- Product Name -->
                <a 
                    href="../pages/product_details.html?id=${productId}"
                    class="block font-semibold text-lg mb-2 hover:text-primary transition-colors line-clamp-2 min-h-[3.5rem]"
                    title="${productName}"
                >
                    ${productName}
                </a>

                <!-- Description -->
                ${product.descripcion ? `
                    <p class="text-sm text-gray-600 dark:text-gray-400 mb-3 line-clamp-2 min-h-[2.5rem]">
                        ${product.descripcion}
                    </p>
                ` : '<div class="mb-3 min-h-[2.5rem]"></div>'}

                <!-- Rating (if available) -->
                ${product.rating ? `
                    <div class="flex items-center gap-1 mb-3">
                        <div class="flex text-yellow-400">
                            ${[...Array(5)].map((_, i) => `
                                <span class="material-icons text-sm">${i < Math.floor(product.rating) ? 'star' : 'star_border'}</span>
                            `).join('')}
                        </div>
                        <span class="text-xs text-gray-500 dark:text-gray-400">
                            (${product.numeroReviews || 0})
                        </span>
                    </div>
                ` : ''}

                <!-- Price & Action -->
                <div class="flex items-center justify-between pt-3 border-t border-gray-200 dark:border-gray-700">
                    <div>
                        ${hasDiscount ? `
                            <p class="text-sm text-gray-500 dark:text-gray-400 line-through">
                                $${product.precioBase.toFixed(2)}
                            </p>
                        ` : ''}
                        <p class="text-2xl font-bold text-primary">
                            $${price.toFixed(2)}
                        </p>
                    </div>

                    <!-- Add to Cart Button -->
                    <button 
                        onclick="addToCart(${productId}); event.preventDefault(); event.stopPropagation();"
                        ${!inStock ? 'disabled' : ''}
                        class="bg-primary hover:bg-blue-600 text-white p-3 rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
                        title="${inStock ? 'Agregar al carrito' : 'No disponible'}"
                    >
                        <span class="material-icons">${inStock ? 'add_shopping_cart' : 'remove_shopping_cart'}</span>
                    </button>
                </div>
            </div>
        </div>
    `;
    },

    //  Update cart badge with better error handling
    async updateCartBadge() {
        const badge = document.getElementById('cartBadge');
        if (!badge) return;

        if (!Auth.isAuthenticated()) {
            badge.classList.add('hidden');
            return;
        }

        try {
            const response = await API.cart.get();
            if (response.success && response.data) {
                const count = response.data.totalItems || 0;
                badge.textContent = count;
                badge.classList.toggle('hidden', count === 0);
            } else {
                badge.classList.add('hidden');
            }
        } catch (error) {
            console.error('Failed to update cart badge:', error);
            badge.classList.add('hidden');
        }
    }
};

// Global search functions
window.handleGlobalSearch = function(event) {
    if (event.key === 'Enter') {
        event.preventDefault();
        executeGlobalSearch();
    }
};

window.executeGlobalSearch = function() {
    const searchInput = document.getElementById('globalSearch') || document.getElementById('mobileSearch');
    if (searchInput && searchInput.value.trim()) {
        const searchTerm = encodeURIComponent(searchInput.value.trim());
        window.location.href = `catalog.html?search=${searchTerm}`;
    }
};

// Initialize cart badge on page load
if (typeof document !== 'undefined') {
    document.addEventListener('DOMContentLoaded', () => {
        Components.updateCartBadge();
    });
}