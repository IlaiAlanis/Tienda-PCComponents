let discounts = [];
let currentPage = 1;
let totalPages = 1;
let filters = { search: '', status: '', page: 1, pageSize: 10 };
let editingDiscountId = null;
let selectedDiscounts = new Set();

async function initAdminDiscountsPage() {
    if (!Auth.requireAdmin()) return;
    await loadAdminProfile();
    await loadDiscounts();
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
        filters.search = e.target.value;
        filters.page = 1;
        loadDiscounts();
    }, 500));

    document.getElementById('statusFilter').addEventListener('change', () => {
        filters.status = document.getElementById('statusFilter').value;
        filters.page = 1;
        loadDiscounts();
    });

    document.getElementById('selectAll').addEventListener('change', (e) => {
        document.querySelectorAll('.discount-checkbox').forEach(cb => {
            cb.checked = e.target.checked;
            const discountId = parseInt(cb.dataset.discountId);
            e.target.checked ? selectedDiscounts.add(discountId) : selectedDiscounts.delete(discountId);
        });
    });

    document.getElementById('discountForm').addEventListener('submit', handleDiscountSubmit);
}

async function loadDiscounts() {
    const tbody = document.getElementById('discountsGrid');
    tbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center"><div class="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary"></div></td></tr>';

    try {
        const response = await API.discounts.getAll(filters);

        if (response.success && response.data) {
            const result = response.data;

            // Check if backend returns paginated result
            if (result.items) {
                // Backend returned PagedResult<DescuentoDto>
                discounts = result.items;
                totalPages = result.totalPages || 1;
                currentPage = result.page || filters.page;

                if (discounts.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center text-text-secondary-light dark:text-text-secondary-dark"><span class="material-icons-outlined text-4xl mb-2 opacity-50">local_offer</span><p>No se encontraron descuentos</p></td></tr>';
                } else {
                    tbody.innerHTML = discounts.map(discount => createDiscountRow(discount)).join('');
                }
                updatePagination(result.totalCount || 0);
            } else {
                // Do client-side filtering temporarily until backend is updated
                let allDiscounts = result;

                // Client-side filtering by status (temporary)
                if (filters.status) {
                    const now = new Date();
                    allDiscounts = allDiscounts.filter(d => {
                        if (filters.status === 'active') return d.estaActivo && (!d.fechaFin || new Date(d.fechaFin) >= now);
                        if (filters.status === 'expired') return d.fechaFin && new Date(d.fechaFin) < now;
                        if (filters.status === 'inactive') return !d.estaActivo;
                        return true;
                    });
                }

                totalPages = Math.ceil(allDiscounts.length / filters.pageSize);
                const startIndex = (filters.page - 1) * filters.pageSize;
                discounts = allDiscounts.slice(startIndex, startIndex + filters.pageSize);
                currentPage = filters.page;

                if (discounts.length === 0) {
                    tbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center text-text-secondary-light dark:text-text-secondary-dark"><span class="material-icons-outlined text-4xl mb-2 opacity-50">local_offer</span><p>No se encontraron descuentos</p></td></tr>';
                } else {
                    tbody.innerHTML = discounts.map(discount => createDiscountRow(discount)).join('');
                }
                updatePagination(allDiscounts.length);
            }
        } else {
            ErrorHandler.handleApiError(response);
            tbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center text-red-500">Error al cargar descuentos</td></tr>';
        }
    } catch (error) {
        console.error('Error loading discounts:', error);
        ErrorHandler.showToast('Error al cargar descuentos', 'error');
        tbody.innerHTML = '<tr><td colspan="8" class="p-8 text-center text-red-500">Error de conexión</td></tr>';
    }
}

function createDiscountRow(discount) {
    const status = getDiscountStatus(discount);
    const isExpired = discount.fechaFin && new Date(discount.fechaFin) < new Date();

    return `
        <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/50">
            <td class="p-3"><input type="checkbox" class="discount-checkbox rounded border-gray-400 text-primary focus:ring-primary" data-discount-id="${discount.idCupon}" onchange="handleCheckboxChange(${discount.idCupon}, this.checked)"/></td>
            <td class="p-3"><span class="font-mono font-bold text-primary">${discount.codigoCupon}</span></td>
            <td class="p-3">${discount.tipoCupon}</td>
            <td class="p-3 font-semibold">${discount.tipoCupon === 'Porcentaje' ? discount.valor + '%' : formatCurrency(discount.valor)}</td>
            <td class="p-3 text-xs">${discount.fechaFin ? formatDate(discount.fechaFin) : 'Sin límite'}</td>
            <td class="p-3 text-xs">${discount.vecesUsado || 0}${discount.limiteUsos ? ' / ' + discount.limiteUsos : ''}</td>
            <td class="p-3"><span class="px-2 py-1 text-xs font-semibold rounded-full ${status.color}">${status.label}</span></td>
            <td class="p-3">
                <div class="flex items-center gap-1">
                    <button onclick="editDiscount(${discount.idCupon})" class="p-2 text-warning hover:bg-warning/10 rounded" title="Editar"><span class="material-icons-outlined text-base">edit</span></button>
                    <button onclick="deleteDiscount(${discount.idCupon})" class="p-2 text-danger hover:bg-danger/10 rounded" title="Eliminar"><span class="material-icons-outlined text-base">delete</span></button>
                </div>
            </td>
        </tr>
    `;
}

function getDiscountStatus(discount) {
    const now = new Date();
    const isExpired = discount.fechaFin && new Date(discount.fechaFin) < now;
    const limitReached = discount.limiteUsos && discount.vecesUsado >= discount.limiteUsos;

    if (isExpired) return { label: 'Expirado', color: 'bg-gray-500/20 text-gray-800 dark:text-gray-400' };
    if (limitReached) return { label: 'Límite alcanzado', color: 'bg-orange-500/20 text-orange-800 dark:text-orange-400' };
    if (!discount.estaActivo) return { label: 'Inactivo', color: 'bg-red-500/20 text-red-800 dark:text-red-400' };
    return { label: 'Activo', color: 'bg-green-200 text-green-800 dark:bg-green-900/40 dark:text-green-400' };
}

function handleCheckboxChange(discountId, checked) {
    checked ? selectedDiscounts.add(discountId) : selectedDiscounts.delete(discountId);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(amount);
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('es-MX', { year: 'numeric', month: 'short', day: 'numeric' });
}

function updatePagination(total) {
    document.getElementById('paginationInfo').textContent = `Mostrando ${(currentPage - 1) * filters.pageSize + 1}-${Math.min(currentPage * filters.pageSize, total)} de ${total}`;
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
    document.getElementById('paginationButtons').innerHTML = buttons.join('');
}

function changePage(page) {
    if (page < 1 || page > totalPages) return;
    filters.page = page;
    loadDiscounts();
}

function clearFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = '';
    filters = { search: '', status: '', page: 1, pageSize: 10 };
    loadDiscounts();
}

function openDiscountModal(discount = null) {
    editingDiscountId = discount?.idCupon || null;
    document.getElementById('modalTitle').textContent = discount ? 'Editar Descuento' : 'Nuevo Descuento';
    const form = document.getElementById('discountForm');
    if (discount) {
        form.codigo.value = discount.codigoCupon || '';
        form.tipo.value = discount.tipoCupon || 'Porcentaje';
        form.valor.value = discount.valor || '';
        form.limiteUsos.value = discount.limiteUsos || '';
        form.fechaInicio.value = discount.fechaInicio ? discount.fechaInicio.split('T')[0] : '';
        form.fechaFin.value = discount.fechaFin ? discount.fechaFin.split('T')[0] : '';
        form.montoMinimo.value = discount.montoMinimo || '';
        form.descripcion.value = discount.descripcion || '';
        form.estaActivo.checked = discount.estaActivo !== false;
    } else {
        form.reset();
        form.estaActivo.checked = true;
    }
    document.getElementById('discountModal').classList.remove('hidden');
}

function closeDiscountModal() {
    document.getElementById('discountModal').classList.add('hidden');
    editingDiscountId = null;
}

async function handleDiscountSubmit(e) {
    e.preventDefault();
    const formData = new FormData(e.target);
    const data = {
        codigoCupon: formData.get('codigo').toUpperCase(),
        tipoCupon: formData.get('tipo'),
        valor: parseFloat(formData.get('valor')),
        limiteUsos: formData.get('limiteUsos') ? parseInt(formData.get('limiteUsos')) : null,
        fechaInicio: formData.get('fechaInicio') || null,
        fechaFin: formData.get('fechaFin') || null,
        montoMinimo: formData.get('montoMinimo') ? parseFloat(formData.get('montoMinimo')) : null,
        descripcion: formData.get('descripcion'),
        estaActivo: formData.get('estaActivo') === 'on',
    };

    const btn = document.getElementById('saveBtn');
    ErrorHandler.setLoading(btn, true);

    try {
        const response = editingDiscountId
            ? await API.discounts.update(editingDiscountId, data)
            : await API.discounts.create(data);

        ErrorHandler.setLoading(btn, false);

        if (response.success) {
            ErrorHandler.showToast(editingDiscountId ? 'Descuento actualizado' : 'Descuento creado', 'success');
            closeDiscountModal();
            await loadDiscounts();
        } else {
            ErrorHandler.handleApiError(response, e.target);
        }
    } catch (error) {
        ErrorHandler.setLoading(btn, false);
        console.error('Error saving discount:', error);
        ErrorHandler.showToast('Error al guardar descuento', 'error');
    }
}

function editDiscount(id) {
    const discount = discounts.find(d => d.idCupon === id);
    if (discount) openDiscountModal(discount);
}

async function deleteDiscount(id) {
    if (!confirm('¿Eliminar este descuento?')) return;
    try {
        const response = await API.discounts.delete(id);
        if (response.success) {
            ErrorHandler.showToast('Descuento eliminado', 'success');
            await loadDiscounts();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error deleting discount:', error);
        ErrorHandler.showToast('Error al eliminar descuento', 'error');
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
    if (confirm('¿Cerrar sesión?')) Auth.logout();
}

document.addEventListener('DOMContentLoaded', initAdminDiscountsPage);