document.addEventListener('DOMContentLoaded', () => {
    loadComponents();
    setupContactForm();
});

function loadComponents() {
    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').innerHTML = Components.mobileMenu();
    
    if (Auth.isAuthenticated()) {
        Components.updateCartBadge();
    }
}

function setupContactForm() {
    const form = document.getElementById('contactForm');
    const submitBtn = document.getElementById('submitBtn');
    
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const formData = new FormData(form);
        const data = {
            nombre: formData.get('nombre'),
            email: formData.get('email'),
            motivo: formData.get('motivo'),
            mensaje: formData.get('mensaje')
        };
        
        ErrorHandler.setLoading(submitBtn, true);
        
        try {
            const response = await API.contact.send(data);
            
            if (response.success) {
                ErrorHandler.showToast('Mensaje enviado correctamente. Te contactaremos pronto.', 'success');
                form.reset();
            } else {
                ErrorHandler.handleApiError(response);
            }
        } catch (error) {
            console.error('Error sending contact form:', error);
            ErrorHandler.showToast('Error al enviar mensaje. Inténtalo más tarde.', 'error');
        } finally {
            ErrorHandler.setLoading(submitBtn, false);
        }
    });
}