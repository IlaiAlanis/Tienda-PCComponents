let currentProduct = null;
let selectedVariation = null;
let selectedQuantity = 1;
let maxQuantity = 1;
let isInWishlist = false;

async function initProductPage() {
    const urlParams = new URLSearchParams(window.location.search);
    const productId = urlParams.get('id');

    if (!productId) {
        window.location.href = '../pages/404.html';
        return;
    }

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    if (Auth.isAuthenticated()) {
        Components.updateCartBadge();
    }

    await loadProduct(productId);
}

async function loadProduct(productId) {
    document.getElementById('loadingState').classList.remove('hidden');
    document.getElementById('productSection').classList.add('hidden');
    
    try {
        const response = await API.products.getById(productId);

        if (response.success && response.data) {
            currentProduct = response.data;
            maxQuantity = currentProduct.stock || currentProduct.cantidadStock || 1;
            renderProduct();
            document.getElementById('productSection').classList.remove('hidden');
            
            if (Auth.isAuthenticated()) {
                await checkWishlist();
            }
            
            await loadRelatedProducts();
            await loadReviews();
        } else {
            ErrorHandler.handleApiError(response);
            ErrorHandler.showToast('Producto no encontrado', 'error');
            setTimeout(() => window.location.href = '../pages/404.html', 2000);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        ErrorHandler.showToast('Error al cargar el producto', 'error');
        setTimeout(() => window.location.href = '../pages/catalog.html', 2000);
    } finally {
        document.getElementById('loadingState').classList.add('hidden');
    }
}

function renderProduct() {
    // Update breadcrumbs
    const categoryName = currentProduct.categoriaNombre || currentProduct.categoria || 'Productos';
    document.getElementById('breadcrumbCategory').textContent = categoryName;
    
    const productName = currentProduct.nombre || currentProduct.nombreProducto || 'Producto sin nombre';
    document.getElementById('breadcrumbProduct').textContent = productName;

    // Update title
    document.title = `${productName} - PC Components`;
    document.getElementById('productName').textContent = productName;

    // Render images
    renderProductImages();

    // Product info
    const brandName = currentProduct.marcaNombre || currentProduct.marca || 'Sin marca';
    const sku = currentProduct.sku || currentProduct.idProducto;
    const description = currentProduct.descripcion || 'Sin descripción disponible';
    
    document.getElementById('productBrand')?.textContent && (document.getElementById('productBrand').textContent = brandName);
    document.getElementById('productSKU').textContent = sku;
    document.getElementById('productCategory')?.textContent && (document.getElementById('productCategory').textContent = categoryName);
    document.getElementById('productDescription').textContent = description;

    // Price
    const precioBase = currentProduct.precioBase || 0;
    const precioPromocional = currentProduct.precioPromocional;
    const price = precioPromocional || precioBase;
    const hasDiscount = precioPromocional && precioPromocional < precioBase;

    document.getElementById('currentPrice').textContent = `$${price.toFixed(2)}`;
    
    if (hasDiscount) {
        const discount = Math.round(((precioBase - precioPromocional) / precioBase) * 100);
        document.getElementById('originalPrice').innerHTML = `$${precioBase.toFixed(2)}`;
        document.getElementById('originalPrice').classList.remove('hidden');
    }

    // Stock status
    const stock = currentProduct.stock || currentProduct.cantidadStock || 0;
    const stockBadge = document.getElementById('stockBadge');
    
    if (stock > 0) {
        if (stock < 5) {
            stockBadge.className = 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400';
            stockBadge.innerHTML = '<span class="material-icons text-sm mr-1">warning</span> Últimas unidades';
        } else {
            stockBadge.className = 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400';
            stockBadge.innerHTML = '<span class="material-icons text-sm mr-1">check_circle</span> En stock';
        }
        
        // Enable buttons
        const addToCartBtn = document.getElementById('addToCartBtn');
        if (addToCartBtn) addToCartBtn.disabled = false;
    } else {
        stockBadge.className = 'inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400';
        stockBadge.innerHTML = '<span class="material-icons text-sm mr-1">cancel</span> Agotado';
        
        // Disable buttons
        const addToCartBtn = document.getElementById('addToCartBtn');
        if (addToCartBtn) addToCartBtn.disabled = true;
    }

    // Specifications
    if (currentProduct.especificaciones || currentProduct.atributos) {
        renderSpecifications();
    }
}

function renderProductImages() {
    // Handle multiple image formats from backend
    let images = [];
    
    if (currentProduct.imagenes && Array.isArray(currentProduct.imagenes)) {
        // Array of image objects with urlImagen property
        images = currentProduct.imagenes.map(img => img.urlImagen || img);
    } else if (currentProduct.imagenUrl) {
        // Single image URL
        images = [currentProduct.imagenUrl];
    } else if (currentProduct.imagen) {
        // Alternative single image property
        images = [currentProduct.imagen];
    }
    
    if (images.length === 0) {
        images = ['https://via.placeholder.com/600x600?text=Sin+Imagen'];
    }

    // Main image
    document.getElementById('mainImage').src = images[0];
    document.getElementById('mainImage').alt = currentProduct.nombre || currentProduct.nombreProducto || 'Producto';
    document.getElementById('mainImage').onerror = function() {
        this.src = 'https://via.placeholder.com/600x600?text=Sin+Imagen';
    };

    // Thumbnails
    const thumbnailsHtml = images.map((img, index) => `
        <img src="${img}" 
             alt="${currentProduct.nombre || currentProduct.nombreProducto} ${index + 1}"
             onclick="changeImage('${img}')"
             class="w-20 h-20 object-cover rounded-lg cursor-pointer border-2 ${index === 0 ? 'border-primary' : 'border-transparent'} hover:border-primary transition-colors"
             id="thumb-${index}"
             onerror="this.src='https://via.placeholder.com/100x100?text=Imagen'"/>
    `).join('');

    document.getElementById('thumbnails').innerHTML = thumbnailsHtml;
}

function changeImage(imageUrl) {
    const mainImage = document.getElementById('mainImage');
    mainImage.src = imageUrl;
    
    // Update active thumbnail
    document.querySelectorAll('#thumbnails img').forEach((thumb) => {
        thumb.classList.remove('border-primary');
        thumb.classList.add('border-transparent');
        if (thumb.src === imageUrl || thumb.onclick.toString().includes(imageUrl)) {
            thumb.classList.add('border-primary');
            thumb.classList.remove('border-transparent');
        }
    });
}

function renderSpecifications() {
    const specsContainer = document.getElementById('specsList');
    if (!specsContainer) return;

    const specs = currentProduct.especificaciones || currentProduct.atributos || {};
    
    if (typeof specs === 'object' && Object.keys(specs).length > 0) {
        const specsHtml = Object.entries(specs).map(([key, value]) => `
            <div class="flex justify-between py-3 border-b border-gray-200 dark:border-gray-700 last:border-b-0">
                <span class="font-medium">${key}</span>
                <span class="text-gray-600 dark:text-gray-400">${value}</span>
            </div>
        `).join('');
        specsContainer.innerHTML = specsHtml;
    } else {
        specsContainer.innerHTML = '<p class="text-gray-500 text-center py-4">No hay especificaciones disponibles</p>';
    }
}

function incrementQuantity() {
    if (selectedQuantity < maxQuantity) {
        selectedQuantity++;
        document.getElementById('quantity').value = selectedQuantity;
    }
}

function decrementQuantity() {
    if (selectedQuantity > 1) {
        selectedQuantity--;
        document.getElementById('quantity').value = selectedQuantity;
    }
}

async function addToCart() {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión para agregar al carrito', 'warning');
        setTimeout(() => {
            window.location.href = `../pages/login.html?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`;
        }, 1500);
        return;
    }

    const btn = document.getElementById('addToCartBtn');
    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="material-icons animate-spin">refresh</span> Agregando...';

    try {
        const response = await API.cart.addItem({
            productoId: currentProduct.idProducto,
            cantidad: selectedQuantity
        });

        if (response.success) {
            ErrorHandler.showToast('Producto agregado al carrito', 'success');
            Components.updateCartBadge();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        btn.disabled = false;
        btn.innerHTML = originalText;
    }
}

async function toggleWishlist() {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión para agregar a favoritos', 'warning');
        setTimeout(() => {
            window.location.href = `../pages/login.html?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`;
        }, 1500);
        return;
    }

    const btn = document.getElementById('wishlistBtn');
    const icon = btn.querySelector('.material-icons');

    try {
        if (isInWishlist) {
            const response = await API.wishlist.removeItem(currentProduct.idProducto);
            if (response.success) {
                isInWishlist = false;
                icon.textContent = 'favorite_border';
                ErrorHandler.showToast('Eliminado de favoritos', 'info');
            } else {
                ErrorHandler.handleApiError(response);
            }
        } else {
            const response = await API.wishlist.addItem(currentProduct.idProducto);
            if (response.success) {
                isInWishlist = true;
                icon.textContent = 'favorite';
                ErrorHandler.showToast('Agregado a favoritos', 'success');
            } else {
                ErrorHandler.handleApiError(response);
            }
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function checkWishlist() {
    try {
        const response = await API.wishlist.get();
        
        if (response.success && response.data) {
            const items = response.data.items || [];
            isInWishlist = items.some(item => item.productoId === currentProduct.idProducto);
            
            const icon = document.getElementById('wishlistBtn')?.querySelector('.material-icons');
            if (icon) {
                icon.textContent = isInWishlist ? 'favorite' : 'favorite_border';
            }
        }
    } catch (error) {
        console.error('Error checking wishlist:', error);
    }
}

async function loadRelatedProducts() {
    const container = document.getElementById('relatedProducts');
    if (!container) return;

    container.innerHTML = '<div class="col-span-full flex justify-center py-8"><div class="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></div>';

    try {
        const categoryId = currentProduct.categoriaId || currentProduct.idCategoria;
        
        const response = await API.products.search({
            categoriaId: categoryId,
            pageSize: 4
        });

        if (response.success && response.data) {
            const products = (response.data.items || response.data)
                .filter(p => p.idProducto !== currentProduct.idProducto)
                .slice(0, 4);

            if (products.length > 0) {
                const html = products.map(product => createRelatedProductCard(product)).join('');
                container.innerHTML = html;
            } else {
                container.innerHTML = '<p class="col-span-full text-center text-gray-500">No hay productos relacionados</p>';
            }
        } else {
            container.innerHTML = '';
        }
    } catch (error) {
        console.error('Error loading related products:', error);
        container.innerHTML = '';
    }
}

function createRelatedProductCard(product) {
    const id = product.idProducto;
    const nombre = product.nombre || product.nombreProducto;
    const precio = product.precioPromocional || product.precioBase;
    
    let imagenUrl = 'https://via.placeholder.com/200x200?text=Producto';
    if (product.imagenes && product.imagenes.length > 0) {
        imagenUrl = product.imagenes[0].urlImagen || product.imagenes[0];
    } else if (product.imagenUrl) {
        imagenUrl = product.imagenUrl;
    }

    return `
        <div class="bg-white dark:bg-gray-800 rounded-lg overflow-hidden shadow-md hover:shadow-xl transition-shadow">
            <img src="${imagenUrl}" 
                 alt="${nombre}"
                 class="w-full h-48 object-cover cursor-pointer hover:scale-105 transition-transform"
                 onclick="window.location.href='../pages/product_details.html?id=${id}'"
                 onerror="this.src='https://via.placeholder.com/200x200?text=Producto'"/>
            <div class="p-4">
                <h3 class="font-semibold text-lg mb-2 line-clamp-2 cursor-pointer hover:text-primary"
                    onclick="window.location.href='../pages/product_details.html?id=${id}'">
                    ${nombre}
                </h3>
                <p class="text-2xl font-bold text-primary mb-3">$${precio.toFixed(2)}</p>
                <button onclick="addRelatedToCart(${id})"
                        class="w-full bg-primary text-white px-4 py-2 rounded-lg hover:bg-primary-hover transition-colors">
                    Agregar
                </button>
            </div>
        </div>
    `;
}

async function addRelatedToCart(productId) {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión', 'warning');
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

async function loadReviews() {
    const container = document.getElementById('reviewsList');
    if (!container) return;
    
    try {
        const response = await API.reviews.getProductReviews(currentProduct.idProducto, 1, 5);

        if (response.success && response.data) {
            const reviews = response.data.items || response.data;
            document.getElementById('reviewCount').textContent = reviews.length;
            
            if (reviews.length === 0) {
                container.innerHTML = '<p class="text-center text-gray-500 py-8">No hay reseñas aún. ¡Sé el primero en escribir una!</p>';
            } else {
                container.innerHTML = reviews.map(review => createReviewCard(review)).join('');
            }
        }
    } catch (error) {
        console.error('Error loading reviews:', error);
        container.innerHTML = '<p class="text-center text-gray-500 py-8">No se pudieron cargar las reseñas</p>';
    }
}

function createReviewCard(review) {
    const stars = Array(5).fill(0).map((_, i) => 
        `<span class="material-icons text-sm ${i < review.calificacion ? 'text-yellow-400' : 'text-gray-300'}">star</span>`
    ).join('');

    return `
        <div class="bg-white dark:bg-gray-800 rounded-lg p-4 mb-4">
            <div class="flex items-center justify-between mb-2">
                <div class="flex items-center gap-2">
                    <div class="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-white font-bold text-sm">
                        ${review.nombreUsuario.charAt(0).toUpperCase()}
                    </div>
                    <span class="font-medium">${review.nombreUsuario}</span>
                </div>
                <div class="flex">${stars}</div>
            </div>
            <p class="text-gray-700 dark:text-gray-300 mb-2">${review.comentario}</p>
            <p class="text-sm text-gray-500">${new Date(review.fechaCreacion).toLocaleDateString('es-ES')}</p>
        </div>
    `;
}

function switchTab(tabName) {
    // Hide all tabs
    document.getElementById('specsContent').classList.add('hidden');
    document.getElementById('reviewsContent').classList.add('hidden');
    
    // Remove active class from all buttons
    document.getElementById('specsTab').classList.remove('active', 'border-primary', 'text-primary');
    document.getElementById('reviewsTab').classList.remove('active', 'border-primary', 'text-primary');
    
    // Show selected tab
    if (tabName === 'specs') {
        document.getElementById('specsContent').classList.remove('hidden');
        document.getElementById('specsTab').classList.add('active', 'border-primary', 'text-primary');
    } else if (tabName === 'reviews') {
        document.getElementById('reviewsContent').classList.remove('hidden');
        document.getElementById('reviewsTab').classList.add('active', 'border-primary', 'text-primary');
    }
}

function shareProduct() {
    if (navigator.share) {
        navigator.share({
            title: currentProduct.nombre || currentProduct.nombreProducto,
            text: currentProduct.descripcion,
            url: window.location.href
        }).catch(err => console.log('Error sharing:', err));
    } else {
        navigator.clipboard.writeText(window.location.href);
        ErrorHandler.showToast('Link copiado al portapapeles', 'success');
    }
}

// Initialize page
initProductPage();