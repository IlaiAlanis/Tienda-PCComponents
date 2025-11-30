let products = [];
let categories = [];
let brands = [];
let suppliers = [];
let currentPage = 1;
let totalPages = 1;
let totalItems = 0;
let filters = {
    search: '',
    category: '',
    stock: '',
    page: 1,
    pageSize: 10
};
let editingProductId = null;

/**
 * Initialize admin products page
 */
async function initAdminProductsPage() {
    // Check if user is admin
    if (!Auth.requireAdmin()) {
        return;
    }

    // Load all necessary data
    await loadAdminProfile();
    await loadFilterData();
    await loadProducts();

    // Setup event listeners
    setupEventListeners();

    // Restore sidebar state
    restoreSidebarState();
}

/**
 * Load admin profile information
 */
async function loadAdminProfile() {
    try {
        const user = Auth.getUser();
        if (user) {
            // Build full name
            const fullName = [
                capitalizeName(user?.nombre),
                capitalizeName(user?.apellidoPaterno),
                capitalizeName(user?.apellidoMaterno)
            ].filter(Boolean).join(" ");

            // Update UI
            const adminNameEl = document.getElementById('adminName');
            const adminEmailEl = document.getElementById('adminEmail');
            const adminAvatarEl = document.getElementById('adminAvatar');

            if (adminNameEl) adminNameEl.textContent = fullName || 'Administrador';
            if (adminEmailEl) adminEmailEl.textContent = user?.correo || 'admin@example.com';

            if (adminAvatarEl) {
                const avatarUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName || 'Admin')}&background=007BFF&color=fff`;
                adminAvatarEl.src = avatarUrl;
            }
        }
    } catch (error) {
        console.error('Error loading admin profile:', error);
    }
}

/**
 * Capitalize first letter of name
 */
function capitalizeName(name) {
    if (!name) return '';
    return name.charAt(0).toUpperCase() + name.slice(1).toLowerCase();
}

/**
 * Load filter data (categories, brands, suppliers)
 */
async function loadFilterData() {
    try {
        // Load categories for filter dropdown
        const categoriesResponse = await API.categories.getAll();
        if (categoriesResponse.success && categoriesResponse.data) {
            categories = categoriesResponse.data;
            renderCategoryFilter();
            renderCategoryOptions();
        }

        // Load brands for modal dropdown
        const brandsResponse = await API.brands.getAll();
        if (brandsResponse.success && brandsResponse.data) {
            brands = brandsResponse.data;
            renderBrandOptions();
        }

        // Load suppliers (optional for create/edit form)
        try {
            const suppliersResponse = await API.suppliers.getAll();
            if (suppliersResponse.success && suppliersResponse.data) {
                suppliers = suppliersResponse.data;
                renderSupplierOptions();
            }
        } catch (error) {
            // Suppliers might not be implemented yet
            console.warn('Suppliers not available:', error);
        }
    } catch (error) {
        console.error('Error loading filter data:', error);
        ErrorHandler.showToast('Error al cargar filtros', 'error');
    }
}

/**
 * Render category filter dropdown
 */
function renderCategoryFilter() {
    const select = document.getElementById('categoryFilter');
    if (!select) return;

    const options = categories.map(cat =>
        `<option value="${cat.idCategoria}">${cat.nombre}</option>`
    ).join('');

    select.innerHTML = `<option value="">Todas las categorías</option>${options}`;
}

/**
 * Render category options in modal
 */
function renderCategoryOptions() {
    const select = document.querySelector('[name="categoriaId"]');
    if (!select) return;

    const options = categories.map(cat =>
        `<option value="${cat.idCategoria}">${cat.nombre}</option>`
    ).join('');

    select.innerHTML = `<option value="">Seleccionar categoría</option>${options}`;
}

/**
 * Render brand options in modal
 */
function renderBrandOptions() {
    const select = document.querySelector('[name="marcaId"]');
    if (!select) return;

    const options = brands.map(brand =>
        `<option value="${brand.idMarca}">${brand.nombre}</option>`
    ).join('');

    select.innerHTML = `<option value="">Seleccionar marca</option>${options}`;
}

/**
 * Render supplier options in modal
 */
function renderSupplierOptions() {
    const select = document.querySelector('[name="proveedorId"]');
    if (!select) return;

    const options = suppliers.map(supplier =>
        `<option value="${supplier.idProveedor}">${supplier.nombreProveedor || supplier.nombre}</option>`
    ).join('');

    select.innerHTML = `<option value="">Seleccionar proveedor (opcional)</option>${options}`;
}

/**
 * Setup all event listeners
 */
function setupEventListeners() {
    // Search input with debounce
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', debounce((e) => {
            filters.search = e.target.value;
            filters.page = 1;
            loadProducts();
        }, 500));
    }

    // Category filter
    const categoryFilter = document.getElementById('categoryFilter');
    if (categoryFilter) {
        categoryFilter.addEventListener('change', (e) => {
            filters.category = e.target.value;
            filters.page = 1;
            loadProducts();
        });
    }

    // Stock filter
    const stockFilter = document.getElementById('stockFilter');
    if (stockFilter) {
        stockFilter.addEventListener('change', (e) => {
            filters.stock = e.target.value;
            filters.page = 1;
            loadProducts();
        });
    }

    // Product form submission
    const productForm = document.getElementById('productForm');
    if (productForm) {
        productForm.addEventListener('submit', handleProductSubmit);
    }
}

/**
 * Load products with filters
 */
async function loadProducts() {
    const tbody = document.getElementById('productsTableBody');
    if (!tbody) return;

    // Show loading spinner
    tbody.innerHTML = `
        <tr>
            <td colspan="6" class="px-6 py-12 text-center">
                <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
                <p class="mt-2 text-text-secondary-light dark:text-text-secondary-dark">Cargando productos...</p>
            </td>
        </tr>
    `;

    try {
        // ✅ FIXED: Build search payload with correct property names
        const searchPayload = {
            searchTerm: filters.search || null,                    // ✅ Changed from "search"
            categoriaId: filters.category ? parseInt(filters.category) : null,  // ✅ Changed from "categoria"
            page: filters.page,
            pageSize: filters.pageSize
        };

        // ✅ FIXED: Add server-side stock filter
        if (filters.stock) {
            if (filters.stock === 'in-stock') {
                searchPayload.enStock = true;  // Server-side: only in-stock items
            } else if (filters.stock === 'out-stock') {
                searchPayload.enStock = false; // Server-side: only out-of-stock items
            }
            // For 'low-stock', we'll filter client-side since backend doesn't support exact range
        }

        const response = await API.products.search(searchPayload);

        if (response.success && response.data) {
            const result = response.data;

            // Check if paginated response
            if (result.items) {
                products = result.items || [];
                totalPages = result.totalPages || 1;
                totalItems = result.totalCount || result.total || 0;
                currentPage = result.page || filters.page;
            } else if (Array.isArray(result)) {
                // Legacy non-paginated response
                products = result;
                totalPages = 1;
                totalItems = products.length;
                currentPage = 1;
            } else {
                products = [];
                totalPages = 1;
                totalItems = 0;
            }

            // ✅ Client-side low-stock filter (only if needed)
            if (filters.stock === 'low-stock') {
                products = products.filter(p => {
                    const stock = p.stock ?? p.stock ?? 0;
                    return stock > 0 && stock <= 5;
                });
                totalItems = products.length;
            }

            // Render products or empty state
            if (products.length === 0) {
                tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="px-6 py-12 text-center text-text-secondary-light dark:text-text-secondary-dark">
                            <span class="material-icons-outlined text-4xl mb-2 opacity-50">inventory_2</span>
                            <p>No se encontraron productos</p>
                            <p class="text-sm mt-2">Intenta ajustar los filtros o agrega un nuevo producto</p>
                        </td>
                    </tr>
                `;
            } else {
                tbody.innerHTML = products.map(product => createProductRow(product)).join('');
            }

            updatePagination();
        } else {
            ErrorHandler.handleApiError(response);
            tbody.innerHTML = `
                <tr>
                    <td colspan="6" class="px-6 py-12 text-center text-red-500">
                        <span class="material-icons-outlined text-4xl mb-2">error_outline</span>
                        <p>Error al cargar productos</p>
                    </td>
                </tr>
            `;
        }
    } catch (error) {
        console.error('Error loading products:', error);
        ErrorHandler.showToast('Error al cargar productos', 'error');
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="px-6 py-12 text-center text-red-500">
                    <span class="material-icons-outlined text-4xl mb-2">cloud_off</span>
                    <p>Error de conexión</p>
                </td>
            </tr>
        `;
    }
}

/**
 * Create product table row
 */
function createProductRow(product) {
    // Handle image URL from different possible property names
    const imageUrl = product.imagenUrl ||
        product.imagenes?.[0]?.urlImagen ||
        product.imagenes?.[0] ||
        'https://via.placeholder.com/50x50?text=P';

    // Handle stock from different possible property names
    const stock = product.stock ?? product.stock ?? 0;

    // Handle price
    const price = product.precioPromocional || product.precioBase;
    const hasDiscount = product.precioPromocional && product.precioPromocional < product.precioBase;

    // Stock status badge
    let stockStatus = '';
    let stockClass = '';
    if (stock === 0) {
        stockStatus = 'Sin Stock';
        stockClass = 'bg-red-500/20 text-red-800 dark:text-red-400';
    } else if (stock <= 5) {
        stockStatus = 'Stock Bajo';
        stockClass = 'bg-orange-500/20 text-orange-800 dark:text-orange-400';
    } else {
        stockStatus = 'En Stock';
        stockClass = 'bg-green-200 text-green-800 dark:bg-green-900/40 dark:text-green-400';
    }

    // Handle product name from different possible property names
    const productName = product.nombreProducto || product.nombre || 'Producto';
    const productSku = product.sku || product.idProducto;
    const category = product.categoria || 'Sin categoría';

    return `
        <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
            <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                    <img 
                        src="${imageUrl}" 
                        alt="${productName}" 
                        class="w-12 h-12 object-cover rounded shadow-sm" 
                        onerror="this.src='https://via.placeholder.com/50x50?text=P'"
                    />
                    <div class="flex-1 min-w-0">
                        <p class="font-semibold truncate text-text-primary-light dark:text-text-primary-dark">
                            ${productName}
                        </p>
                        <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark">
                            SKU: ${productSku}
                        </p>
                    </div>
                </div>
            </td>
            <td class="px-6 py-4">
                <span class="text-sm text-text-primary-light dark:text-text-primary-dark">
                    ${category}
                </span>
            </td>
            <td class="px-6 py-4">
                <div>
                    <p class="font-semibold text-text-primary-light dark:text-text-primary-dark">
                        ${formatCurrency(price)}
                    </p>
                    ${hasDiscount ? `
                        <p class="text-xs text-text-secondary-light dark:text-text-secondary-dark line-through">
                            ${formatCurrency(product.precioBase)}
                        </p>
                    ` : ''}
                </div>
            </td>
            <td class="px-6 py-4">
                <span class="font-semibold text-text-primary-light dark:text-text-primary-dark">
                    ${stock}
                </span>
            </td>
            <td class="px-6 py-4">
                <span class="px-2 py-1 text-xs font-semibold rounded-full ${stockClass}">
                    ${stockStatus}
                </span>
            </td>
            <td class="px-6 py-4">
                <div class="flex items-center gap-1 justify-end">
                    <button 
                        onclick="editProduct(${product.idProducto})" 
                        class="p-2 text-warning hover:bg-warning/10 rounded transition-colors" 
                        title="Editar producto"
                    >
                        <span class="material-icons-outlined text-base">edit</span>
                    </button>
                    <button 
                        onclick="deleteProduct(${product.idProducto})" 
                        class="p-2 text-danger hover:bg-danger/10 rounded transition-colors" 
                        title="Eliminar producto"
                    >
                        <span class="material-icons-outlined text-base">delete</span>
                    </button>
                </div>
            </td>
        </tr>
    `;
}

/**
 * Format currency (MXN)
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN'
    }).format(amount);
}

/**
 * Update pagination UI
 */
function updatePagination() {
    const paginationInfo = document.getElementById('paginationInfo');
    const paginationContainer = document.getElementById('pagination');

    if (!paginationContainer) return;

    const buttons = [];

    // Update info text
    const start = totalItems === 0 ? 0 : (currentPage - 1) * filters.pageSize + 1;
    const end = Math.min(currentPage * filters.pageSize, totalItems);

    if (paginationInfo) {
        paginationInfo.textContent = `Mostrando ${start}-${end} de ${totalItems}`;
    }

    // Previous button
    buttons.push(`
        <button 
            onclick="changePage(${currentPage - 1})" 
            ${currentPage === 1 ? 'disabled' : ''} 
            class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
            Anterior
        </button>
    `);

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
            buttons.push(`
                <button 
                    onclick="changePage(${i})" 
                    class="px-3 py-2 text-sm rounded ${i === currentPage ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500'} transition-colors"
                >
                    ${i}
                </button>
            `);
        } else if (i === currentPage - 2 || i === currentPage + 2) {
            buttons.push('<span class="px-2 text-sm text-text-secondary-light dark:text-text-secondary-dark">...</span>');
        }
    }

    // Next button
    buttons.push(`
        <button 
            onclick="changePage(${currentPage + 1})" 
            ${currentPage === totalPages ? 'disabled' : ''} 
            class="px-3 py-2 text-sm rounded bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
            Siguiente
        </button>
    `);

    paginationContainer.innerHTML = buttons.join('');
}

/**
 * Change page
 */
function changePage(page) {
    if (page < 1 || page > totalPages) return;
    filters.page = page;
    loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

/**
 * Clear all filters
 */
function clearFilters() {
    const searchInput = document.getElementById('searchInput');
    const categoryFilter = document.getElementById('categoryFilter');
    const stockFilter = document.getElementById('stockFilter');

    if (searchInput) searchInput.value = '';
    if (categoryFilter) categoryFilter.value = '';
    if (stockFilter) stockFilter.value = '';

    filters = { search: '', category: '', stock: '', page: 1, pageSize: 10 };
    loadProducts();
}

/**
 * Open product modal for create/edit
 */
function openProductModal(product = null) {
    editingProductId = product?.idProducto || null;

    const modalTitle = document.getElementById('modalTitle');
    if (modalTitle) {
        modalTitle.textContent = product ? 'Editar Producto' : 'Nuevo Producto';
    }

    const form = document.getElementById('productForm');
    if (!form) return;

    if (product) {
        // Edit mode - populate form
        form.nombreProducto.value = product.nombreProducto || product.nombre || '';
        form.categoriaId.value = product.categoriaId || '';
        form.marcaId.value = product.marcaId || '';

        // Provider is optional
        if (form.proveedorId) {
            form.proveedorId.value = product.proveedorId || '';
        }

        form.sku.value = product.sku || '';
        form.precioBase.value = product.precioBase || '';
        form.precioPromocional.value = product.precioPromocional || '';
        form.stock.value = product.stock ?? product.stock ?? '';
        form.descripcion.value = product.descripcion || '';
        form.estaActivo.checked = product.estaActivo !== false;
    } else {
        // Create mode - reset form
        form.reset();
        form.estaActivo.checked = true;
    }

    const modal = document.getElementById('productModal');
    if (modal) {
        modal.classList.remove('hidden');
    }
}

/**
 * Close product modal
 */
function closeProductModal() {
    const modal = document.getElementById('productModal');
    if (modal) {
        modal.classList.add('hidden');
    }
    editingProductId = null;
}

/**
 * Handle product form submission
 */
async function handleProductSubmit(e) {
    e.preventDefault();

    const formData = new FormData(e.target);

    // ✅ FIXED: Use correct property names matching backend DTOs
    const data = {
        nombreProducto: formData.get('nombreProducto'),         //  Matches backend
        categoriaId: parseInt(formData.get('categoriaId')),     // Matches backend
        marcaId: parseInt(formData.get('marcaId')),             // Matches backend
        proveedorId: formData.get('proveedorId') ? parseInt(formData.get('proveedorId')) : null,
        sku: formData.get('sku'),
        precioBase: parseFloat(formData.get('precioBase')),     // Matches backend
        precioPromocional: formData.get('precioPromocional') ? parseFloat(formData.get('precioPromocional')) : null,
        stock: parseInt(formData.get('stock')), // Matches backend
        descripcion: formData.get('descripcion'),
        estaActivo: formData.get('estaActivo') === 'on',
    };

    // Client-side validation
    if (!data.nombreProducto || !data.categoriaId || !data.marcaId || !data.sku || !data.precioBase) {
        ErrorHandler.showToast('Por favor completa todos los campos requeridos', 'warning');
        return;
    }

    if (data.precioBase <= 0) {
        ErrorHandler.showToast('El precio debe ser mayor a 0', 'warning');
        return;
    }

    if (data.stock < 0) {
        ErrorHandler.showToast('El stock no puede ser negativo', 'warning');
        return;
    }

    const btn = document.getElementById('saveBtn');
    if (!btn) return;

    ErrorHandler.setLoading(btn, true);

    try {
        const response = editingProductId
            ? await API.products.update(editingProductId, data)
            : await API.products.create(data);

        if (response.success) {
            ErrorHandler.showToast(
                editingProductId ? 'Producto actualizado exitosamente' : 'Producto creado exitosamente',
                'success'
            );
            closeProductModal();
            await loadProducts();
        } else {
            ErrorHandler.handleApiError(response, e.target);
        }
    } catch (error) {
        console.error('Error saving product:', error);
        ErrorHandler.showToast('Error al guardar producto', 'error');
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

/**
 * Edit product
 */
function editProduct(id) {
    const product = products.find(p => p.idProducto === id);

    if (product) {
        openProductModal(product);
    } else {
        ErrorHandler.showToast('Producto no encontrado', 'error');
    }
}

/**
 * Delete product
 */
async function deleteProduct(id) {
    const product = products.find(p => p.idProducto === id);
    const productName = product?.nombreProducto || product?.nombre || 'este producto';

    if (!confirm(`¿Estás seguro de eliminar "${productName}"?`)) {
        return;
    }

    try {
        const response = await API.products.delete(id);

        if (response.success) {
            ErrorHandler.showToast('Producto eliminado exitosamente', 'success');
            await loadProducts();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error deleting product:', error);
        ErrorHandler.showToast('Error al eliminar producto', 'error');
    }
}

/**
 * Toggle sidebar (mobile/desktop)
 */
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (!sidebar) return;

    if (window.innerWidth < 1024) {
        // Mobile: toggle sidebar overlay
        sidebar.classList.toggle('mobile-open');
        if (overlay) overlay.classList.toggle('hidden');
    } else {
        // Desktop: collapse sidebar
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


/**
 * Restore sidebar state from localStorage
 */
function restoreSidebarState() {
    const sidebar = document.getElementById('sidebar');
    if (sidebar && window.innerWidth >= 1024 && localStorage.getItem('sidebarCollapsed') === 'true') {
        sidebar.classList.add('collapsed');
    }
}

/**
 * Handle window resize
 */
window.addEventListener('resize', () => {
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');

    if (window.innerWidth >= 1024) {
        if (sidebar) sidebar.classList.remove('mobile-open');
        if (overlay) overlay.classList.add('hidden');
    }
});

/**
 * Debounce function for search input
 */
function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

/**
 * Logout function
 */
function logout() {
    if (confirm('¿Cerrar sesión?')) {
        Auth.logout();
    }
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', initAdminProductsPage);
