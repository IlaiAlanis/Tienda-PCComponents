// Current filter state
let currentFilters = {
    page: 1,
    pageSize: 12,
    categoryIds: [],
    categoryNames: [],
    brandIds: [],
    search: '',
    minPrice: null,
    maxPrice: null,
    inStock: false,
    sortBy: 'featured'
};

// Store categories and brands for filters
let allCategories = [];
let allBrands = [];

/**
 * Initialize catalog page
 */
async function initCatalogPage() {
    try {
        console.log('🚀 Initializing catalog page...');
        document.getElementById('navbar').innerHTML = Components.navbar();
        document.getElementById('footer').innerHTML = Components.footer();
        document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

        // IMPORTANT: Load filters data FIRST
        await Promise.all([
            loadCategoriesForFilter(),
            loadBrandsForFilter()
        ]);

        // THEN apply URL parameters (after data is loaded)
        applyUrlParameters();

        // Load products
        await loadProducts();

        // Setup event listeners
        setupEventListeners();

        console.log('✅ Catalog initialized successfully');
    } catch (error) {
        console.error('❌ Error initializing catalog:', error);
        ErrorHandler.showToast('Error al cargar el catálogo', 'error');
    }
}

/**
 * Apply URL parameters to filters - FIXED
 */
function applyUrlParameters() {
    const urlParams = new URLSearchParams(window.location.search);

    // SEARCH parameter
    if (urlParams.has('search')) {
        currentFilters.search = urlParams.get('search');
        const searchInput = document.getElementById('globalSearch');
        if (searchInput) {
            searchInput.value = currentFilters.search;
        }
        console.log('🔍 Search query:', currentFilters.search);
    }

    // CATEGORY parameter (from navbar clicks)
    if (urlParams.has('category')) {
        const categoryParam = urlParams.get('category');
        console.log('📂 Category from URL:', categoryParam);

        // FIXED: Check multiple possible property names
        const matchingCategory = allCategories.find(cat => {
            const catName = cat.nombreCategoria || cat.nombre || '';
            return catName.toLowerCase() === categoryParam.toLowerCase();
        });

        if (matchingCategory) {
            const catId = matchingCategory.idCategoria || matchingCategory.id;
            const catName = matchingCategory.nombreCategoria || matchingCategory.nombre;

            currentFilters.categoryIds = [catId];
            currentFilters.categoryNames = [catName];
            console.log('✅ Category matched:', catName, 'ID:', catId);
        } else {
            console.warn('⚠️ Category not found:', categoryParam);
            console.log('Available categories:', allCategories.map(c => c.nombreCategoria || c.nombre));
        }
    }

    // BRAND parameter
    if (urlParams.has('brand')) {
        const brandParam = parseInt(urlParams.get('brand'));
        if (!isNaN(brandParam)) {
            currentFilters.brandIds = [brandParam];
        }
    }

    // Update page title
    updatePageTitle();
}

/**
 * Update page title based on filters
 */
function updatePageTitle() {
    const titleElement = document.getElementById('pageTitle');
    const breadcrumb = document.getElementById('breadcrumbCategory');

    if (currentFilters.search) {
        const title = `Resultados para: "${currentFilters.search}"`;
        if (titleElement) titleElement.textContent = title;
        if (breadcrumb) breadcrumb.textContent = title;
    } else if (currentFilters.categoryNames.length > 0) {
        const title = currentFilters.categoryNames.join(', ');
        if (titleElement) titleElement.textContent = title;
        if (breadcrumb) breadcrumb.textContent = title;
    } else {
        if (titleElement) titleElement.textContent = 'Catálogo de Productos';
        if (breadcrumb) breadcrumb.textContent = 'Catálogo';
    }
}

/**
 * Load categories for filter sidebar
 */
async function loadCategoriesForFilter() {
    try {
        const response = await API.categories.getAll();

        console.log('📦 Categories response:', response);

        if (response.success && response.data) {
            allCategories = response.data;
            console.log('✅ Categories loaded:', allCategories.length);

            // Log first category structure for debugging
            if (allCategories.length > 0) {
                console.log('Sample category structure:', allCategories[0]);
            }

            renderCategoryFilters();
        }
    } catch (error) {
        console.error('❌ Error loading categories:', error);
    }
}

/**
 * Load brands for filter sidebar
 */
async function loadBrandsForFilter() {
    try {
        const response = await API.brands.getAll();

        console.log('🏷️ Brands response:', response);

        if (response.success && response.data) {
            allBrands = response.data;
            console.log('✅ Brands loaded:', allBrands.length);
            renderBrandFilters();
        }
    } catch (error) {
        console.error('❌ Error loading brands:', error);
    }
}

/**
 * Render category filter checkboxes - FIXED
 */
function renderCategoryFilters() {
    const container = document.getElementById('categoryFilters');
    const mobileContainer = document.getElementById('categoryFiltersMobile');

    if (!container) return;

    const html = allCategories.map(cat => {
        const categoryId = cat.idCategoria || cat.id;
        const categoryName = cat.nombreCategoria || cat.nombre;
        const isChecked = currentFilters.categoryIds.includes(categoryId);

        return `
            <label class="flex items-center cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 p-2 rounded">
                <input 
                    type="checkbox" 
                    value="${categoryId}" 
                    onchange="toggleCategoryFilter(${categoryId}, '${categoryName}')"
                    ${isChecked ? 'checked' : ''}
                    class="text-primary focus:ring-primary rounded"
                />
                <span class="ml-2 text-sm">${categoryName}</span>
            </label>
        `;
    }).join('');

    container.innerHTML = html || '<p class="text-sm text-gray-500">No hay categorías disponibles</p>';
    if (mobileContainer) {
        mobileContainer.innerHTML = html;
    }
}

/**
 * Render brand filter checkboxes - FIXED
 */
function renderBrandFilters() {
    const container = document.getElementById('brandFilters');
    const mobileContainer = document.getElementById('brandFiltersMobile');

    if (!container) return;

    const html = allBrands.map(brand => {
        const brandId = brand.idMarca || brand.id;
        const brandName = brand.nombreMarca || brand.nombre;
        const isChecked = currentFilters.brandIds.includes(brandId);

        return `
            <label class="flex items-center cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-700 p-2 rounded">
                <input 
                    type="checkbox" 
                    value="${brandId}" 
                    onchange="toggleBrandFilter(${brandId})"
                    ${isChecked ? 'checked' : ''}
                    class="text-primary focus:ring-primary rounded"
                />
                <span class="ml-2 text-sm">${brandName}</span>
            </label>
        `;
    }).join('');

    container.innerHTML = html || '<p class="text-sm text-gray-500">No hay marcas disponibles</p>';
    if (mobileContainer) {
        mobileContainer.innerHTML = html;
    }
}

/**
 * Toggle category filter
 */
function toggleCategoryFilter(categoryId, categoryName) {
    const index = currentFilters.categoryIds.indexOf(categoryId);

    if (index > -1) {
        // Remove category
        currentFilters.categoryIds.splice(index, 1);
        currentFilters.categoryNames.splice(index, 1);
    } else {
        // Add category
        currentFilters.categoryIds.push(categoryId);
        currentFilters.categoryNames.push(categoryName);
    }

    currentFilters.page = 1;
    updatePageTitle();
    loadProducts();
}

/**
 * Toggle brand filter
 */
function toggleBrandFilter(brandId) {
    const index = currentFilters.brandIds.indexOf(brandId);

    if (index > -1) {
        // Remove brand
        currentFilters.brandIds.splice(index, 1);
    } else {
        // Add brand
        currentFilters.brandIds.push(brandId);
    }

    currentFilters.page = 1;
    loadProducts();
}

/**
 * Update price label
 */
function updatePriceLabel() {
    const priceRange = document.getElementById('priceRange');
    const priceLabel = document.getElementById('priceLabel');
    const priceRangeMobile = document.getElementById('priceRangeMobile');
    const priceLabelMobile = document.getElementById('priceLabelMobile');

    if (priceRange && priceLabel) {
        const value = parseInt(priceRange.value);
        priceLabel.textContent = value >= 10000 ? '$10,000+' : `$${value.toLocaleString()}`;
        currentFilters.maxPrice = value >= 10000 ? null : value;
    }

    if (priceRangeMobile && priceLabelMobile) {
        const value = parseInt(priceRangeMobile.value);
        priceLabelMobile.textContent = value >= 10000 ? '$10,000+' : `$${value.toLocaleString()}`;
    }
}

/**
 * Apply filters
 */
function applyFilters() {
    const sortSelect = document.getElementById('sortBy');
    if (sortSelect) {
        currentFilters.sortBy = sortSelect.value;
    }

    const inStockCheckbox = document.getElementById('inStockOnly');
    if (inStockCheckbox) {
        currentFilters.inStock = inStockCheckbox.checked;
    }

    currentFilters.page = 1;
    loadProducts();
}

/**
 * Clear all filters
 */
function clearFilters() {
    currentFilters = {
        page: 1,
        pageSize: 12,
        categoryIds: [],
        categoryNames: [],
        brandIds: [],
        search: '',
        minPrice: null,
        maxPrice: null,
        inStock: false,
        sortBy: 'featured'
    };

    // Clear URL parameters
    window.history.pushState({}, '', window.location.pathname);

    // Reset UI
    const searchInput = document.getElementById('globalSearch');
    if (searchInput) searchInput.value = '';

    const priceRange = document.getElementById('priceRange');
    if (priceRange) {
        priceRange.value = 10000;
        updatePriceLabel();
    }

    const inStockCheckbox = document.getElementById('inStockOnly');
    if (inStockCheckbox) inStockCheckbox.checked = false;

    const sortSelect = document.getElementById('sortBy');
    if (sortSelect) sortSelect.value = 'featured';

    // Re-render filters
    renderCategoryFilters();
    renderBrandFilters();

    // Update title and load
    updatePageTitle();
    loadProducts();
}

/**
 * Load products with current filters
 */
async function loadProducts() {
    const grid = document.getElementById('productsGrid');
    if (!grid) return;

    // Show loading state
    grid.innerHTML = `
        <div class="col-span-full text-center py-12">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
            <p class="text-gray-500">Cargando productos...</p>
        </div>
    `;

    try {
        const payload = buildSearchPayload();
        console.log('🔎 Search payload:', payload);

        const response = await API.products.search(payload);
        console.log('📦 Products response:', response);

        if (response.success && response.data) {
            const products = response.data.items || [];
            const totalItems = response.data.totalCount || 0;
            const currentPage = response.data.currentPage || 1;
            const totalPages = response.data.totalPages || 1;
            // Update product count
            updateProductCount(totalItems);

            if (products.length > 0) {
                // Render products using Components.productCard()
                grid.innerHTML = products.map(product => Components.productCard(product)).join('');

                // Render pagination
                renderPagination(currentPage, totalPages);
            } else {
                grid.innerHTML = `
                    <div class="col-span-full text-center py-12 text-gray-500">
                        <span class="material-icons text-6xl mb-4">search_off</span>
                        <p class="text-lg">No se encontraron productos</p>
                        <button onclick="clearFilters()" class="mt-4 px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors">
                            Limpiar Filtros
                        </button>
                    </div>
                `;
                document.getElementById('pagination').innerHTML = '';
            }
        } else {
            grid.innerHTML = `
                <div class="col-span-full text-center py-12 text-red-500">
                    <span class="material-icons text-6xl mb-4">error_outline</span>
                    <p class="text-lg">Error al cargar productos</p>
                    <button onclick="loadProducts()" class="mt-4 px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors">
                        Reintentar
                    </button>
                </div>
            `;
        }
    } catch (error) {
        console.error('❌ Error loading products:', error);
        ErrorHandler.handleNetworkError(error);
        grid.innerHTML = `
            <div class="col-span-full text-center py-12 text-red-500">
                <span class="material-icons text-6xl mb-4">wifi_off</span>
                <p class="text-lg">Error de conexión</p>
                <button onclick="loadProducts()" class="mt-4 px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover transition-colors">
                    Reintentar
                </button>
            </div>
        `;
    }
}

/**
 * Build search payload for API
 */
function buildSearchPayload() {
    const payload = {
        searchTerm: currentFilters.search || null,
        categoriaIds: currentFilters.categoryIds.length > 0 ? currentFilters.categoryIds : null,
        marcaIds: currentFilters.brandIds.length > 0 ? currentFilters.brandIds : null,
        minPrice: currentFilters.minPrice || null,
        maxPrice: currentFilters.maxPrice || null,
        enStock: currentFilters.inStock || null,
        enDescuento: null,
        page: currentFilters.page,
        pageSize: currentFilters.pageSize,
        orderBy: 'nombre',
        orderDirection: 'asc'
    };

    // Handle sorting
    switch (currentFilters.sortBy) {
        case 'price_asc':
            payload.orderBy = 'precio';
            payload.orderDirection = 'asc';
            break;
        case 'price_desc':
            payload.orderBy = 'precio';
            payload.orderDirection = 'desc';
            break;
        case 'name_asc':
            payload.orderBy = 'nombre';
            payload.orderDirection = 'asc';
            break;
        case 'name_desc':
            payload.orderBy = 'nombre';
            payload.orderDirection = 'desc';
            break;
        case 'featured':
            payload.orderBy = 'destacado';
            payload.orderDirection = 'desc';
            break;
        default:
            payload.orderBy = 'nombre';
            payload.orderDirection = 'asc';
    }

    return payload;
}

/**
 * Render pagination
 */
function renderPagination(currentPage, totalPages) {
    const container = document.getElementById('pagination');
    if (!container || totalPages <= 1) {
        if (container) container.innerHTML = '';
        return;
    }

    let html = '';

    // Previous button
    html += `
        <button 
            onclick="changePage(${currentPage - 1})" 
            ${currentPage === 1 ? 'disabled' : ''} 
            class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
            Anterior
        </button>
    `;

    // Page numbers
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
            html += `
                <button 
                    onclick="changePage(${i})" 
                    class="px-4 py-2 border rounded transition-colors ${i === currentPage ? 'bg-primary text-white border-primary' : 'border-gray-300 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-700'}">
                    ${i}
                </button>
            `;
        } else if (i === currentPage - 3 || i === currentPage + 3) {
            html += '<span class="px-2 text-gray-500">...</span>';
        }
    }

    // Next button
    html += `
        <button 
            onclick="changePage(${currentPage + 1})" 
            ${currentPage === totalPages ? 'disabled' : ''} 
            class="px-4 py-2 border border-gray-300 dark:border-gray-600 rounded hover:bg-gray-100 dark:hover:bg-gray-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors">
            Siguiente
        </button>
    `;

    container.innerHTML = html;
}

/**
 * Change page
 */
function changePage(page) {
    if (page < 1) return;
    currentFilters.page = page;
    loadProducts();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

/**
 * Update product count display
 */
function updateProductCount(count) {
    const counter = document.getElementById('productCount');
    if (counter) {
        counter.textContent = `${count} producto${count !== 1 ? 's' : ''} encontrado${count !== 1 ? 's' : ''}`;
    }
}

async function addToCart(productId) {
    if (!Auth.isAuthenticated()) {
        ErrorHandler.showToast('Debes iniciar sesión', 'warning');
        setTimeout(() => {
            window.location.href = `../pages/login.html?redirect=${encodeURIComponent(window.location.pathname)}`;
        }, 1500);
        return;
    }

    try {
        const response = await API.cart.addItem({
            productoId: productId,
            cantidad: 1
        });

        if (response.success) {
            ErrorHandler.showToast('Producto agregado al carrito', 'success');
            // Update cart badge if available
            if (typeof Components !== 'undefined' && Components.updateCartBadge) {
                Components.updateCartBadge();
            }
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

/**
 * Perform search from search bar
 */
function performSearch(searchTerm) {
    if (searchTerm.trim()) {
        currentFilters.search = searchTerm.trim();
        currentFilters.page = 1;
        updatePageTitle();
        loadProducts();
    }
}

/**
 * Setup event listeners
 */
function setupEventListeners() {
    // Sort select
    const sortSelect = document.getElementById('sortBy');
    if (sortSelect) {
        sortSelect.addEventListener('change', applyFilters);
    }

    // Price range
    const priceRange = document.getElementById('priceRange');
    if (priceRange) {
        priceRange.addEventListener('input', updatePriceLabel);
        priceRange.addEventListener('change', applyFilters);
    }

    // Stock checkbox
    const inStockCheckbox = document.getElementById('inStockOnly');
    if (inStockCheckbox) {
        inStockCheckbox.addEventListener('change', applyFilters);
    }

    // Global search bar (in navbar)
    const globalSearch = document.getElementById('globalSearch');
    if (globalSearch) {
        // Search on Enter key
        globalSearch.addEventListener('keypress', (e) => {
            if (e.key === 'Enter' && globalSearch.value.trim()) {
                performSearch(globalSearch.value);
            }
        });

        // Optional: Search on input with debounce
        let searchTimeout;
        globalSearch.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                if (globalSearch.value.trim().length >= 3) {
                    performSearch(globalSearch.value);
                } else if (globalSearch.value.trim().length === 0 && currentFilters.search) {
                    currentFilters.search = '';
                    loadProducts();
                }
            }, 500);
        });
    }
}

/**
 * Open mobile filters
 */
function openMobileFilters() {
    const modal = document.getElementById('mobileFilterModal');
    if (modal) {
        modal.classList.remove('hidden');
    }
}

/**
 * Close mobile filters
 */
function closeMobileFilters() {
    const modal = document.getElementById('mobileFilterModal');
    if (modal) {
        modal.classList.add('hidden');
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', initCatalogPage);