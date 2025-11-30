let userData = null;

async function initProfilePage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    await loadUserData();
}

async function loadUserData() {
    try {
        const response = await API.users.getProfile();
        debugger;
        if (response.success && response.data) {
            userData = response.data;
            populateForm();
        } else {
            ErrorHandler.handleApiError(response);
            ErrorHandler.showToast('Error al cargar perfil', 'error');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

function populateForm() {
    //   Build full name with apellidoMaterno
    debugger;
    const nameParts = [
        userData.nombre,
        userData.apellidoPaterno,
        userData.apellidoMaterno,
    ].filter(Boolean); // Remove null/undefined values

    const fullName = nameParts.join(' ');

    document.getElementById('userName').textContent = fullName;
    document.getElementById('userEmail').textContent = userData.correo;

    const avatarUrl = userData.avatarUrl || `https://ui-avatars.com/api/?name=${encodeURIComponent(fullName)}&background=007BFF&color=fff`;
    document.getElementById('userAvatar').src = avatarUrl;

    //   Populate all fields including apellidoMaterno
    document.getElementById('profileNameUser').value = userData.nombreUsuario || '';
    document.getElementById('profileName').value = userData.nombre || '';
    document.getElementById('profileApellidoPaterno').value = userData.apellidoPaterno || '';
    document.getElementById('profileApellidoMaterno').value = userData.apellidoMaterno || '';
    document.getElementById('profileCorreo').value = userData.correo || '';

    // Show email verification status
    if (userData.correoVerificado === false) {
        showEmailVerificationBanner();
    }
}

function showEmailVerificationBanner() {
    const banner = document.createElement('div');
    banner.className = 'bg-yellow-50 dark:bg-yellow-900/20 border-l-4 border-yellow-400 p-4 mb-6';
    banner.innerHTML = `
        <div class="flex items-center">
            <span class="material-symbols-outlined text-yellow-400 mr-3">warning</span>
            <div class="flex-1">
                <p class="text-sm text-yellow-700 dark:text-yellow-400">
                    Tu correo electrónico no ha sido verificado. 
                    <button onclick="resendVerification()" class="font-semibold underline hover:no-underline">
                        Reenviar correo de verificación
                    </button>
                </p>
            </div>
        </div>
    `;

    const form = document.getElementById('profileForm');
    form.parentElement.insertBefore(banner, form);
}

async function resendVerification() {
    try {
        const response = await API.auth.resendVerification();

        if (response.success) {
            ErrorHandler.showToast('Correo de verificación enviado. Revisa tu bandeja de entrada.', 'success');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function saveProfile() {
    const profileForm = document.getElementById('profileForm');
    const passwordForm = document.getElementById('passwordForm');
    const btn = document.getElementById('saveBtn');

    ErrorHandler.setLoading(btn, true);

    //   Include apellidoMaterno in update
    const updateData = {
        nombre: document.getElementById('first-name').value.trim(),
        apellido: document.getElementById('last-name').value.trim(),
        apellidoMaterno: document.getElementById('apellido-materno').value.trim() || null,
        correo: document.getElementById('email').value.trim()
    };

    const currentPassword = document.getElementById('current-password').value;
    const newPassword = document.getElementById('new-password').value;

    //   Password validation
    if (newPassword) {
        if (!currentPassword) {
            ErrorHandler.showToast('Ingresa tu contraseña actual para cambiarla', 'error');
            ErrorHandler.setLoading(btn, false);
            return;
        }

        if (newPassword.length < 8) {
            ErrorHandler.showToast('La nueva contraseña debe tener al menos 8 caracteres', 'error');
            ErrorHandler.setLoading(btn, false);
            return;
        }

        updateData.currentPassword = currentPassword;
        updateData.newPassword = newPassword;
    }

    try {
        const response = await API.users.updateProfile(updateData);

        if (response.success) {
            ErrorHandler.showToast('Perfil actualizado correctamente', 'success');

            // Update cached user data in Auth
            if (response.data) {
                Auth.updateUser(response.data);
            }

            // Clear password fields
            passwordForm.reset();

            // Reload user data to get fresh info
            await loadUserData();
        } else {
            ErrorHandler.handleApiError(response, profileForm);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
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
        //   Full name with apellidoMaterno
        const nameParts = [
            userData.nombre,
            userData.apellido,
            userData.apellidoMaterno
        ].filter(Boolean);
        const fullName = nameParts.join(' ');

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
    if (confirm('¿Estás seguro de cerrar sesión?')) {
        Auth.logout();
    }
}

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) {
        closeMobileProfileMenu();
    }
});

initProfilePage();