/**
 * Main Application Entry Point
 * Initializes the application and sets up global event listeners
 */

// Initialize app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Initialize authentication
    Auth.init();
    
    // Load navbar
    const navbarContainer = document.getElementById('navbar');
    if (navbarContainer) {
        navbarContainer.innerHTML = Components.navbar();
    }
    
    // Load footer
    const footerContainer = document.getElementById('footer');
    if (footerContainer) {
        footerContainer.innerHTML = Components.footer();
    }
    
    // Load mobile menu
    const mobileMenuContainer = document.getElementById('mobileMenu');
    if (mobileMenuContainer) {
        mobileMenuContainer.innerHTML = Components.mobileMenu();
    }
    
    // Update cart badge if authenticated
    if (Auth.isAuthenticated()) {
        Components.updateCartBadge();
    }
    
    console.log('App initialized successfully');
});

// Global error handler for unhandled promise rejections
window.addEventListener('unhandledrejection', (event) => {
    console.error('Unhandled promise rejection:', event.reason);
    ErrorHandler.showToast('Ha ocurrido un error inesperado', 'error');
    event.preventDefault();
});

// Global error handler
window.addEventListener('error', (event) => {
    console.error('Global error:', event.error);
    
    // Handle 404 errors
    if (event.message && (event.message.includes('404') || event.message.includes('Not Found'))) {
        window.location.href = '404.html';
    }
});

// Detect online/offline status
window.addEventListener('online', () => {
    ErrorHandler.showToast('Conexión a internet restablecida', 'success');
});

window.addEventListener('offline', () => {
    ErrorHandler.showToast('Sin conexión a internet', 'warning');
});