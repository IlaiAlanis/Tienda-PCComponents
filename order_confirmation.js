let orderData = null;

async function initOrderConfirmationPage() {
    if (!Auth.requireAuth()) return;

    const urlParams = new URLSearchParams(window.location.search);
    const orderId = urlParams.get('order');

    if (!orderId) {
        ErrorHandler.showToast('Pedido no encontrado', 'error');
        setTimeout(() => window.location.href = '../pages/orders.html', 1500);
        return;
    }

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadOrder(orderId);
}

async function loadOrder(orderId) {
    document.getElementById('loadingState').classList.remove('hidden');

    try {
        const response = await API.orders.getById(orderId);

        if (response.success && response.data) {
            orderData = response.data;
            renderOrderConfirmation();
        } else {
            ErrorHandler.handleApiError(response);
            ErrorHandler.showToast('No se pudo cargar el pedido', 'error');
            setTimeout(() => window.location.href = '../pages/orders.html', 2000);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        setTimeout(() => window.location.href = '../pages/orders.html', 2000);
    } finally {
        document.getElementById('loadingState').classList.add('hidden');
    }
}

function renderOrderConfirmation() {
    document.getElementById('orderNumber').textContent = `#${orderData.idOrden}`;
    document.getElementById('orderDate').textContent = formatDate(orderData.fechaPedido);
    document.getElementById('estimatedDelivery').textContent = getEstimatedDelivery(orderData.fechaPedido);

    // Render order items
    renderOrderItems();

    // Render shipping info
    renderShippingInfo();

    // Render totals
    renderOrderTotals();

    document.getElementById('confirmationContent').classList.remove('hidden');

    // Confetti animation
    celebrateOrder();
}

function renderOrderItems() {
    const itemsHtml = orderData.items.map(item => {
        const imageUrl = item.producto?.imagenes?.[0] || item.producto?.imagenUrl || 'https://via.placeholder.com/80x80?text=P';
        
        return `
            <li class="flex gap-4 py-4 border-b border-gray-200 dark:border-gray-700 last:border-b-0">
                <img src="${imageUrl}" alt="${item.nombreProducto}" class="w-20 h-20 object-cover rounded-lg"/>
                <div class="flex-1">
                    <h4 class="font-semibold mb-1">${item.nombreProducto}</h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Cantidad: ${item.cantidad}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">Precio unitario: $${item.precioUnitario.toFixed(2)}</p>
                </div>
                <p class="font-bold text-lg">$${(item.precioUnitario * item.cantidad).toFixed(2)}</p>
            </li>
        `;
    }).join('');

    document.getElementById('orderItemsList').innerHTML = itemsHtml;
}

function renderShippingInfo() {
    const address = orderData.direccionEnvio;
    
    if (address) {
        document.getElementById('shippingAddress').innerHTML = `
            <p class="font-semibold mb-2">${address.nombre}</p>
            <p class="text-sm text-gray-600 dark:text-gray-400">${address.calle}</p>
            <p class="text-sm text-gray-600 dark:text-gray-400">${address.ciudad}, ${address.estado} ${address.codigoPostal}</p>
            <p class="text-sm text-gray-600 dark:text-gray-400 mt-2">Tel: ${address.telefono}</p>
        `;
    }

    document.getElementById('paymentMethod').textContent = orderData.metodoPago || 'No especificado';
}

function renderOrderTotals() {
    const subtotal = orderData.subtotal || 0;
    const shipping = orderData.costoEnvio || 0;
    const tax = orderData.impuestos || 0;
    const discount = orderData.descuento || 0;
    const total = orderData.total || 0;

    document.getElementById('orderSubtotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('orderShipping').textContent = shipping === 0 ? 'GRATIS' : `$${shipping.toFixed(2)}`;
    document.getElementById('orderTax').textContent = `$${tax.toFixed(2)}`;
    document.getElementById('orderTotal').textContent = `$${total.toFixed(2)}`;

    if (discount > 0) {
        document.getElementById('orderDiscountRow').classList.remove('hidden');
        document.getElementById('orderDiscount').textContent = `-$${discount.toFixed(2)}`;
    }
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', { 
        weekday: 'long',
        year: 'numeric', 
        month: 'long', 
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function getEstimatedDelivery(orderDate) {
    const date = new Date(orderDate);
    date.setDate(date.getDate() + 5); // Add 5 business days
    return date.toLocaleDateString('es-ES', { 
        weekday: 'long',
        year: 'numeric', 
        month: 'long', 
        day: 'numeric'
    });
}

function celebrateOrder() {
    // Simple confetti effect (you can replace with a library like canvas-confetti)
    const duration = 3 * 1000;
    const end = Date.now() + duration;

    (function frame() {
        if (Date.now() < end) {
            requestAnimationFrame(frame);
        }
    }());
}

function printOrder() {
    window.print();
}

async function downloadInvoice() {
    try {
        const blob = await API.invoices.downloadPdf(orderData.idOrden);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `factura-${orderData.idOrden}.pdf`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        ErrorHandler.showToast('Factura descargada', 'success');
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function continueShopping() {
    window.location.href = '../pages/catalog.html';
}

function viewOrders() {
    window.location.href = '../pages/user_orders.html';
}

initOrderConfirmationPage();