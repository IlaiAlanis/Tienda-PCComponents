// assets/js/pages/about.js

document.addEventListener('DOMContentLoaded', () => {
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
    
    // Update cart badge
    if (Auth.isAuthenticated()) {
        Components.updateCartBadge();
    }
});