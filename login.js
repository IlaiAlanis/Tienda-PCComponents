// ==================== TAB SWITCHING ====================

function switchTab(tab) {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const loginTab = document.getElementById('loginTab');
    const registerTab = document.getElementById('registerTab');

    if (tab === 'login') {
        loginForm.classList.remove('hidden');
        registerForm.classList.add('hidden');
        loginTab.classList.add('bg-primary', 'text-white', 'shadow');
        loginTab.classList.remove('text-gray-600', 'dark:text-gray-300');
        registerTab.classList.remove('bg-primary', 'text-white', 'shadow');
        registerTab.classList.add('text-gray-600', 'dark:text-gray-300');
        registerForm.reset();
        clearValidationStates();
    } else {
        registerForm.classList.remove('hidden');
        loginForm.classList.add('hidden');
        registerTab.classList.add('bg-primary', 'text-white', 'shadow');
        registerTab.classList.remove('text-gray-600', 'dark:text-gray-300');
        loginTab.classList.remove('bg-primary', 'text-white', 'shadow');
        loginTab.classList.add('text-gray-600', 'dark:text-gray-300');
        loginForm.reset();
    }
}

function clearValidationStates() {
    // Reset password requirements
    ['req-length', 'req-uppercase', 'req-lowercase', 'req-number', 'req-special'].forEach(id => {
        const element = document.getElementById(id);
        if (element) {
            element.classList.remove('text-green-500');
            element.classList.add('text-gray-500');
            const icon = element.querySelector('.material-icons');
            if (icon) icon.textContent = 'radio_button_unchecked';
        }
    });

    // Hide match error
    const matchError = document.getElementById('passwordMatchError');
    if (matchError) matchError.classList.add('hidden');

    // Remove all field errors
    document.querySelectorAll('input').forEach(input => {
        input.classList.remove('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
    });
}

// ==================== EMAIL VALIDATION ====================

function validateEmail(email) {
    const regex = /^[A-Za-z0-9._\-+!#$%&'*\/=?^`{|}~]+@[A-Za-z0-9.-]+\.(com|mx|edu|org|net|gov|com\.mx|edu\.mx|gob\.mx)$/i;
    return regex.test(email);
}

function validateEmailField(input) {
    const isValid = validateEmail(input.value);
    if (input.value && !isValid) {
        input.classList.add('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
        input.classList.remove('focus:border-primary', 'focus:ring-primary');
    } else {
        input.classList.remove('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
        input.classList.add('focus:border-primary', 'focus:ring-primary');
    }
    return isValid;
}

// ==================== PASSWORD VALIDATION ====================
const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._\-+(){}[\]=\/\\|~^,;:])[A-Za-z\d@$!%*?&#._\-+(){}[\]=\/\\|~^,;:]{8,}$/;

function validatePassword() {
    const password = document.getElementById('registerPassword')?.value || '';

    const requirements = {
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /\d/.test(password),
        special: /[@$!%*?&#._\-+(){}[\]=\/\\|~^,;:]/.test(password),
        validChars: passwordRegex.test(password)
    };

    // Update UI for each requirement
    updateRequirement('req-length', requirements.length);
    updateRequirement('req-uppercase', requirements.uppercase);
    updateRequirement('req-lowercase', requirements.lowercase);
    updateRequirement('req-number', requirements.number);
    updateRequirement('req-special', requirements.special);
    updateRequirement('req-validchars', requirements.validChars);
    // Check if password matches confirmation
    validatePasswordMatch();

    // Return true if all requirements met
    return Object.values(requirements).every(req => req);
}

function validatePasswordRequirements(password) {
    return password.length >= 8 &&
        /[A-Z]/.test(password) &&
        /[a-z]/.test(password) &&
        /\d/.test(password) &&
        /[@$!%*?&#]/.test(password);
}

function updateRequirement(id, ok) {
    const el = document.getElementById(id);
    if (!el) return;


    const icon = el.querySelector('span.material-icons');


    if (ok) {
        el.classList.remove('text-gray-500');
        el.classList.add('text-green-500');
        icon.textContent = 'check_circle';
    } else {
        el.classList.add('text-gray-500');
        el.classList.remove('text-green-500');
        icon.textContent = 'radio_button_unchecked';
    }
}

function validatePasswordMatch() {
    const password = document.getElementById('registerPassword')?.value || '';
    const confirm = document.getElementById('confirmPassword')?.value || '';
    const errorDiv = document.getElementById('passwordMatchError');
    const errorText = document.getElementById('matchErrorText');

    if (confirm.length === 0) {
        errorDiv.classList.add('hidden');
        return false;
    }

    if (password !== confirm) {
        errorDiv.classList.remove('hidden');
        errorText.textContent = "Las contraseñas no coinciden";
        return false;
    } else {
        errorDiv.classList.add('hidden');
        return true;
    }
}
// ==================== LOGIN HANDLER ====================

async function handleLogin(e) {
    e.preventDefault();

    const btn = document.getElementById('loginBtn');
    const form = e.target;

    ErrorHandler.clearFormErrors(form);
    ErrorHandler.setLoading(btn, true);

    try {
        //  Changed "password" to "contrasena" to match backend DTO
        const formData = {
            correo: form.correo.value.trim(),
            contrasena: form.password.value,  //  was "password"
        };

        const response = await API.auth.login(formData);

        ErrorHandler.setLoading(btn, false);

        if (response.success) {
            // Save authentication data
            Auth.saveAuthData(response.data);
            ErrorHandler.showToast('Inicio de sesión exitoso', 'success');

            setTimeout(() => {
                // Check for redirect parameter first
                const redirectParam = new URLSearchParams(window.location.search).get('redirect');

                if (redirectParam) {
                    // If there's a redirect parameter, use it
                    window.location.href = decodeURIComponent(redirectParam);
                } else if (Auth.isAdmin()) {
                    // Admin → Admin dashboard
                    window.location.href = '../pages/admin_dashboard.html';
                } else {
                    // Regular user → Home
                    window.location.href = '../pages/index.html';
                }
            }, 1000);
        } else {
            ErrorHandler.handleApiError(response, form);
        }
    } catch (error) {
        ErrorHandler.setLoading(btn, false);
        ErrorHandler.handleNetworkError(error);
    }
}

// ==================== REGISTER HANDLER ====================

async function handleRegister(e) {
    e.preventDefault();

    const btn = document.getElementById('registerBtn');
    const form = e.target;

    ErrorHandler.clearFormErrors(form);

    // Validate email
    if (!validateEmail(form.correo.value)) {
        ErrorHandler.showFieldError(form.correo, 'Formato de correo inválido');
        return;
    }

    // Validate password match
    if (!validatePasswordMatch()) {
        return; // Error already shown
    }

    // VALIDATE PASSWORD REQUIREMENTS
    const password = form.password.value;
    if (!validatePasswordRequirements(password)) {
        ErrorHandler.showToast('La contraseña no cumple con todos los requisitos', 'error');
        return;
    }

    ErrorHandler.setLoading(btn, true);
    debugger;
    try {
        // Now sending ALL required fields
        const formData = {
            nombre: form.nombre.value.trim(),
            nombreUsuario: form.correo.value.trim().split('@')[0],
            apellidoPaterno: form.apellidoPaterno.value.trim(),
            apellidoMaterno: form.apellidoMaterno.value.trim(),
            correo: form.correo.value.trim(),
            contrasena: form.password.value,
            confirmarContrasena: form.confirmPassword.value,
            recaptchaToken: '',  // Empty for now (can implement reCAPTCHA later)
            rolId: 2  // Default to User role
        };

        const response = await API.auth.register(formData);

        ErrorHandler.setLoading(btn, false);

        if (response.success) {
            ErrorHandler.showToast('Cuenta creada exitosamente. Revisa tu correo para verificar tu cuenta.', 'success', 6000);

            // Clear form
            form.reset();

            // Switch to login tab after 2 seconds
            setTimeout(() => {
                switchTab('login');
                const loginEmail = document.getElementById('loginEmail');
                if (loginEmail) {
                    loginEmail.value = formData.correo;
                }
            }, 2000);
        } else {
            ErrorHandler.handleApiError(response, form);
        }
    } catch (error) {
        ErrorHandler.setLoading(btn, false);
        ErrorHandler.handleNetworkError(error);
    }
}

// ==================== GOOGLE LOGIN ====================

function handleGoogleLogin() {
    // Check if Google SDK loaded
    if (typeof google === 'undefined' || !google.accounts) {
        ErrorHandler.showToast('Google Sign-In no disponible', 'warning');
        return;
    }

    try {
        google.accounts.id.initialize({
            client_id: CONFIG.GOOGLE_CLIENT_ID,
            callback: handleGoogleCallback,
            error_callback: (error) => {
                console.error('Google Sign-In Error:', error);
                ErrorHandler.showToast('Error de autenticación con Google', 'error');
            }
        });

        google.accounts.id.prompt((notification) => {
            if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
                console.warn('Google Sign-In not displayed');
            }
        });
    } catch (error) {
        console.error('Google Sign-In initialization error:', error);
        ErrorHandler.showToast('Error al inicializar Google Sign-In', 'error');
    }
}

async function handleGoogleCallback(response) {
    const loginBtn = document.getElementById('loginBtn');
    ErrorHandler.setLoading(loginBtn, true);

    try {
        const result = await API.auth.googleLogin({ idToken: response.credential });

        if (result.success) {
            Auth.saveAuthData(result.data);
            ErrorHandler.showToast('Login con Google exitoso', 'success');

            setTimeout(() => {
                const redirectParam = new URLSearchParams(window.location.search).get('redirect');

                if (redirectParam) {
                    window.location.href = decodeURIComponent(redirectParam);
                } else if (Auth.isAdmin()) {
                    window.location.href = '../pages/admin/users.html';
                } else {
                    window.location.href = '../pages/index.html';
                }
            }, 1000);
        } else {
            ErrorHandler.handleApiError(result);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(loginBtn, false);
    }
}

// ==================== INITIALIZATION ====================

function initLoginPage() {
    // Check if already authenticated
    if (Auth.isAuthenticated()) {
        const redirectUrl = new URLSearchParams(window.location.search).get('redirect');

        if (redirectUrl) {
            window.location.href = decodeURIComponent(redirectUrl);
        } else if (Auth.isAdmin()) {
            window.location.href = '../pages/admin/users.html';
        } else {
            window.location.href = '../pages/index.html';
        }
        return;
    }

    // Setup event listeners
    setupEventListeners();

    // Load components if available
    loadPageComponents();

    // Set focus on email input
    setTimeout(() => {
        const emailInput = document.getElementById('loginEmail');
        if (emailInput) emailInput.focus();
    }, 100);
}

function setupEventListeners() {
    // Form submissions
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');

    if (loginForm) {
        loginForm.addEventListener('submit', handleLogin);
    }

    if (registerForm) {
        registerForm.addEventListener('submit', handleRegister);
    }

    // Email validation on blur
    const emailInputs = document.querySelectorAll('input[type="email"]');
    emailInputs.forEach(input => {
        input.addEventListener('blur', () => validateEmailField(input));
    });

    // Password validation on input
    const registerPassword = document.getElementById('registerPassword');
    if (registerPassword) {
        registerPassword.addEventListener('input', validatePassword);
    }

    const confirmPassword = document.getElementById('confirmPassword');
    if (confirmPassword) {
        confirmPassword.addEventListener('input', validatePasswordMatch);
    }
}

function loadPageComponents() {
    // Only load if Components is defined
    if (typeof Components !== 'undefined') {
        const navbar = document.getElementById('navbar');
        const footer = document.getElementById('footer');
        const mobileMenu = document.getElementById('mobileMenu');

        if (navbar) navbar.innerHTML = Components.navbar();
        if (footer) footer.innerHTML = Components.footer();
        if (mobileMenu) mobileMenu.outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', initLoginPage);