let orders = [];
let currentPage = 1;
let totalPages = 1;
let totalItems = 0;
let filters = { search: '', status: '', page: 1, pageSize: 10 };
let currentOrder = null;

async function initAdminOrdersPage() {
    if (!Auth.requireAdmin()) return;
    await loadAdminProfile();
    await loadOrders();
    setupEventListeners();
    restoreSidebarState();
}

async function loadAdminProfile() {
    try {
        const user = Auth.getUser();
        if (user) {
            const fullName = [
                capitalizeName(user?.nombre),
                capitalizeName(user?.apellidoPaterno),
                capitalizeName(user?.apellidoMaterno)
            ].filter(Boolean).join(" ");

            document.getElementById('adminName').textContent = fullName || 'Administrador';
            document.getElementById('adminEmail').textContent = user?.correo || 'admin@example.com';
            
            const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'Admin')}&background=007BFF&color=fff`;
            document.getElementById('adminAvatar').src = avatarUrl;
        }
    } catch (error) {
        console.error('Error loading admin profile:', error);
    }
}

function setupEventListeners() {
    document.getElementById('searchInput').addEventListener('input', debounce((e) => {
        filters.search = e.target.value;
        filters.page = 1;
        loadOrders();
    }, 500));

    document.getElementById('statusFilter').addEventListener('change', (e) => {
        filters.status = e.target.value;
        filters.page = 1;
        loadOrders();
    });
}

async function loadOrders() {
    const tbody = document.getElementById('ordersTableBody');
    tbody.innerHTML = '<tr><td colspan="6" class="px-6 py-12 text-center"><div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></td></tr>';

    try {
        // ✅ FIXED: Use admin endpoint with server-side filtering
        const response = await API.admin.getAllOrders(filters);

        if (response.success && response.data) {
            const result = response.data;

            // Backend returns PagedResult<OrdenDto>
            orders = result.items || [];
            totalPages = result.totalPages || 1;
            totalItems = result.totalCount || 0;
            currentPage = result.page || filters.page;

            if (orders.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="px-6 py-12 text-center text-text-secondary-light dark:text-text-secondary-dark"><span class="material-icons-outlined text-4xl mb-2 opacity-50">shopping_cart</span><p>No se encontraron pedidos</p></td></tr>';
            } else {
                tbody.innerHTML = orders.map(order => createOrderRow(order)).join('');
            }

            updatePagination();
        } else {
            ErrorHandler.handleApiError(response);
            tbody.innerHTML = '<tr><td colspan="6" class="px-6 py-12 text-center text-red-500">Error al cargar pedidos</td></tr>';
        }
    } catch (error) {
        console.error('Error loading orders:', error);
        ErrorHandler.showToast('Error al cargar pedidos', 'error');
        tbody.innerHTML = '<tr><td colspan="6" class="px-6 py-12 text-center text-red-500">Error de conexión</td></tr>';
    }
}

function createOrderRow(order) {
    const statusColors = {
        'pendiente': 'bg-orange-500/20 text-orange-800 dark:text-orange-400',
        'procesando': 'bg-blue-500/20 text-blue-800 dark:text-blue-400',
        'enviado': 'bg-purple-500/20 text-purple-800 dark:text-purple-400',
        'entregado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'completado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'cancelado': 'bg-red-500/20 text-red-800 dark:text-red-400',
    };

    const statusLabels = {
        'pendiente': 'Pendiente',
        'procesando': 'Procesando',
        'enviado': 'Enviado',
        'entregado': 'Entregado',
        'completado': 'Completado',
        'cancelado': 'Cancelado',
    };

    const status = (order.estado || '').toLowerCase();
    const statusColor = statusColors[status] || statusColors['pendiente'];
    const statusLabel = statusLabels[status] || order.estado;

    return `
        <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50">
            <td class="px-6 py-4">
                <span class="font-semibold text-primary cursor-pointer" onclick="viewOrderDetails(${order.idOrden})">#${order.idOrden}</span>
            </td>
            <td class="px-6 py-4">
                <div>
                    <p class="font-medium">${order.nombreCliente || 'Cliente'}</p>
                    <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark">${order.emailCliente || ''}</p>
                </div>
            </td>
            <td class="px-6 py-4 text-sm text-text-secondary-light dark:text-text-secondary-dark">
                ${formatDate(order.fechaPedido || order.fechaOrden || order.fechaCreacion)}
            </td>
            <td class="px-6 py-4">
                <span class="font-semibold">${formatCurrency(order.total || 0)}</span>
            </td>
            <td class="px-6 py-4">
                <span class="px-2 py-1 text-xs font-semibold rounded-full ${statusColor}">
                    ${statusLabel}
                </span>
            </td>
            <td class="px-6 py-4">
                <div class="flex items-center gap-1 justify-end">
                    <button onclick="viewOrderDetails(${order.idOrden})" class="p-2 text-primary hover:bg-primary/10 rounded" title="Ver detalles">
                        <span class="material-icons-outlined text-base">visibility</span>
                    </button>
                    ${status !== 'cancelado' && status !== 'entregado' && status !== 'completado' ? `
                        <button onclick="updateOrderStatus(${order.idOrden})" class="p-2 text-warning hover:bg-warning/10 rounded" title="Actualizar estado">
                            <span class="material-icons-outlined text-base">edit</span>
                        </button>
                    ` : ''}
                </div>
            </td>
        </tr>
    `;
}

async function viewOrderDetails(orderId) {
    const modal = document.getElementById('orderDetailsModal');
    const content = document.getElementById('orderDetailsContent');
    
    modal.classList.remove('hidden');
    content.innerHTML = '<div class="flex justify-center py-8"><div class="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div></div>';

    try {
        // ✅ FIXED: Use admin endpoint to get any order
        const response = await API.admin.getOrderById(orderId);

        if (response.success && response.data) {
            currentOrder = response.data;
            renderOrderDetails(currentOrder);
        } else {
            ErrorHandler.handleApiError(response);
            content.innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar detalles del pedido</p>';
        }
    } catch (error) {
        console.error('Error loading order details:', error);
        ErrorHandler.showToast('Error al cargar detalles', 'error');
        content.innerHTML = '<p class="text-center text-red-500 py-8">Error de conexión</p>';
    }
}

function renderOrderDetails(order) {
    const content = document.getElementById('orderDetailsContent');
    
    const statusColors = {
        'pendiente': 'bg-orange-500/20 text-orange-800 dark:text-orange-400',
        'procesando': 'bg-blue-500/20 text-blue-800 dark:text-blue-400',
        'enviado': 'bg-purple-500/20 text-purple-800 dark:text-purple-400',
        'entregado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'completado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'cancelado': 'bg-red-500/20 text-red-800 dark:text-red-400',
    };

    const status = (order.estado || '').toLowerCase();
    const statusColor = statusColors[status] || statusColors['pendiente'];

    const itemsHTML = (order.items || []).map(item => `
        <div class="flex items-center gap-4 py-3 border-b border-border-light dark:border-border-dark last:border-b-0">
            <img src="${item.imagenUrl || item.producto?.imagenes?.[0] || 'https://via.placeholder.com/60x60?text=P'}" 
                 alt="${item.nombreProducto}" 
                 class="w-16 h-16 object-cover rounded"/>
            <div class="flex-1">
                <p class="font-semibold">${item.nombreProducto}</p>
                <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark">
                    Cantidad: ${item.cantidad} × ${formatCurrency(item.precioUnitario)}
                </p>
            </div>
            <p class="font-semibold">${formatCurrency(item.cantidad * item.precioUnitario)}</p>
        </div>
    `).join('');

    content.innerHTML = `
        <div class="space-y-6">
            <!-- Order Header -->
            <div class="flex items-center justify-between pb-4 border-b border-border-light dark:border-border-dark">
                <div>
                    <h3 class="text-2xl font-bold">Pedido #${order.idOrden}</h3>
                    <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark">
                        ${formatDate(order.fechaPedido || order.fechaOrden || order.fechaCreacion)}
                    </p>
                </div>
                <span class="px-3 py-1 text-sm font-semibold rounded-full ${statusColor}">
                    ${order.estado}
                </span>
            </div>

            <!-- Customer Info -->
            <div>
                <h4 class="font-semibold mb-3">Información del Cliente</h4>
                <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-4 space-y-2">
                    <p><span class="font-medium">Nombre:</span> ${order.nombreCliente || 'N/A'}</p>
                    <p><span class="font-medium">Email:</span> ${order.emailCliente || 'N/A'}</p>
                    <p><span class="font-medium">Teléfono:</span> ${order.telefonoCliente || 'N/A'}</p>
                </div>
            </div>

            <!-- Shipping Address -->
            ${order.direccionEnvio ? `
                <div>
                    <h4 class="font-semibold mb-3">Dirección de Envío</h4>
                    <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-4">
                        <p>${order.direccionEnvio.calle} ${order.direccionEnvio.numeroExterior || ''}</p>
                        ${order.direccionEnvio.numeroInterior ? `<p>Interior: ${order.direccionEnvio.numeroInterior}</p>` : ''}
                        ${order.direccionEnvio.colonia ? `<p>${order.direccionEnvio.colonia}</p>` : ''}
                        <p>${order.direccionEnvio.ciudad || ''}, ${order.direccionEnvio.estado || ''} ${order.direccionEnvio.codigoPostal || ''}</p>
                    </div>
                </div>
            ` : ''}

            <!-- Order Items -->
            <div>
                <h4 class="font-semibold mb-3">Productos</h4>
                <div class="border border-border-light dark:border-border-dark rounded-lg overflow-hidden">
                    ${itemsHTML}
                </div>
            </div>

            <!-- Order Summary -->
            <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-4 space-y-2">
                <div class="flex justify-between text-sm">
                    <span>Subtotal</span>
                    <span>${formatCurrency(order.subtotal || 0)}</span>
                </div>
                ${order.descuento || order.descuentoTotal ? `
                    <div class="flex justify-between text-sm text-green-600">
                        <span>Descuento</span>
                        <span>-${formatCurrency(order.descuento || order.descuentoTotal || 0)}</span>
                    </div>
                ` : ''}
                <div class="flex justify-between text-sm">
                    <span>Envío</span>
                    <span>${formatCurrency(order.costoEnvio || order.envioTotal || 0)}</span>
                </div>
                <div class="flex justify-between text-sm">
                    <span>Impuestos</span>
                    <span>${formatCurrency(order.impuestos || order.impuestoTotal || 0)}</span>
                </div>
                <div class="flex justify-between text-lg font-bold pt-2 border-t border-border-light dark:border-border-dark">
                    <span>Total</span>
                    <span>${formatCurrency(order.total || 0)}</span>
                </div>
            </div>

            <!-- Status Update -->
            ${status !== 'cancelado' && status !== 'entregado' && status !== 'completado' ? `
                <div class="flex gap-2">
                    <select id="newStatus" class="flex-1 px-3 py-2 text-sm rounded border border-border-light dark:border-border-dark bg-card-light dark:bg-background-dark">
                        <option value="pendiente" ${status === 'pendiente' ? 'selected' : ''}>Pendiente</option>
                        <option value="procesando" ${status === 'procesando' ? 'selected' : ''}>Procesando</option>
                        <option value="enviado" ${status === 'enviado' ? 'selected' : ''}>Enviado</option>
                        <option value="entregado" ${status === 'entregado' ? 'selected' : ''}>Entregado</option>
                        <option value="completado" ${status === 'completado' ? 'selected' : ''}>Completado</option>
                        <option value="cancelado" ${status === 'cancelado' ? 'selected' : ''}>Cancelado</option>
                    </select>
                    <button onclick="saveOrderStatus()" class="px-6 py-2 bg-primary text-white rounded-lg hover:bg-blue-600 font-medium">
                        Actualizar Estado
                    </button>
                </div>
            ` : ''}
        </div>
    `;
}

async function updateOrderStatus(orderId) {
    await viewOrderDetails(orderId);
}

async function saveOrderStatus() {
    if (!currentOrder) return;

    const newStatus = document.getElementById('newStatus').value;
    
    if (!newStatus) {
        ErrorHandler.showToast('Selecciona un estado', 'warning');
        return;
    }

    try {
        // ✅ FIXED: Use correct endpoint and payload format
        const response = await API.admin.updateOrderStatus(currentOrder.idOrden, { estado: newStatus });

        if (response.success) {
            ErrorHandler.showToast('Estado actualizado correctamente', 'success');
            closeOrderDetailsModal();
            await loadOrders();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error updating order status:', error);
        ErrorHandler.showToast('Error al actualizar estado', 'error');
    }
}

function closeOrderDetailsModal() {
    document.getElementById('orderDetailsModal').classList.add('hidden');
    currentOrder = null;
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
}

function formatDate(dateString) {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('es-MX', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function updatePagination() {
    const paginationInfo = document.getElementById('paginationInfo');
    const buttons = [];
    
    // Update info text
    const start = totalItems === 0 ? 0 : (currentPage - 1) * filters.pageSize + 1;
    const end = Math.min(currentPage * filters.pageSize, totalItems);
    
    if (paginationInfo) {
        paginationInfo.textContent = `Mostrando ${start}-${end} de ${totalItems}`;
    }
    
    // Previous button
    buttons.push(`<button onclick="changePage(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''} class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed">Anterior</button>`);
    
    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
            buttons.push(`<button onclick="changePage(${i})" class="px-3 py-2 text-sm rounded ${i === currentPage ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500'}">${i}</button>`);
        } else if (i === currentPage - 2 || i === currentPage + 2) {
            buttons.push('<span class="px-2 text-sm">...</span>');
        }
    }
    
    // Next button
    buttons.push(`<button onclick="changePage(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''} class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed">Siguiente</button>`);
    
    const container = document.getElementById('pagination');
    if (container) {
        container.innerHTML = buttons.join('');
    }
}

function changePage(page) {
    if (page < 1 || page > totalPages) return;
    filters.page = page;
    loadOrders();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = '';
    filters = { search: '', status: '', page: 1, pageSize: 10 };
    loadOrders();
}

function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    if (window.innerWidth < 1024) {
        sidebar.classList.toggle('mobile-open');
        overlay.classList.toggle('hidden');
    } else {
        sidebar.classList.toggle('collapsed');
        localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
    }
}

function toggleUserDropdown() {
    const dropdown = document.querySelector('.user-dropdown');
    dropdown.classList.toggle('active');
}

// Close dropdown when clicking outside
document.addEventListener('click', function (e) {
    const dropdown = document.querySelector('.user-dropdown');
    if (dropdown && !dropdown.contains(e.target)) {
        dropdown.classList.remove('active');
    }
});


function restoreSidebarState() {
    if (window.innerWidth >= 1024 && localStorage.getItem('sidebarCollapsed') === 'true') {
        document.getElementById('sidebar').classList.add('collapsed');
    }
}

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) {
        document.getElementById('sidebar').classList.remove('mobile-open');
        document.getElementById('sidebarOverlay').classList.add('hidden');
    }
});

function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

function logout() {
    if (confirm('¿Cerrar sesión?')) {
        Auth.logout();
    }
}

document.addEventListener('DOMContentLoaded', initAdminOrdersPage);