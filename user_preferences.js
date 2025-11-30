let userData = null;
let preferences = {};

async function initPreferencesPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadUserData();
    await loadPreferences();
}

async function loadUserData() {
    const response = await API.users.getProfile();
    if (response.success && response.data) {
        userData = response.data;
        const fullName = `${userData.nombre} ${userData.apellido || ''}`.trim();
        document.getElementById('userName').textContent = fullName;
        document.getElementById('userEmail').textContent = userData.email;
        const avatarUrl = userData.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff`;
        document.getElementById('userAvatar').src = avatarUrl;
    }
}

async function loadPreferences() {
    const response = await API.preferences.get();

    if (response.success && response.data) {
        preferences = response.data;
        populatePreferences();
    } else {
        // Load defaults
        preferences = {
            theme: 'dark',
            language: 'es',
            currency: 'USD',
            emailNotifications: true,
            promoNotifications: false,
            stockNotifications: true,
            publicProfile: false,
            shareData: false
        };
        populatePreferences();
    }
}

function populatePreferences() {
    document.getElementById('themeSelect').value = preferences.theme || 'dark';
    document.getElementById('languageSelect').value = preferences.language || 'es';
    document.getElementById('currencySelect').value = preferences.currency || 'USD';
    document.getElementById('emailNotifications').checked = preferences.emailNotifications !== false;
    document.getElementById('promoNotifications').checked = preferences.promoNotifications === true;
    document.getElementById('stockNotifications').checked = preferences.stockNotifications !== false;
    document.getElementById('publicProfile').checked = preferences.publicProfile === true;
    document.getElementById('shareData').checked = preferences.shareData === true;
}

async function savePreferences() {
    const btn = document.getElementById('saveBtn');
    ErrorHandler.setLoading(btn, true);

    const data = {
        theme: document.getElementById('themeSelect').value,
        language: document.getElementById('languageSelect').value,
        currency: document.getElementById('currencySelect').value,
        emailNotifications: document.getElementById('emailNotifications').checked,
        promoNotifications: document.getElementById('promoNotifications').checked,
        stockNotifications: document.getElementById('stockNotifications').checked,
        publicProfile: document.getElementById('publicProfile').checked,
        shareData: document.getElementById('shareData').checked
    };

    const response = await API.preferences.update(data);

    ErrorHandler.setLoading(btn, false);

    if (response.success) {
        ErrorHandler.showToast('Preferencias actualizadas correctamente', 'success');
        preferences = data;

        // Apply theme immediately
        if (data.theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else if (data.theme === 'light') {
            document.documentElement.classList.remove('dark');
        }
    } else {
        ErrorHandler.handleApiError(response);
    }
}

function resetPreferences() {
    if (!confirm('¿Restablecer todas las preferencias a los valores predeterminados?')) return;

    preferences = {
        theme: 'dark',
        language: 'es',
        currency: 'USD',
        emailNotifications: true,
        promoNotifications: false,
        stockNotifications: true,
        publicProfile: false,
        shareData: false
    };

    populatePreferences();
    ErrorHandler.showToast('Preferencias restablecidas. Guarda los cambios para aplicarlos.', 'info');
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
                        <a href="${item.href}" class="flex items-center space-x-3 px-4 py-2.5 rounded-md ${window.location.pathname.includes(item.href.split('/').pop().split('.')[0]) ? 'bg-primary text-white' : 'hover:bg-gray-100 dark:hover:bg-gray-700/50 text-muted-light dark:text-muted-dark'} text-sm font-medium">
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
        document.getElementById('modalUserEmail').textContent = userData.email;
    }

    modal.classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeMobileProfileMenu() {
    const modal = document.getElementById('mobileProfileModal');
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = '';
    }
}

function logout() {
    if (confirm('¿Estás seguro de cerrar sesión?')) Auth.logout();
}

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) closeMobileProfileMenu();
});

initPreferencesPage();