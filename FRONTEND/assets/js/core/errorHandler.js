/**
 * Global Error Handler
 * Handles all error display and user feedback
 */
const ErrorHandler = {
    /**
     * Show toast notification
     * @param {string} message - Message to display
     * @param {string} type - Type: success, error, warning, info
     * @param {number} duration - Duration in ms (default: 5000)
     */
    showToast(message, type = 'info', duration = 5000) {
        const toast = document.createElement('div');
        const icons = {
            success: 'check_circle',
            error: 'error',
            warning: 'warning',
            info: 'info'
        };
        const colors = {
            success: 'bg-green-500',
            error: 'bg-red-500',
            warning: 'bg-yellow-500',
            info: 'bg-blue-500'
        };

        toast.className = `fixed bottom-4 right-4 ${colors[type]} text-white px-6 py-4 rounded-lg shadow-lg z-50 flex items-center gap-3 animate-slide-in max-w-md`;
        toast.innerHTML = `
            <span class="material-icons">${icons[type]}</span>
            <span class="flex-1">${message}</span>
            <button onclick="this.parentElement.remove()" class="hover:bg-white/20 p-1 rounded">
                <span class="material-icons text-sm">close</span>
            </button>
        `;
        document.body.appendChild(toast);

        // Auto remove
        setTimeout(() => {
            if (toast.parentElement) {
                toast.remove();
            }
        }, duration);
    },

    /**
     * Show modal dialog for critical errors
     * @param {string} title - Modal title
     * @param {string} message - Modal message
     * @param {function} onClose - Callback when modal closes
     */
    showModal(title, message, onClose = null) {
        const modal = document.createElement('div');
        modal.className = 'fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4 animate-fade-in';
        modal.innerHTML = `
            <div class="bg-white dark:bg-gray-800 rounded-lg p-6 max-w-md w-full shadow-xl animate-slide-up">
                <div class="flex items-center gap-3 mb-4">
                    <span class="material-icons text-red-500 text-3xl">error_outline</span>
                    <h3 class="text-xl font-bold text-gray-900 dark:text-white">${title}</h3>
                </div>
                <p class="text-gray-600 dark:text-gray-400 mb-6">${message}</p>
                <div class="flex justify-end gap-3">
                    <button onclick="this.closest('.fixed').remove(); ${onClose ? `(${onClose})()` : ''}" 
                            class="bg-blue-500 text-white px-6 py-2 rounded-lg hover:bg-blue-600 transition-colors">
                        Aceptar
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        // Close on overlay click
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                modal.remove();
                if (onClose) onClose();
            }
        });
    },

    /**
     * Show field-specific validation error
     * @param {HTMLElement} inputElement - Input element
     * @param {string} message - Error message
     */
    showFieldError(inputElement, message) {
        this.clearFieldError(inputElement);

        const errorDiv = document.createElement('div');
        errorDiv.className = 'text-red-500 text-sm mt-1 flex items-center gap-1';
        errorDiv.innerHTML = `
            <span class="material-icons text-sm">error</span>
            <span>${message}</span>
        `;
        errorDiv.dataset.errorMessage = 'true';

        inputElement.classList.add('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
        inputElement.parentElement.appendChild(errorDiv);
    },

    /**
     * Clear field error
     * @param {HTMLElement} inputElement - Input element
     */
    clearFieldError(inputElement) {
        const errorDiv = inputElement.parentElement.querySelector('[data-error-message]');
        if (errorDiv) errorDiv.remove();
        inputElement.classList.remove('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
    },

    /**
     * Clear all form errors
     * @param {HTMLElement} formElement - Form element
     */
    clearFormErrors(formElement) {
        formElement.querySelectorAll('[data-error-message]').forEach(el => el.remove());
        formElement.querySelectorAll('input, select, textarea').forEach(el => {
            el.classList.remove('border-red-500', 'focus:border-red-500', 'focus:ring-red-500');
        });
    },

    /**
     * Handle API error response
     * @param {object} response - API response object
     * @param {HTMLElement} formElement - Form element (optional)
     */
    handleApiError(response, formElement = null) {
        const error = response.error || {};
        const message = error.message || 'Ocurrió un error';

        // Show toast with error
        this.showToast(message, 'error');

        // If form provided, show field errors
        if (formElement && error.errors) {
            Object.keys(error.errors).forEach(field => {
                const input = formElement.querySelector(`[name="${field}"]`);
                if (input) {
                    this.showFieldError(input, error.errors[field][0]);
                }
            });
        }
    },

    /**
     * Handle error based on error code
     * @param {object} error - Error object { code, message, severity }
     */
    handleErrorCode(error) {
        const code = error.code || 9999;
        const message = error.message || 'Ha ocurrido un error inesperado';

        // Authentication errors (100-199)
        if (code >= 100 && code < 200) {
            if (code === CONFIG.ERROR_CODES.CREDENCIALES_INVALIDAS) {
                this.showToast('Correo o contraseña incorrectos', 'error');
            } else if (code === CONFIG.ERROR_CODES.USUARIO_BLOQUEADO) {
                this.showToast(message, 'warning');
            } else if (code === CONFIG.ERROR_CODES.TOKEN_EXPIRADO) {
                this.showToast('Sesión expirada. Por favor inicia sesión nuevamente.', 'warning');
                setTimeout(() => {
                    Auth.clearAuthData();
                    window.location.href = 'login.html';
                }, 2000);
            } else {
                this.showToast(message, 'error');
            }
            return;
        }

        // User errors (200-299)
        if (code >= 200 && code < 300) {
            if (code === CONFIG.ERROR_CODES.USUARIO_DUPLICADO) {
                this.showToast('Este correo ya está registrado', 'warning');
            } else {
                this.showToast(message, 'error');
            }
            return;
        }

        // System errors (9000-9999)
        if (code >= 9000) {
            this.showModal(
                'Error del Servidor',
                'Ocurrió un error en el servidor. Por favor intenta nuevamente más tarde o contacta a soporte si el problema persiste.',
                null
            );
            return;
        }

        // Generic error
        this.showToast(message, 'error');
    },

    /**
     * Handle network errors (timeout, no connection, etc.)
     * @param {Error} error - Error object
     */
    handleNetworkError(error) {
        console.error('Network error:', error);

        if (error.message === 'TimeoutError') {
            this.showToast('La solicitud tardó demasiado. Intenta de nuevo.', 'error');
        } else if (error.message.includes('Failed to fetch')) {
            this.showToast('No se pudo conectar con el servidor', 'error');
        } else {
            this.showToast('Error de conexión. Verifica tu internet.', 'error');
        }
    },

    /**
     * Set loading state on button
     * @param {HTMLElement} element - Button element
     * @param {boolean} isLoading - Loading state
     */
    setLoading(element, isLoading) {
        if (isLoading) {
            element.disabled = true;
            element.dataset.originalText = element.innerHTML;
            element.innerHTML = `
                <span class="inline-block animate-spin rounded-full h-4 w-4 border-t-2 border-b-2 border-white mr-2"></span>
                Cargando...
            `;
        } else {
            element.disabled = false;
            element.innerHTML = element.dataset.originalText || element.innerHTML;
        }
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { ErrorHandler };
}