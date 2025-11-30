/**
 * Shopping Cart Page - FIXED to match backend CarritoItemDto structure
 * Backend returns FLAT properties on cart items
 * NOT nested under item.producto
 */

let cart = null;
const SHIPPING_COST = 149.00;
const TAX_RATE = 0.16;

async function initCartPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadCart();
}

async function loadCart() {
    document.getElementById('loadingState').classList.remove('hidden');

    try {
        const response = await API.cart.get();

        if (response.success && response.data) {
            cart = response.data;
            renderCart();
        } else {
            ErrorHandler.handleApiError(response);
            showEmptyCart();
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        showEmptyCart();
    } finally {
        document.getElementById('loadingState').classList.add('hidden');
    }
}

function renderCart() {
    if (!cart.items || cart.items.length === 0) {
        showEmptyCart();
        return;
    }

    document.getElementById('cartContent').classList.remove('hidden');
    document.getElementById('relatedSection').classList.remove('hidden');

    renderCartItems();
    updateSummary();
    loadRelatedProducts();
}

function showEmptyCart() {
    document.getElementById('loadingState').classList.add('hidden');
    document.getElementById('cartContent').classList.add('hidden');
    document.getElementById('relatedSection').classList.add('hidden');
    document.getElementById('emptyCart').classList.remove('hidden');
}

function renderCartItems() {
    const itemsHtml = cart.items.map(item => {
        const price = item.precioUnitario || 0;
        const cantidad = item.cantidad || 1;
        const itemTotal = item.subtotal || (price * cantidad);
        const imageUrl = item.imagenUrl || 'https://via.placeholder.com/100x100?text=Producto';
        const productName = item.nombre || 'Producto sin nombre'; // ✅ FIXED: JsonPropertyName is "nombre"
        const productId = item.productoId;
        const stockDisponible = item.stockDisponible || 0;
        const idCarritoItem = item.idCarritoItem;

        return `
            <li class="grid grid-cols-1 md:grid-cols-6 gap-4 p-4 border-b border-gray-200 dark:border-gray-700 last:border-b-0">
                <!-- Product Info -->
                <div class="col-span-1 md:col-span-3 flex items-center gap-4">
                    <img src="${imageUrl}" 
                         alt="${productName}" 
                         class="w-20 h-20 object-cover rounded cursor-pointer hover:opacity-80 transition-opacity"
                         onclick="window.location.href='../pages/product_details.html?id=${productId}'"
                         onerror="this.src='https://via.placeholder.com/100x100?text=Producto'"/>
                    <div class="flex-1">
                        <a href="../pages/product_details.html?id=${productId}" 
                           class="font-semibold hover:text-primary transition-colors">
                            ${productName}
                        </a>
                        <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">
                            ID: ${productId}
                        </p>
                    </div>
                </div>

                <!-- Price -->
                <div class="flex md:justify-center items-center">
                    <span class="md:hidden font-medium mr-2">Precio:</span>
                    <span class="font-semibold">$${price.toFixed(2)}</span>
                </div>

                <!-- Quantity Controls -->
                <div class="flex md:justify-center items-center">
                    <span class="md:hidden font-medium mr-2">Cantidad:</span>
                    <div class="flex items-center border border-gray-300 dark:border-gray-600 rounded-lg">
                        <button onclick="updateQuantity(${idCarritoItem}, ${cantidad - 1})" 
                                ${cantidad <= 1 ? 'disabled' : ''}
                                class="px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                            <span class="material-icons text-sm">remove</span>
                        </button>
                        <span class="px-4 py-2 min-w-[3rem] text-center">${cantidad}</span>
                        <button onclick="updateQuantity(${idCarritoItem}, ${cantidad + 1})" 
                                ${cantidad >= stockDisponible ? 'disabled' : ''}
                                class="px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed">
                            <span class="material-icons text-sm">add</span>
                        </button>
                    </div>
                </div>

                <!-- Total & Remove -->
                <div class="flex md:justify-end items-center gap-4">
                    <div class="flex-1 md:flex-none">
                        <span class="md:hidden font-medium">Total: </span>
                        <span class="font-bold text-lg">$${itemTotal.toFixed(2)}</span>
                    </div>
                    <button onclick="removeItem(${idCarritoItem})" 
                            class="text-red-500 hover:text-red-700 transition-colors">
                        <span class="material-icons">delete_outline</span>
                    </button>
                </div>
            </li>
        `;
    }).join('');

    document.getElementById('cartItems').innerHTML = itemsHtml;
}

function updateSummary() {
    // Use backend-calculated totals if available, otherwise calculate
    const subtotal = cart.subtotal || cart.items.reduce((sum, item) => {
        return sum + (item.subtotal || (item.precioUnitario * item.cantidad));
    }, 0);

    const descuentoTotal = cart.descuentoTotal || 0;
    const envioTotal = cart.envioTotal || (subtotal > 1000 ? 0 : SHIPPING_COST);
    const impuestoTotal = cart.impuestoTotal || ((subtotal - descuentoTotal + envioTotal) * TAX_RATE);
    const total = cart.total || (subtotal - descuentoTotal + envioTotal + impuestoTotal);

    document.getElementById('itemCount').textContent = cart.totalItems || cart.items.length;
    document.getElementById('subtotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('shipping').textContent = envioTotal === 0 ? 'GRATIS' : `$${envioTotal.toFixed(2)}`;
    document.getElementById('tax').textContent = `$${impuestoTotal.toFixed(2)}`;
    document.getElementById('total').textContent = `$${total.toFixed(2)}`;

    if (descuentoTotal > 0) {
        document.getElementById('discountRow').classList.remove('hidden');
        document.getElementById('discount').textContent = `-$${descuentoTotal.toFixed(2)}`;
    }
}

async function updateQuantity(itemId, newQuantity) {
    if (newQuantity < 1) return;

    try {
        const response = await API.cart.updateItem(itemId, { cantidad: newQuantity });

        if (response.success) {
            cart = response.data;
            renderCartItems();
            updateSummary();
            Components.updateCartBadge();
            ErrorHandler.showToast('Cantidad actualizada', 'success');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function removeItem(itemId) {
    if (!confirm('¿Estás seguro de eliminar este producto del carrito?')) return;

    try {
        const response = await API.cart.removeItem(itemId);

        if (response.success) {
            cart = response.data;
            if (!cart.items || cart.items.length === 0) {
                showEmptyCart();
            } else {
                renderCartItems();
                updateSummary();
            }
            Components.updateCartBadge();
            ErrorHandler.showToast('Producto eliminado del carrito', 'info');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function clearCart() {
    if (!confirm('¿Vaciar todo el carrito?')) return;

    try {
        const response = await API.cart.clear();

        if (response.success) {
            ErrorHandler.showToast('Carrito vaciado', 'info');
            
            // ✅ Reset cart object and UI immediately
            cart = null;
            showEmptyCart();
            Components.updateCartBadge();
            
            // Clear coupon input
            const couponInput = document.getElementById('couponCode');
            if (couponInput) couponInput.value = '';
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function applyCoupon() {
    const code = document.getElementById('couponInput').value.trim();
    const messageEl = document.getElementById('couponMessage');

    if (!code) {
        messageEl.textContent = 'Por favor ingresa un código';
        messageEl.className = 'mt-2 text-sm text-red-500';
        return;
    }

    try {
        const response = await API.cart.applyCoupon(code);

        if (response.success) {
            messageEl.textContent = '✓ Cupón aplicado correctamente';
            messageEl.className = 'mt-2 text-sm text-green-500';
            await loadCart();
        } else {
            messageEl.textContent = response.error?.message || 'Cupón inválido';
            messageEl.className = 'mt-2 text-sm text-red-500';
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        messageEl.textContent = 'Error al aplicar el cupón';
        messageEl.className = 'mt-2 text-sm text-red-500';
    }
}

async function loadRelatedProducts() {
    if (!cart.items || cart.items.length === 0) return;

    const container = document.getElementById('relatedProducts');

    try {
        // Get first product's category for related search
        const firstProductId = cart.items[0].productoId;
        const productResponse = await API.products.getById(firstProductId);

        if (!productResponse.success || !productResponse.data) return;

        const category = productResponse.data.categoriaId;

        const response = await API.products.search({
            categoriaId: category,
            pageSize: 4
        });

        if (response.success && response.data) {
            const products = (response.data.items || response.data)
                .filter(p => !cart.items.find(item => item.productoId === p.idProducto))
                .slice(0, 4);

            if (products.length > 0) {
                renderRelatedProducts(products);
            }
        }
    } catch (error) {
        console.error('Error loading related products:', error);
    }
}

function renderRelatedProducts(products) {
    const html = products.map(product => {
        const id = product.idProducto;
        const nombre = product.nombre;
        const precio = product.precioPromocional || product.precioBase;
        const imagenUrl = (product.imagenes && product.imagenes.length > 0)
            ? product.imagenes[0].urlImagen
            : 'https://via.placeholder.com/200x200?text=Producto';

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
                        Agregar al Carrito
                    </button>
                </div>
            </div>
        `;
    }).join('');

    document.getElementById('relatedProducts').innerHTML = html;
}

async function addRelatedToCart(productId) {
    try {
        const response = await API.cart.addItem({ productoId: productId, cantidad: 1 });

        if (response.success) {
            ErrorHandler.showToast('Producto agregado al carrito', 'success');
            cart = response.data;
            if (cart.items && cart.items.length > 0) {
                renderCart();
            }
            Components.updateCartBadge();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function proceedToCheckout() {
    window.location.href = '../pages/checkout.html';
}

// Initialize page
initCartPage();