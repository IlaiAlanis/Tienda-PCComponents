// Toast notifications
function showToast(message, type = 'info') {
    const toast = document.createElement('div');
    const bgColor = type === 'success' ? 'bg-green-500' : 
                    type === 'error' ? 'bg-red-500' : 'bg-primary';
    
    toast.className = `fixed bottom-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 ${bgColor} text-white font-semibold flex items-center gap-2 animate-slide-in`;
    toast.innerHTML = `
        <span class="material-icons text-sm">${type === 'success' ? 'check_circle' : type === 'error' ? 'error' : 'info'}</span>
        <span>${message}</span>
    `;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

// Loading spinner
function createLoadingSpinner() {
    return `
        <div class="col-span-full text-center py-12">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary"></div>
        </div>
    `;
}

// Empty state
function createEmptyState(message) {
    return `
        <div class="col-span-full text-center py-12 text-gray-500 dark:text-gray-400">
            <span class="material-icons text-5xl mb-4 opacity-50">inbox</span>
            <p>${message}</p>
        </div>
    `;
}

// Error state
function createErrorState(message) {
    return `
        <div class="col-span-full text-center py-12 text-red-500">
            <span class="material-icons text-5xl mb-4">error_outline</span>
            <p>${message}</p>
        </div>
    `;
}

// Mobile menu toggle
function toggleMobileMenu() {
    const menu = document.getElementById('mobileMenu');
    const overlay = menu.firstElementChild; // Get the first child (the overlay)
    if (overlay) 
        overlay.classList.toggle('hidden');
    
}

function closeMobileMenu() {
    const menu = document.getElementById('mobileMenu');
    const overlay = menu.firstElementChild; // Get the first child (the overlay)
    if (overlay) 
        overlay.classList.add('hidden');
}

window.addEventListener('error', (event) => {
    if (event.message.includes('404') || event.message.includes('Not Found')) {
        window.location.href = '../pages/404.html';
    }
});

// Check for broken product/page IDs
function handle404(error) {
    if (error?.statusCode === 404) {
        window.location.href = '../pages/404.html';
    }
}

/**
 * Format currency (Mexican Peso)
 * @param {number} amount - Amount to format
 * @returns {string} - Formatted currency string
 */
function formatCurrency(amount) {
    return new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN'
    }).format(amount);
}

/**
 * Format date
 * @param {string|Date} date - Date to format
 * @returns {string} - Formatted date string
 */
function formatDate(date) {
    return new Intl.DateTimeFormat('es-MX', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

/**
 * Format date and time
 * @param {string|Date} date - Date to format
 * @returns {string} - Formatted date-time string
 */
function formatDateTime(date) {
    return new Intl.DateTimeFormat('es-MX', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    }).format(new Date(date));
}

/**
 * Validate email format
 * @param {string} email - Email to validate
 * @returns {boolean} - True if valid
 */
function isValidEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}

/**
 * Validate password strength
 * @param {string} password - Password to validate
 * @returns {object} - { valid: boolean, message: string }
 */
function validatePassword(password) {
    if (password.length < 8) {
        return { valid: false, message: 'La contraseña debe tener al menos 8 caracteres' };
    }
    if (!/[A-Z]/.test(password)) {
        return { valid: false, message: 'La contraseña debe contener al menos una mayúscula' };
    }
    if (!/[a-z]/.test(password)) {
        return { valid: false, message: 'La contraseña debe contener al menos una minúscula' };
    }
    if (!/[0-9]/.test(password)) {
        return { valid: false, message: 'La contraseña debe contener al menos un número' };
    }
    if (!/[@$!%*?&#]/.test(password)) {
        return { valid: false, message: 'La contraseña debe contener al menos un carácter especial (@$!%*?&#)' };
    }
    return { valid: true, message: '' };
}

/**
 * Debounce function
 * @param {function} func - Function to debounce
 * @param {number} wait - Wait time in ms
 * @returns {function} - Debounced function
 */
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

/**
 * Get query parameter from URL
 * @param {string} param - Parameter name
 * @returns {string|null} - Parameter value
 */
function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

/**
 * Set page title
 * @param {string} title - Page title
 */
function setPageTitle(title) {
    document.title = `${title} - PC Components`;
}

/**
 * Handle navbar search
 */
function handleNavbarSearch() {
    const searchInput = document.getElementById('navbarSearch') || document.getElementById('mobileNavbarSearch');
    const searchTerm = searchInput?.value?.trim();
    
    if (searchTerm) {
        window.location.href = `../pages/catalog.html?search=${encodeURIComponent(searchTerm)}`;
    }
}

/**
 * Define a function to capitalize names
 * @param {string} str - Name string
 * @returns {string} - Capitalized name
 */
function capitalizeName(str) {
  if (!str || typeof str !== "string") return "";

  return str
    .trim()
    .split(/\s+/) // divide por uno o más espacios
    .map(part => {
      // Caso inicial con punto: "m." → "M."
      if (/^[a-zA-Z]\.$/.test(part)) {
        return part.charAt(0).toUpperCase() + ".";
      }

      // Capitalización normal
      return part.charAt(0).toUpperCase() + part.slice(1).toLowerCase();
    })
    .join(" ");
}