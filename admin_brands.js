// assets/js/pages/admin_brands.js

let brands = [];
let filteredBrands = [];
let editingBrandId = null;

async function initAdminBrandsPage() {
    if (!Auth.requireAdmin()) return;

    await loadAdminProfile();
    await loadBrands();
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
            document.getElementById('adminAvatar').src = `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'Admin')}&background=007BFF&color=fff`;
        }
    } catch (error) {
        console.error('Error loading admin profile:', error);
    }
}

function setupEventListeners() {
    document.getElementById('searchInput').addEventListener('input', debounce((e) => {
        filterBrands(e.target.value);
    }, 300));
    document.getElementById('brandForm').addEventListener('submit', handleBrandSubmit);
}

async function loadBrands() {
    const grid = document.getElementById('brandsGrid');
    grid.innerHTML = '<div class="col-span-full flex justify-center py-12"><div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div></div>';

    try {
        const response = await API.brands.getAll();
        if (response.success && response.data) {
            brands = response.data;
            filteredBrands = brands;
            renderBrands();
        } else {
            ErrorHandler.handleApiError(response);
            grid.innerHTML = '<div class="col-span-full text-center py-12 text-red-500">Error al cargar marcas</div>';
        }
    } catch (error) {
        console.error('Error loading brands:', error);
        ErrorHandler.showToast('Error al cargar marcas', 'error');
        grid.innerHTML = '<div class="col-span-full text-center py-12 text-red-500">Error de conexión</div>';
    }
}

function filterBrands(searchTerm) {
    filteredBrands = searchTerm
        ? brands.filter(b => b.nombre.toLowerCase().includes(searchTerm.toLowerCase()))
        : brands;
    renderBrands();
}

function renderBrands() {
    const grid = document.getElementById('brandsGrid');
    if (filteredBrands.length === 0) {
        grid.innerHTML = '<div class="col-span-full text-center py-12 text-text-secondary-light dark:text-text-secondary-dark"><span class="material-icons-outlined text-4xl mb-2 opacity-50">label</span><p>No se encontraron marcas</p></div>';
        return;
    }
    grid.innerHTML = filteredBrands.map(brand => createBrandCard(brand)).join('');
}

function createBrandCard(brand) {
    const statusColor = brand.estaActivo ? 'bg-green-200 text-green-800 dark:bg-green-900/40 dark:text-green-400' : 'bg-gray-500/20 text-gray-800 dark:text-gray-400';
    return `
        <div class="bg-card-light dark:bg-card-dark rounded-lg shadow-md p-6 hover:shadow-lg transition-shadow border border-border-light dark:border-border-dark">
            <div class="flex items-start justify-between mb-3">
                <h3 class="text-lg font-bold">${brand.nombre}</h3>
                <span class="px-2 py-1 text-xs font-semibold rounded-full ${statusColor}">${brand.estaActivo ? 'Activa' : 'Inactiva'}</span>
            </div>
            ${brand.descripcion ? `<p class="text-sm text-text-secondary-light dark:text-text-secondary-dark mb-4">${brand.descripcion}</p>` : ''}
            <div class="flex items-center gap-2 pt-3 border-t border-border-light dark:border-border-dark">
                <button onclick="editBrand(${brand.idMarca})" class="flex-1 py-2 text-sm font-medium text-warning hover:bg-warning/10 rounded transition-colors">Editar</button>
                <button onclick="deleteBrand(${brand.idMarca})" class="flex-1 py-2 text-sm font-medium text-danger hover:bg-danger/10 rounded transition-colors">Eliminar</button>
            </div>
        </div>
    `;
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    filteredBrands = brands;
    renderBrands();
}

function openBrandModal(brand = null) {
    editingBrandId = brand?.idMarca || null;
    document.getElementById('modalTitle').textContent = brand ? 'Editar Marca' : 'Nueva Marca';
    const form = document.getElementById('brandForm');
    if (brand) {
        form.nombre.value = brand.nombre || '';
        form.descripcion.value = brand.descripcion || '';
        form.estaActivo.checked = brand.estaActivo !== false;
    } else {
        form.reset();
        form.estaActivo.checked = true;
    }
    document.getElementById('brandModal').classList.remove('hidden');
}

function closeBrandModal() {
    document.getElementById('brandModal').classList.add('hidden');
    editingBrandId = null;
}

async function handleBrandSubmit(e) {
    e.preventDefault();
    const formData = new FormData(e.target);
    const data = {
        nombre: formData.get('nombre'),
        descripcion: formData.get('descripcion'),
        estaActivo: formData.get('estaActivo') === 'on',
    };

    const btn = document.getElementById('saveBtn');
    ErrorHandler.setLoading(btn, true);

    try {
        const response = editingBrandId
            ? await API.brands.update(editingBrandId, data)
            : await API.brands.create(data);

        ErrorHandler.setLoading(btn, false);

        if (response.success) {
            ErrorHandler.showToast(editingBrandId ? 'Marca actualizada' : 'Marca creada', 'success');
            closeBrandModal();
            await loadBrands();
        } else {
            ErrorHandler.handleApiError(response, e.target);
        }
    } catch (error) {
        ErrorHandler.setLoading(btn, false);
        console.error('Error saving brand:', error);
        ErrorHandler.showToast('Error al guardar marca', 'error');
    }
}

function editBrand(id) {
    const brand = brands.find(b => b.idMarca === id);
    if (brand) openBrandModal(brand);
}

async function deleteBrand(id) {
    if (!confirm('¿Eliminar esta marca?')) return;
    try {
        const response = await API.brands.delete(id);
        if (response.success) {
            ErrorHandler.showToast('Marca eliminada', 'success');
            await loadBrands();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error deleting brand:', error);
        ErrorHandler.showToast('Error al eliminar marca', 'error');
    }
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
    if (confirm('¿Cerrar sesión?')) Auth.logout();
}

document.addEventListener('DOMContentLoaded', initAdminBrandsPage);