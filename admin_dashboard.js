let dashboardData = null;

/**
 * Initialize dashboard
 */
async function initDashboard() {
    // Check admin authentication
    if (!Auth.requireAdmin()) {
        return;
    }

    // Load admin profile
    await loadAdminProfile();

    // Load dashboard data
    await loadDashboardMetrics();

    // Setup event listeners
    setupEventListeners();

    // Restore sidebar state
    restoreSidebarState();
}

/**
 * Load admin profile info
 */
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

            // Set avatar
            const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'Admin')}&background=007BFF&color=fff`;
            document.getElementById('adminAvatar').src = avatarUrl;
        }
    } catch (error) {
        console.error('Error loading admin profile:', error);
    }
}

/**
 * Load dashboard metrics from API
 */
async function loadDashboardMetrics() {
    try {
        const response = await API.admin.getDashboardMetrics();

        if (response.success && response.data) {
            dashboardData = response.data;
            renderStatCards(dashboardData);
            renderSalesChart(dashboardData.salesData || []);
            renderLowStockAlerts(dashboardData.lowStockProductsList || []);
            renderRecentOrders(dashboardData.recentOrders || []);
            renderTopProducts(dashboardData.topProducts || []);
        } else {
            ErrorHandler.handleApiError(response);
            showErrorState();
        }
    } catch (error) {
        console.error('Error loading dashboard metrics:', error);
        ErrorHandler.showToast('Error al cargar métricas del dashboard', 'error');
        showErrorState();
    }
}

/**
 * Render stat cards
 */
function renderStatCards(data) {
    const stats = [
        {
            title: 'Pedidos Pendientes',
            value: data.pendingOrders || 0,
            icon: 'pending_actions',
            color: 'bg-orange-500',
            textColor: 'text-orange-500',
        },
        {
            title: 'Stock Bajo',
            value: data.lowStockProducts || 0,
            icon: 'inventory',
            color: 'bg-red-500',
            textColor: 'text-red-500',
        },
        {
            title: 'Nuevos Usuarios (30d)',
            value: data.newUsersCount || 0,
            icon: 'person_add',
            color: 'bg-blue-500',
            textColor: 'text-blue-500',
        },
        {
            title: 'Ventas del Mes',
            value: formatCurrency(data.monthSales || 0),
            icon: 'attach_money',
            color: 'bg-green-500',
            textColor: 'text-green-500',
        },
    ];

    const statsHTML = stats.map(stat => createStatCard(stat)).join('');
    document.getElementById('statsGrid').innerHTML = statsHTML;
}

/**
 * Create stat card HTML
 */
function createStatCard(stat) {
    return `
        <div class="stat-card bg-card-light dark:bg-card-dark rounded-lg shadow-md p-6 border border-border-light dark:border-border-dark">
            <div class="flex items-center justify-between mb-4">
                <div class="${stat.color} p-3 rounded-lg">
                    <span class="material-icons-outlined text-white text-2xl">${stat.icon}</span>
                </div>
            </div>
            <p class="text-text-secondary-light dark:text-text-secondary-dark text-sm font-medium mb-1">
                ${stat.title}
            </p>
            <p class="text-3xl font-bold ${stat.textColor}">${stat.value}</p>
        </div>
    `;
}

/**
 * Render sales chart (simple bar chart)
 */
function renderSalesChart(salesData) {
    const container = document.getElementById('salesChart');

    if (!salesData || salesData.length === 0) {
        // Generate mock data for demonstration
        salesData = [
            { label: 'Semana 1', value: 3200 },
            { label: 'Semana 2', value: 2800 },
            { label: 'Semana 3', value: 4500 },
            { label: 'Semana 4', value: 5200 },
        ];
    }

    // Calculate max value for scaling
    const maxValue = Math.max(...salesData.map(d => d.value));

    // Calculate total and percentage change
    const total = salesData.reduce((sum, d) => sum + d.value, 0);
    const avgPrevious = total / salesData.length * 0.95; // Mock previous period
    const percentageChange = ((total - avgPrevious) / avgPrevious * 100).toFixed(1);

    const chartHTML = `
        <div class="mb-4">
            <div class="flex items-baseline gap-2">
                <p class="text-3xl font-bold">${formatCurrency(total)}</p>
                <p class="${percentageChange >= 0 ? 'text-success' : 'text-danger'} text-sm font-medium">
                    ${percentageChange >= 0 ? '+' : ''}${percentageChange}%
                </p>
            </div>
        </div>
        <div class="h-48 grid grid-flow-col gap-4 grid-rows-[1fr_auto] items-end justify-items-center">
            ${salesData.map((item, index) => {
        const height = (item.value / maxValue * 100);
        const isLast = index === salesData.length - 1;
        return `
                    <div class="${isLast ? 'bg-primary' : 'bg-primary/20'} w-full rounded-t-md transition-all hover:bg-primary" 
                         style="height: ${height}%;" 
                         title="${item.label}: ${formatCurrency(item.value)}"></div>
                    <p class="${isLast ? 'text-primary font-bold' : 'text-text-secondary-light dark:text-text-secondary-dark'} text-xs">
                        ${item.label}
                    </p>
                `;
    }).join('')}
        </div>
    `;

    container.innerHTML = chartHTML;
}

/**
 * Render low stock alerts
 */
function renderLowStockAlerts(products) {
    const container = document.getElementById('lowStockAlerts');

    if (!products || products.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8 text-text-secondary-light dark:text-text-secondary-dark">
                <span class="material-icons-outlined text-4xl mb-2 opacity-50">check_circle</span>
                <p class="text-sm">No hay alertas de stock</p>
            </div>
        `;
        return;
    }

    const alertsHTML = products.slice(0, 5).map(product => `
        <div class="flex justify-between items-center py-3 border-b border-border-light dark:border-border-dark last:border-b-0">
            <div class="flex-1">
                <p class="font-medium text-sm truncate">${product.nombreProducto}</p>
                <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark">
                    Solo quedan ${product.stock || 0} unidades
                </p>
            </div>
            <span class="${product.stock <= 3 ? 'text-red-500' : 'text-orange-500'} font-bold text-lg">
                ${product.stock || 0}
            </span>
        </div>
    `).join('');

    container.innerHTML = alertsHTML;
}

/**
 * Render recent orders
 */
function renderRecentOrders(orders) {
    const container = document.getElementById('recentOrders');

    if (!orders || orders.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8 text-text-secondary-light dark:text-text-secondary-dark">
                <span class="material-icons-outlined text-4xl mb-2 opacity-50">inbox</span>
                <p class="text-sm">No hay pedidos recientes</p>
            </div>
        `;
        return;
    }

    const ordersHTML = orders.slice(0, 5).map(order => `
        <div class="flex items-center justify-between p-3 border-b border-border-light dark:border-border-dark last:border-b-0 hover:bg-gray-50 dark:hover:bg-gray-800/50 rounded transition-colors">
            <div class="flex-1">
                <p class="font-semibold text-sm text-primary cursor-pointer" onclick="viewOrder(${order.idOrden})">#${order.idOrden}</p>
                <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark">
                    ${order.nombreCliente || 'Cliente'} • ${formatDate(order.fechaCreacion)}
                </p>
            </div>
            <div class="text-right">
                <p class="font-semibold text-sm mb-1">${formatCurrency(order.total || 0)}</p>
                <span class="text-xs px-2 py-1 rounded-full ${getOrderStatusColor(order.estado)}">
                    ${getOrderStatusLabel(order.estado)}
                </span>
            </div>
        </div>
    `).join('');

    container.innerHTML = ordersHTML;
}

/**
 * Render top products
 */
function renderTopProducts(products) {
    const container = document.getElementById('topProducts');

    if (!products || products.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8 text-text-secondary-light dark:text-text-secondary-dark">
                <span class="material-icons-outlined text-4xl mb-2 opacity-50">inventory_2</span>
                <p class="text-sm">No hay datos de productos</p>
            </div>
        `;
        return;
    }

    const productsHTML = products.slice(0, 5).map((product, index) => `
        <div class="flex items-center gap-3 p-3 border-b border-border-light dark:border-border-dark last:border-b-0 hover:bg-gray-50 dark:hover:bg-gray-800/50 rounded transition-colors">
            <span class="text-lg font-bold text-text-secondary-light dark:text-text-secondary-dark w-6">
                ${index + 1}
            </span>
            <div class="flex-1">
                <p class="font-semibold text-sm truncate">${product.nombreProducto}</p>
                <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark">
                    ${product.cantidadVendida || 0} vendidos
                </p>
            </div>
            <div class="text-right">
                <p class="font-semibold text-sm">${formatCurrency(product.precioBase || 0)}</p>
            </div>
        </div>
    `).join('');

    container.innerHTML = productsHTML;
}

/**
 * Get order status color classes
 */
function getOrderStatusColor(status) {
    const colors = {
        'Pendiente': 'bg-orange-500/20 text-orange-800 dark:text-orange-400',
        'Procesando': 'bg-blue-500/20 text-blue-800 dark:text-blue-400',
        'Enviado': 'bg-purple-500/20 text-purple-800 dark:text-purple-400',
        'Entregado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'Completado': 'bg-green-500/20 text-green-800 dark:text-green-400',
        'Cancelado': 'bg-red-500/20 text-red-800 dark:text-red-400',
    };
    return colors[status] || 'bg-gray-500/20 text-gray-800 dark:text-gray-400';
}

/**
 * Get order status label
 */
function getOrderStatusLabel(status) {
    return status || 'Desconocido';
}

/**
 * Format currency
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN'
    }).format(amount);
}

/**
 * Format date
 */
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('es-MX', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

/**
 * View order details
 */
function viewOrder(orderId) {
    window.location.href = `../pages/admin/admin_orders.html?id=${orderId}`;
}

/**
 * Show error state
 */
function showErrorState() {
    document.getElementById('statsGrid').innerHTML = `
        <div class="col-span-full text-center py-12 text-red-500">
            <span class="material-icons-outlined text-5xl mb-4">error_outline</span>
            <p>Error al cargar métricas del dashboard</p>
            <button onclick="loadDashboardMetrics()" class="mt-4 px-4 py-2 bg-primary text-white rounded-lg hover:bg-blue-600">
                Reintentar
            </button>
        </div>
    `;

    document.getElementById('salesChart').innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar gráfico</p>';
    document.getElementById('lowStockAlerts').innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar alertas</p>';
    document.getElementById('recentOrders').innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar pedidos</p>';
    document.getElementById('topProducts').innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar productos</p>';
}

/**
 * Refresh dashboard
 */
async function refreshDashboard() {
    ErrorHandler.showToast('Actualizando dashboard...', 'info');
    await loadDashboardMetrics();
}

/**
 * Setup event listeners
 */
function setupEventListeners() {
    // Handle window resize
    window.addEventListener('resize', () => {
        if (window.innerWidth >= 1024) {
            document.getElementById('sidebar').classList.remove('mobile-open');
            document.getElementById('sidebarOverlay').classList.add('hidden');
        }
    });
}

/**
 * Toggle sidebar (mobile/desktop)
 */
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (window.innerWidth < 1024) {
        // Mobile: toggle overlay menu
        sidebar.classList.toggle('mobile-open');
        overlay.classList.toggle('hidden');
    } else {
        // Desktop: toggle collapsed state
        sidebar.classList.toggle('collapsed');
        localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
    }
}

// Toggle user dropdown
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

/**
 * Restore sidebar state from localStorage
 */
function restoreSidebarState() {
    if (window.innerWidth >= 1024 && localStorage.getItem('sidebarCollapsed') === 'true') {
        document.getElementById('sidebar').classList.add('collapsed');
    }
}

/**
 * Logout
 */
function logout() {
    if (confirm('¿Estás seguro de que deseas cerrar sesión?')) {
        Auth.logout();
    }
}

// Initialize dashboard when DOM is ready
document.addEventListener('DOMContentLoaded', initDashboard);