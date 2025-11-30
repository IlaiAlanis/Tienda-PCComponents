let userData = null;
let orders = [];

// Pagination state
let currentPage = 1;
const pageSize = 10;
let totalPages = 1;
let totalOrders = 0;

async function initOrdersPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadUserData();
    await loadOrders();
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

async function loadOrders() {
    const container = document.getElementById('ordersContainer');
    container.innerHTML = createLoadingSpinner();

    try {
        //  Pass pagination parameters
        const response = await API.orders.getAll(currentPage, pageSize);

        if (response.success && response.data) {
            //  Extract from PagedResult structure
            orders = response.data.items || response.data;  // Handle both formats
            totalPages = response.data.totalPages || 1;
            totalOrders = response.data.totalCount || orders.length;

            if (orders.length === 0) {
                container.innerHTML = createEmptyState();
            } else {
                container.innerHTML = orders.map((order, index) => createOrderCard(order, index)).join('');
                //  Add pagination controls
                renderPagination();
            }
        } else {
            ErrorHandler.handleApiError(response);
            container.innerHTML = createErrorState('No se pudieron cargar los pedidos');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        container.innerHTML = createErrorState('Error de conexión al cargar pedidos');
    }
}

function createEmptyState() {
    return `
        <div class="text-center py-12">
            <span class="material-symbols-outlined text-6xl text-gray-400 mb-4">shopping_bag</span>
            <p class="text-gray-500 dark:text-gray-400">No tienes pedidos aún</p>
            <a href="../pages/catalog.html" class="inline-block mt-4 px-6 py-3 bg-primary text-white rounded-lg hover:bg-primary-hover">
                Ir a Comprar
            </a>
        </div>
    `;
}

function createOrderCard(order, index) {
    const statusColors = {
        'entregado': 'bg-green-100 text-green-800 dark:bg-green-900/50 dark:text-green-300',
        'enviado': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/50 dark:text-yellow-300',
        'procesando': 'bg-blue-100 text-blue-800 dark:bg-blue-900/50 dark:text-blue-300',
        'cancelado': 'bg-red-100 text-red-800 dark:bg-red-900/50 dark:text-red-300',
        'pendiente': 'bg-gray-100 text-gray-800 dark:bg-gray-900/50 dark:text-gray-300'
    };

    //  Use correct property names from backend
    const fechaPedido = order.fechaPedido || order.fechaOrden; // Handle both formats
    const estado = (order.estado || order.estatusVenta || 'Procesando').toLowerCase();

    const isExpanded = index === 0;

    return `
        <div class="bg-surface-light dark:bg-surface-dark rounded-lg ${isExpanded ? 'border border-primary/50' : ''}">
            <div class="p-6 flex items-center justify-between cursor-pointer" onclick="toggleOrder(${index})">
                <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 sm:gap-8 w-full items-center pr-4">
                    <div>
                        <p class="text-xs text-muted-light dark:text-muted-dark">Fecha</p>
                        <p class="font-medium">${formatDate(fechaPedido)}</p>
                    </div>
                    <div>
                        <p class="text-xs text-muted-light dark:text-muted-dark">Nº Pedido</p>
                        <p class="font-medium">#${order.idOrden}</p>
                    </div>
                    <div>
                        <p class="text-xs text-muted-light dark:text-muted-dark">Total</p>
                        <p class="font-medium">$${order.total.toFixed(2)}</p>
                    </div>
                    <div class="justify-self-start sm:justify-self-end">
                        <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${statusColors[estado] || statusColors.procesando}">
                            ${estado.charAt(0).toUpperCase() + estado.slice(1)}
                        </span>
                    </div>
                </div>
                <span class="material-symbols-outlined text-muted-light dark:text-muted-dark transform ${isExpanded ? 'rotate-180' : ''}" id="arrow-${index}">expand_more</span>
            </div>
            <div id="details-${index}" class="px-6 pb-6 border-t border-border-light dark:border-border-dark ${isExpanded ? '' : 'hidden'}">
                ${createOrderDetails(order)}
            </div>
        </div>
    `;
}

function createOrderDetails(order) {
    return `
        <h3 class="text-lg font-semibold pt-6 pb-4">Detalles del Pedido #${order.idOrden}</h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div>
                <h4 class="font-medium mb-3">Productos</h4>
                <div class="space-y-3">
                    ${order.items && order.items.length > 0 ? order.items.map(item => `
                        <div class="flex justify-between items-center text-sm">
                            <span>${item.nombreProducto} (x${item.cantidad})</span>
                            <span class="font-medium">$${(item.precioUnitario * item.cantidad).toFixed(2)}</span>
                        </div>
                    `).join('') : '<p class="text-sm text-muted-light dark:text-muted-dark">Sin productos</p>'}
                </div>
            </div>
            <div>
                <h4 class="font-medium mb-3">Método de Pago</h4>
                <p class="text-sm text-muted-light dark:text-muted-dark">${order.metodoPago || 'No especificado'}</p>
            </div>
            <div>
                <h4 class="font-medium mb-3">Dirección de Envío</h4>
                <p class="text-sm text-muted-light dark:text-muted-dark">${formatAddress(order.direccionEnvio)}</p>
            </div>
            <div>
                <h4 class="font-medium mb-3">Seguimiento</h4>
                ${order.tracking ? `<a class="text-sm text-primary font-medium hover:underline" href="#">Rastrear envío: ${order.tracking}</a>` : '<p class="text-sm text-muted-light dark:text-muted-dark">No disponible</p>'}
            </div>
        </div>
        <div class="mt-8 flex flex-col sm:flex-row gap-4">
            <button onclick="reorder(${order.idOrden})" class="w-full sm:w-auto flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-primary text-white font-medium text-sm hover:bg-primary-hover">
                <span class="material-symbols-outlined text-base">refresh</span>
                Repetir Pedido
            </button>
            ${(order.estado || order.estatusVenta || '').toLowerCase() === 'entregado' ? `
                <button onclick="returnOrder(${order.idOrden})" class="w-full sm:w-auto flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark font-medium text-sm hover:bg-background-light dark:hover:bg-background-dark">
                    <span class="material-symbols-outlined text-base">assignment_return</span>
                    Iniciar Devolución
                </button>
            ` : ''}
            ${canCancelOrder(order) ? `
                <button onclick="cancelOrder(${order.idOrden})" class="w-full sm:w-auto flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg bg-red-500 text-white font-medium text-sm hover:bg-red-600">
                    <span class="material-symbols-outlined text-base">cancel</span>
                    Cancelar Pedido
                </button>
            ` : ''}
        </div>
    `;
}

function canCancelOrder(order) {
    const estado = (order.estado || order.estatusVenta || '').toLowerCase();
    return estado === 'pendiente' || estado === 'procesando';
}

async function cancelOrder(orderId) {
    if (!confirm('¿Estás seguro de que deseas cancelar este pedido?')) return;

    try {
        const response = await API.orders.cancel(orderId);

        if (response.success) {
            ErrorHandler.showToast('Pedido cancelado exitosamente', 'success');
            await loadOrders();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

// ✅ NEW: Pagination controls
function renderPagination() {
    let container = document.getElementById('paginationContainer');

    if (!container) {
        // Create pagination container
        const ordersContainer = document.getElementById('ordersContainer');
        container = document.createElement('div');
        container.id = 'paginationContainer';
        container.className = 'mt-6';
        ordersContainer.after(container);
    }

    if (totalPages <= 1) {
        container.innerHTML = '';
        return;
    }

    container.innerHTML = `
        <div class="flex items-center justify-between">
            <div class="text-sm text-muted-light dark:text-muted-dark">
                Mostrando ${(currentPage - 1) * pageSize + 1} - ${Math.min(currentPage * pageSize, totalOrders)} de ${totalOrders} pedidos
            </div>
            <div class="flex gap-2">
                <button onclick="changePage(${currentPage - 1})" 
                        ${currentPage === 1 ? 'disabled' : ''}
                        class="px-4 py-2 bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark rounded-lg disabled:opacity-50 hover:bg-background-light dark:hover:bg-background-dark disabled:cursor-not-allowed">
                    Anterior
                </button>
                <div class="flex items-center px-4 py-2 bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark rounded-lg">
                    <span class="font-medium">${currentPage}</span>
                    <span class="mx-2 text-muted-light dark:text-muted-dark">de</span>
                    <span class="font-medium">${totalPages}</span>
                </div>
                <button onclick="changePage(${currentPage + 1})" 
                        ${currentPage === totalPages ? 'disabled' : ''}
                        class="px-4 py-2 bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark rounded-lg disabled:opacity-50 hover:bg-background-light dark:hover:bg-background-dark disabled:cursor-not-allowed">
                    Siguiente
                </button>
            </div>
        </div>
    `;
}

// ✅ NEW: Page change handler
async function changePage(newPage) {
    if (newPage < 1 || newPage > totalPages) return;
    currentPage = newPage;
    await loadOrders();
    // Scroll to top of orders
    document.getElementById('ordersContainer').scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function toggleOrder(index) {
    const details = document.getElementById(`details-${index}`);
    const arrow = document.getElementById(`arrow-${index}`);

    details.classList.toggle('hidden');
    arrow.classList.toggle('rotate-180');
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('es-ES', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function formatAddress(address) {
    if (!address) return 'No especificada';
    const parts = [];
    if (address.calle) parts.push(address.calle);
    if (address.numeroExterior) parts.push(`#${address.numeroExterior}`);
    if (address.ciudad) parts.push(address.ciudad);
    if (address.estado) parts.push(address.estado);
    if (address.codigoPostal) parts.push(address.codigoPostal);
    return parts.join(', ') || 'No especificada';
}

async function reorder(orderId) {
    const order = orders.find(o => o.idOrden === orderId);
    if (!order || !order.items || order.items.length === 0) {
        ErrorHandler.showToast('No se pudieron cargar los productos del pedido', 'error');
        return;
    }

    try {
        for (const item of order.items) {
            const response = await API.cart.addItem({
                productoId: item.productoId,
                cantidad: item.cantidad
            });

            if (!response.success) {
                ErrorHandler.handleApiError(response);
                return;
            }
        }

        ErrorHandler.showToast('Productos agregados al carrito', 'success');
        Components.updateCartBadge();
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function returnOrder(orderId) {
    ErrorHandler.showToast('Función de devolución próximamente', 'info');
}

function openMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
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
                <button onclick="closeMobileProfileMenu()" class="p-2 hover:bg-gray-100 dark:hover:bg-gray-700/50 rounded-lg"><span class="material-symbols-outlined">close</span></button>
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
                    ${menuItems.map(item => `
                        <a href="${item.href}" class="flex items-center space-x-3 px-4 py-2.5 rounded-md ${window.location.pathname.includes('orders') ? 'bg-primary text-white' : 'hover:bg-gray-100 dark:hover:bg-gray-700/50 text-muted-light dark:text-muted-dark'} text-sm font-medium">
                            <span class="material-symbols-outlined text-base">${item.icon}</span><span>${item.label}</span>
                        </a>
                    `).join('')}
                </nav>
                <div class="border-t border-border-light dark:border-border-dark mt-6 pt-4">
                    <button onclick="logout()" class="flex items-center space-x-3 px-4 py-2.5 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-danger text-sm font-medium w-full">
                        <span class="material-symbols-outlined text-base">logout</span><span>Cerrar Sesión</span>
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

initOrdersPage();