let userData = null;
let notifications = [];
let currentPage = 1;
const pageSize = 20;
let hasMore = true;

async function initNotificationsPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadUserData();
    await loadNotifications();
    
    // Setup infinite scroll
    setupInfiniteScroll();
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

async function loadNotifications(append = false) {
    const container = document.getElementById('notificationsContainer');
    
    if (!append) {
        container.innerHTML = createLoadingSpinner();
    }

    try {
        const response = await API.notifications.getAll();

        if (response.success && response.data) {
            if (!append) {
                notifications = response.data;
            } else {
                notifications = [...notifications, ...response.data];
            }
            
            renderNotifications();
            updateNotificationCounts();
        } else {
            ErrorHandler.handleApiError(response);
            if (!append) {
                container.innerHTML = createErrorState('No se pudieron cargar las notificaciones');
            }
        }
    } catch (error) {
        console.error('Load notifications error:', error);
        ErrorHandler.handleNetworkError(error);
        if (!append) {
            container.innerHTML = createErrorState('Error de conexión al cargar notificaciones');
        }
    }
}

function renderNotifications() {
    const container = document.getElementById('notificationsContainer');
    
    if (notifications.length === 0) {
        container.innerHTML = `
            <div class="text-center py-12">
                <span class="material-symbols-outlined text-6xl text-gray-400 mb-4">notifications_off</span>
                <p class="text-gray-500 dark:text-gray-400">No tienes notificaciones</p>
            </div>
        `;
        return;
    }
    
    // Group notifications by date
    const grouped = groupByDate(notifications);
    
    let html = '';
    for (const [date, items] of Object.entries(grouped)) {
        html += `
            <div class="mb-6">
                <h3 class="text-sm font-semibold text-gray-500 dark:text-gray-400 mb-3">${date}</h3>
                <div class="space-y-2">
                    ${items.map(notif => createNotificationItem(notif)).join('')}
                </div>
            </div>
        `;
    }
    
    container.innerHTML = html;
}

function groupByDate(notifications) {
    const groups = {};
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);
    
    notifications.forEach(notif => {
        const date = new Date(notif.fechaCreacion);
        let key;
        
        if (isSameDay(date, today)) {
            key = 'Hoy';
        } else if (isSameDay(date, yesterday)) {
            key = 'Ayer';
        } else if (isThisWeek(date)) {
            key = 'Esta semana';
        } else {
            key = date.toLocaleDateString('es-ES', { month: 'long', year: 'numeric' });
        }
        
        if (!groups[key]) groups[key] = [];
        groups[key].push(notif);
    });
    
    return groups;
}

function isSameDay(d1, d2) {
    return d1.getFullYear() === d2.getFullYear() &&
           d1.getMonth() === d2.getMonth() &&
           d1.getDate() === d2.getDate();
}

function isThisWeek(date) {
    const today = new Date();
    const weekAgo = new Date(today);
    weekAgo.setDate(weekAgo.getDate() - 7);
    return date > weekAgo && date < today;
}

function createNotificationItem(notification) {
    const isUnread = !notification.leida;
    const iconMap = {
        'PEDIDO': 'shopping_bag',
        'GENERAL': 'info',
        'PROMOCION': 'local_offer',
        'SISTEMA': 'settings'
    };
    const icon = iconMap[notification.tipo] || 'notifications';
    
    return `
        <div class="flex items-start gap-4 p-4 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors ${isUnread ? 'bg-primary/5 border-l-4 border-primary' : 'border-l-4 border-transparent'}">
            <div class="flex-shrink-0">
                <div class="${isUnread ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-400'} p-2 rounded-full">
                    <span class="material-symbols-outlined text-base">${icon}</span>
                </div>
            </div>
            <div class="flex-1 min-w-0">
                ${notification.titulo ? `<h4 class="font-semibold text-sm mb-1 ${isUnread ? 'text-gray-900 dark:text-white' : 'text-gray-700 dark:text-gray-300'}">${notification.titulo}</h4>` : ''}
                <p class="text-sm ${isUnread ? 'text-gray-900 dark:text-white' : 'text-muted-light dark:text-muted-dark'}">${notification.mensaje}</p>
                <p class="text-xs text-muted-light dark:text-muted-dark mt-2">${formatTimeAgo(notification.fechaCreacion)}</p>
            </div>
            <div class="flex items-center gap-2 flex-shrink-0">
                <button onclick="toggleRead(${notification.idNotificacion}, ${isUnread})" 
                        class="text-muted-light dark:text-muted-dark hover:text-gray-900 dark:hover:text-white transition-colors"
                        title="${isUnread ? 'Marcar como leída' : 'Marcar como no leída'}">
                    <span class="material-symbols-outlined text-base">${isUnread ? 'drafts' : 'mark_email_read'}</span>
                </button>
                <button onclick="deleteNotification(${notification.idNotificacion})" 
                        class="text-muted-light dark:text-muted-dark hover:text-red-500 transition-colors"
                        title="Eliminar notificación">
                    <span class="material-symbols-outlined text-base">delete</span>
                </button>
            </div>
        </div>
    `;
}

function formatTimeAgo(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now - date;
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);

    if (minutes < 1) return 'Ahora mismo';
    if (minutes < 60) return `Hace ${minutes} min`;
    if (hours < 24) return `Hace ${hours} h`;
    if (days === 1) return 'Ayer';
    if (days < 7) return `Hace ${days} días`;
    
    return date.toLocaleDateString('es-ES', { 
        day: 'numeric', 
        month: 'short',
        year: date.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
    });
}

function updateNotificationCounts() {
    const unreadCount = notifications.filter(n => !n.leida).length;
    const totalCount = notifications.length;
    
    const title = document.querySelector('.lg\\:col-span-3 h1');
    if (title) {
        const countBadge = unreadCount > 0 
            ? `<span class="ml-2 bg-primary text-white text-sm px-2 py-1 rounded-full">${unreadCount}</span>`
            : '';
        title.innerHTML = `Mis Notificaciones ${countBadge}`;
    }
}

async function toggleRead(id, isUnread) {
    try {
        const response = await API.notifications.markAsRead(id);

        if (response.success) {
            // Update local state
            const notification = notifications.find(n => n.idNotificacion === id);
            if (notification) {
                notification.leida = true;
            }
            renderNotifications();
            updateNotificationCounts();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function markAllAsRead() {
    const unreadCount = notifications.filter(n => !n.leida).length;
    if (unreadCount === 0) {
        ErrorHandler.showToast('No hay notificaciones sin leer', 'info');
        return;
    }
    
    try {
        const response = await API.notifications.markAllAsRead();
        
        if (response.success) {
            ErrorHandler.showToast(`${unreadCount} notificaciones marcadas como leídas`, 'success');
            notifications.forEach(n => n.leida = true);
            renderNotifications();
            updateNotificationCounts();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function deleteNotification(id) {
    if (!confirm('¿Eliminar esta notificación?')) return;
    
    try {
        const response = await API.notifications.delete(id);
        
        if (response.success) {
            ErrorHandler.showToast('Notificación eliminada', 'info');
            notifications = notifications.filter(n => n.idNotificacion !== id);
            renderNotifications();
            updateNotificationCounts();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function setupInfiniteScroll() {
    const container = document.getElementById('notificationsContainer');
    const observer = new IntersectionObserver((entries) => {
        if (entries[0].isIntersecting && hasMore) {
            // Load more if we implement pagination in backend
        }
    }, { threshold: 0.1 });
    
    // Observe the last notification
    if (container.lastElementChild) {
        observer.observe(container.lastElementChild);
    }
}

function openSettings() {
    ErrorHandler.showToast('Configuración de notificaciones próximamente', 'info');
}

function openMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
    const currentPath = window.location.pathname;
    
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
                <button onclick="closeMobileProfileMenu()" class="p-2 hover:bg-gray-100 dark:hover:bg-gray-700/50 rounded-lg">
                    <span class="material-symbols-outlined">close</span>
                </button>
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
                    ${menuItems.map(item => {
                        const isActive = currentPath.includes(item.href.split('/').pop().split('.')[0]);
                        return `
                            <a href="${item.href}" 
                               class="flex items-center space-x-3 px-4 py-2.5 rounded-md ${isActive ? 'bg-primary text-white' : 'hover:bg-gray-100 dark:hover:bg-gray-700/50 text-muted-light dark:text-muted-dark hover:text-gray-900 dark:hover:text-white'} text-sm font-medium">
                                <span class="material-symbols-outlined text-base">${item.icon}</span>
                                <span>${item.label}</span>
                            </a>
                        `;
                    }).join('')}
                </nav>

                <div class="border-t border-border-light dark:border-border-dark mt-6 pt-4">
                    <button onclick="logout()" class="flex items-center space-x-3 px-4 py-2.5 rounded-md hover:bg-red-50 dark:hover:bg-red-900/20 text-danger text-sm font-medium w-full">
                        <span class="material-symbols-outlined text-base">logout</span>
                        <span>Cerrar Sesión</span>
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
    
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeMobileProfileMenu();
    });
}

function closeMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
    modal.classList.add('hidden');
    document.body.style.overflow = '';
}

function logout() {
    if (confirm('¿Estás seguro de cerrar sesión?')) Auth.logout();
}

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) {
        closeMobileProfileMenu();
    }
});

initNotificationsPage();