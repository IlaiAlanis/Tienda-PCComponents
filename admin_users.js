let users = [];
let currentPage = 1;
let totalPages = 1;
let filters = { search: '', role: '', status: '', page: 1, pageSize: 10 };

async function initAdminUsersPage() {
    if (!Auth.requireAdmin()) return;
    await loadAdminProfile();
    await loadUsers();
    setupEventListeners();
    restoreSidebarState();
}

async function loadAdminProfile() {
    try {
        const user = Auth.getUser();
        if (user) {
            const fullName = `${user.nombre} ${user.apellido || ''}`.trim();
            document.getElementById('adminName').textContent = fullName || 'Administrador';
            document.getElementById('adminEmail').textContent = user.correo;
            const avatarUrl = user.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'Admin')}&background=007BFF&color=fff`;
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
        loadUsers();
    }, 500));

    document.getElementById('roleFilter').addEventListener('change', (e) => {
        filters.role = e.target.value;
        filters.page = 1;
        loadUsers();
    });

    document.getElementById('statusFilter').addEventListener('change', (e) => {
        filters.status = e.target.value;
        filters.page = 1;
        loadUsers();
    });
}

async function loadUsers() {
    const tbody = document.getElementById('usersTableBody');
    tbody.innerHTML = '<tr><td colspan="6" class="p-8 text-center"><div class="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div></td></tr>';

    try {
        const response = await API.admin.getUsers(filters);

        if (response.success && response.data) {
            users = response.data.items || response.data;
            totalPages = response.data.totalPages || 1;
            currentPage = filters.page;

            if (users.length === 0) {
                tbody.innerHTML = '<tr><td colspan="6" class="p-8 text-center text-text-secondary-light dark:text-text-secondary-dark"><span class="material-icons-outlined text-4xl mb-2 opacity-50">group</span><p>No se encontraron usuarios</p></td></tr>';
            } else {
                tbody.innerHTML = users.map(user => createUserRow(user)).join('');
            }

            updatePagination(response.data.total || users.length);
        } else {
            ErrorHandler.handleApiError(response);
            tbody.innerHTML = '<tr><td colspan="6" class="p-8 text-center text-red-500">Error al cargar usuarios</td></tr>';
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        tbody.innerHTML = '<tr><td colspan="6" class="p-8 text-center text-red-500">Error de conexión</td></tr>';
    }
}

function createUserRow(user) {
    const roleColors = {
        'ADMIN': 'bg-warning/20 text-yellow-800 dark:text-warning',
        'CLIENTE': 'bg-primary/20 text-blue-800 dark:text-primary',
    };

    const roleLabels = {
        'ADMIN': 'Administrador',
        'CLIENTE': 'Cliente',
    };

    const fullName = `${user.nombre || ''} ${user.apellido || ''}`.trim() || user.nombreUsuario;
    const statusIcon = user.estaActivo ? 'check_circle' : 'cancel';
    const statusColor = user.estaActivo ? 'text-green-500' : 'text-red-500';

    return `
        <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50">
            <td class="p-3">
                <div class="flex items-center gap-3">
                    <img src="https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff" 
                         alt="${fullName}" 
                         class="w-8 h-8 rounded-full"/>
                    <span class="font-medium">${fullName}</span>
                </div>
            </td>
            <td class="p-3">${user.correo}</td>
            <td class="p-3">
                <span class="px-2 py-1 text-xs font-semibold rounded-full ${roleColors[user.rol] || roleColors['CLIENTE']}">
                    ${roleLabels[user.rol] || user.rol}
                </span>
            </td>
            <td class="p-3 text-sm text-text-secondary-light dark:text-text-secondary-dark">
                ${formatDate(user.fechaCreacion)}
            </td>
            <td class="p-3">
                <span class="material-icons-outlined ${statusColor}">${statusIcon}</span>
            </td>
            <td class="p-3">
                <div class="flex items-center gap-1">
                    <button onclick="viewUserDetails(${user.idUsuario})" 
                            class="p-2 text-primary hover:bg-primary/10 rounded" 
                            title="Ver detalles">
                        <span class="material-icons-outlined text-base">visibility</span>
                    </button>
                    <button onclick="toggleUserStatus(${user.idUsuario})" 
                            class="p-2 text-warning hover:bg-warning/10 rounded" 
                            title="${user.estaActivo ? 'Desactivar' : 'Activar'}">
                        <span class="material-icons-outlined text-base">
                            ${user.estaActivo ? 'block' : 'check_circle'}
                        </span>
                    </button>
                </div>
            </td>
        </tr>
    `;
}

async function viewUserDetails(userId) {
    const modal = document.getElementById('userDetailsModal');
    const content = document.getElementById('userDetailsContent');

    modal.classList.remove('hidden');
    content.innerHTML = '<div class="flex justify-center py-8"><div class="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div></div>';

    try {
        const response = await API.admin.getUserDetails(userId);

        if (response.success && response.data) {
            const user = response.data;
            const fullName = `${user.nombre || ''} ${user.apellido || ''}`.trim() || user.nombreUsuario;

            content.innerHTML = `
                <div class="space-y-6">
                    <div class="flex items-center gap-4">
                        <img src="https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff" 
                             alt="${fullName}" 
                             class="w-20 h-20 rounded-full"/>
                        <div>
                            <h3 class="text-2xl font-bold">${fullName}</h3>
                            <p class="text-text-secondary-light dark:text-text-secondary-dark">${user.correo}</p>
                        </div>
                    </div>
                    
                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark mb-1">Rol</p>
                            <p class="font-semibold">${user.rol}</p>
                        </div>
                        <div>
                            <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark mb-1">Estado</p>
                            <p class="font-semibold ${user.estaActivo ? 'text-green-500' : 'text-red-500'}">
                                ${user.estaActivo ? 'Activo' : 'Inactivo'}
                            </p>
                        </div>
                        <div>
                            <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark mb-1">Fecha de Registro</p>
                            <p class="font-semibold">${formatDate(user.fechaCreacion)}</p>
                        </div>
                        <div>
                            <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark mb-1">Total de Pedidos</p>
                            <p class="font-semibold">${user.totalPedidos || 0}</p>
                        </div>
                    </div>
                </div>
            `;
        } else {
            ErrorHandler.handleApiError(response);
            content.innerHTML = '<p class="text-center text-red-500 py-8">Error al cargar detalles del usuario</p>';
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        content.innerHTML = '<p class="text-center text-red-500 py-8">Error de conexión</p>';
    }
}

function closeUserDetailsModal() {
    document.getElementById('userDetailsModal').classList.add('hidden');
}

async function toggleUserStatus(userId) {
    if (!confirm('¿Cambiar el estado de este usuario?')) return;

    try {
        const response = await API.admin.toggleUserStatus(userId);

        if (response.success) {
            ErrorHandler.showToast('Estado del usuario actualizado', 'success');
            await loadUsers();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function updatePagination(total) {
    const paginationInfo = document.getElementById('paginationInfo');
    if (paginationInfo) {
        const start = (currentPage - 1) * filters.pageSize + 1;
        const end = Math.min(currentPage * filters.pageSize, total);
        paginationInfo.textContent = `Mostrando ${start}-${end} de ${total}`;
    }

    const buttons = [];
    buttons.push(`<button onclick="changePage(${currentPage - 1})" ${currentPage === 1 ? 'disabled' : ''} class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed">Anterior</button>`);

    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
            buttons.push(`<button onclick="changePage(${i})" class="px-3 py-2 text-sm rounded ${i === currentPage ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500'}">${i}</button>`);
        } else if (i === currentPage - 2 || i === currentPage + 2) {
            buttons.push('<span class="px-2 text-sm">...</span>');
        }
    }

    buttons.push(`<button onclick="changePage(${currentPage + 1})" ${currentPage === totalPages ? 'disabled' : ''} class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed">Siguiente</button>`);

    const container = document.getElementById('pagination');
    if (container) {
        container.innerHTML = buttons.join('');
    }
}

function changePage(page) {
    if (page < 1 || page > totalPages) return;
    filters.page = page;
    loadUsers();
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('roleFilter').value = '';
    document.getElementById('statusFilter').value = '';
    filters = { search: '', role: '', status: '', page: 1, pageSize: 10 };
    loadUsers();
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('es-MX', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
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
        window.location.href = '../pages/index.html';
    }
}

document.addEventListener('DOMContentLoaded', initAdminUsersPage);