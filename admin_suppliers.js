let suppliers = [];
let filteredSuppliers = [];
let editingSupplierId = null;

async function initAdminSuppliersPage() {
    if (!Auth.requireAdmin()) return;
    await loadAdminProfile();
    await loadSuppliers();
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
        filterSuppliers(e.target.value);
    }, 300));
    document.getElementById('supplierForm').addEventListener('submit', handleSupplierSubmit);
}

async function loadSuppliers() {
    const grid = document.getElementById('suppliersGrid');
    grid.innerHTML = createLoadingSpinner();

    try {
        const response = await API.suppliers.getAll();

        if (response.success && response.data) {
            suppliers = response.data;
            filteredSuppliers = suppliers;
            renderSuppliers();

            // Show count
            updateSupplierCount();
        } else {
            ErrorHandler.handleApiError(response);
            grid.innerHTML = createErrorState('Error al cargar proveedores');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        grid.innerHTML = createErrorState('Error de conexión');
    }
}

function updateSupplierCount() {
    const countElement = document.getElementById('supplierCount');
    if (countElement) {
        countElement.textContent = `${filteredSuppliers.length} de ${suppliers.length} proveedores`;
    }
}

function filterSuppliers(searchTerm) {
    const term = searchTerm.toLowerCase().trim();

    filteredSuppliers = term
        ? suppliers.filter(s =>
            s.nombreProveedor?.toLowerCase().includes(term) ||
            s.contacto?.toLowerCase().includes(term) ||
            s.email?.toLowerCase().includes(term) ||
            s.telefono?.toLowerCase().includes(term)
        )
        : suppliers;

    renderSuppliers();
    updateSupplierCount();
}

function renderSuppliers() {
    const grid = document.getElementById('suppliersGrid');

    if (filteredSuppliers.length === 0) {
        grid.innerHTML = createEmptyState('No se encontraron proveedores');
        return;
    }

    grid.innerHTML = filteredSuppliers.map(supplier => createSupplierCard(supplier)).join('');
}

function createSupplierCard(supplier) {
    const statusColor = supplier.estaActivo
        ? 'bg-green-200 text-green-800 dark:bg-green-900/40 dark:text-green-400'
        : 'bg-gray-500/20 text-gray-800 dark:text-gray-400';

    // Use correct property names that match backend DTO
    const contacto = supplier.contacto || 'Sin contacto';
    const email = supplier.email || 'Sin email';
    const telefono = supplier.telefono || 'Sin teléfono';

    return `
        <div class="bg-card-light dark:bg-card-dark rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow border border-border-light dark:border-border-dark">
            <div class="flex items-start justify-between mb-4">
                <div class="flex-1">
                    <h3 class="text-lg font-bold mb-1">${supplier.nombreProveedor}</h3>
                    <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark">
                        <span class="material-icons-outlined text-xs align-middle">person</span>
                        ${contacto}
                    </p>
                </div>
                <span class="px-2 py-1 text-xs font-semibold rounded-full ${statusColor}">
                    ${supplier.estaActivo ? 'Activo' : 'Inactivo'}
                </span>
            </div>
            
            <div class="space-y-2 mb-4">
                <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark">
                    <span class="material-icons-outlined text-xs align-middle">phone</span>
                    ${telefono}
                </p>
                <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark break-all">
                    <span class="material-icons-outlined text-xs align-middle">email</span>
                    ${email}
                </p>
                ${supplier.direccion ? `
                    <p class="text-sm text-text-secondary-light dark:text-text-secondary-dark">
                        <span class="material-icons-outlined text-xs align-middle">location_on</span>
                        ${supplier.direccion}
                    </p>
                ` : ''}
            </div>
            
            <div class="flex items-center gap-2 pt-3 border-t border-border-light dark:border-border-dark">
                <button onclick="editSupplier(${supplier.idProveedor})" 
                    class="flex-1 py-2 text-sm font-medium text-warning hover:bg-warning/10 rounded transition-colors">
                    Editar
                </button>
                <button onclick="deleteSupplier(${supplier.idProveedor})" 
                    class="flex-1 py-2 text-sm font-medium text-danger hover:bg-danger/10 rounded transition-colors">
                    Eliminar
                </button>
            </div>
        </div>
    `;
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    filteredSuppliers = suppliers;
    renderSuppliers();
    updateSupplierCount();
}

function openSupplierModal(supplier = null) {
    editingSupplierId = supplier?.idProveedor || null;
    document.getElementById('modalTitle').textContent = supplier ? 'Editar Proveedor' : 'Nuevo Proveedor';

    const form = document.getElementById('supplierForm');
    if (supplier) {
        // Map backend properties to form fields
        form.nombreProveedor.value = supplier.nombreProveedor || '';
        form.contacto.value = supplier.contacto || '';
        form.telefono.value = supplier.telefono || '';
        form.email.value = supplier.email || '';
        form.direccion.value = supplier.direccion || '';
        form.estaActivo.checked = supplier.estaActivo !== false;
    } else {
        form.reset();
        form.estaActivo.checked = true;
    }

    document.getElementById('supplierModal').classList.remove('hidden');
}

function closeSupplierModal() {
    document.getElementById('supplierModal').classList.add('hidden');
    editingSupplierId = null;
}

async function handleSupplierSubmit(e) {
    e.preventDefault();

    const formData = new FormData(e.target);

    // Build request matching backend CreateProveedorRequest DTO
    // Location fields are now optional in backend
    const data = {
        nombreProveedor: formData.get('nombreProveedor')?.trim(),
        contacto: formData.get('contacto')?.trim() || null,
        telefono: formData.get('telefono')?.trim(),
        email: formData.get('email')?.trim(),
        direccion: formData.get('direccion')?.trim() || null,
        estaActivo: formData.get('estaActivo') === 'on'
    };

    // Validate required fields
    if (!data.nombreProveedor) {
        ErrorHandler.showToast('El nombre del proveedor es requerido', 'error');
        return;
    }
    if (!data.telefono) {
        ErrorHandler.showToast('El teléfono es requerido', 'error');
        return;
    }
    if (!data.email) {
        ErrorHandler.showToast('El email es requerido', 'error');
        return;
    }

    const btn = document.getElementById('saveBtn');
    ErrorHandler.setLoading(btn, true);

    try {
        const response = editingSupplierId
            ? await API.suppliers.update(editingSupplierId, data)
            : await API.suppliers.create(data);

        if (response.success) {
            ErrorHandler.showToast(
                editingSupplierId ? 'Proveedor actualizado exitosamente' : 'Proveedor creado exitosamente',
                'success'
            );
            closeSupplierModal();
            await loadSuppliers();
        } else {
            ErrorHandler.handleApiError(response, e.target);
        }
    } catch (error) {
        console.error('Error saving supplier:', error);
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

function editSupplier(id) {
    const supplier = suppliers.find(s => s.idProveedor === id);
    if (supplier) {
        openSupplierModal(supplier);
    } else {
        ErrorHandler.showToast('Proveedor no encontrado', 'error');
    }
}

async function deleteSupplier(id) {
    const supplier = suppliers.find(s => s.idProveedor === id);
    if (!supplier) {
        ErrorHandler.showToast('Proveedor no encontrado', 'error');
        return;
    }

    if (!confirm(`¿Eliminar el proveedor "${supplier.nombreProveedor}"?\n\nEsta acción desactivará el proveedor.`)) {
        return;
    }

    try {
        const response = await API.suppliers.delete(id);

        if (response.success) {
            ErrorHandler.showToast('Proveedor desactivado exitosamente', 'success');
            await loadSuppliers();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error deleting supplier:', error);
        ErrorHandler.handleNetworkError(error);
    }
}


function createLoadingSpinner() {
    return `
        <div class="col-span-full flex justify-center py-12">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
        </div>
    `;
}

function createEmptyState(message) {
    return `
        <div class="col-span-full text-center py-12 text-text-secondary-light dark:text-text-secondary-dark">
            <span class="material-icons-outlined text-4xl mb-2 opacity-50">local_shipping</span>
            <p class="text-lg font-medium">${message}</p>
            ${filteredSuppliers.length === 0 && suppliers.length > 0 ? `
                <button onclick="clearFilters()" 
                    class="mt-4 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600 transition-colors">
                    Ver todos los proveedores
                </button>
            ` : ''}
        </div>
    `;
}

function createErrorState(message) {
    return `
        <div class="col-span-full text-center py-12 text-red-500">
            <span class="material-icons-outlined text-4xl mb-2">error_outline</span>
            <p class="text-lg font-medium">${message}</p>
            <button onclick="loadSuppliers()" 
                class="mt-4 px-4 py-2 bg-primary text-white rounded hover:bg-blue-600 transition-colors">
                Reintentar
            </button>
        </div>
    `;
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


document.addEventListener('DOMContentLoaded', initAdminSuppliersPage);