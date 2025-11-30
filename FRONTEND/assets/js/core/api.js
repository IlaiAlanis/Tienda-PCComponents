const API = {
    /**
     * Base API request handler with authentication & auto token refresh
     */
    async request(endpoint, options = {}) {
        const url = `${CONFIG.API_URL}${endpoint}`;
        const token = Auth.getToken();

        const headers = {
            'Content-Type': 'application/json',
            ...options.headers
        };

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const config = {
            ...options,
            headers,
            credentials: 'include' // For refresh token cookies
        };

        try {
            const controller = new AbortController();
            const timeoutId = setTimeout(() => controller.abort(), CONFIG.TIMEOUT);

            const response = await fetch(url, {
                ...config,
                signal: controller.signal
            });

            clearTimeout(timeoutId);

            const data = await response.json();

            // Handle token expiration - auto refresh
            if (data.error?.code === 105) { // TOKEN_EXPIRADO
                const refreshed = await Auth.refreshToken();
                if (refreshed) {
                    // Retry original request with new token
                    return this.request(endpoint, options);
                }
            }

            return data;

        } catch (error) {
            if (error.name === 'AbortError') {
                throw new Error('TimeoutError');
            }
            throw error;
        }
    },

    // Helper methods
    get(endpoint) {
        return this.request(endpoint, { method: 'GET' });
    },

    post(endpoint, data) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(data)
        });
    },

    put(endpoint, data) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(data)
        });
    },

    delete(endpoint, data = null) {
        return this.request(endpoint, {
            method: 'DELETE',
            body: data ? JSON.stringify(data) : null
        });
    },

    // ==================== AUTHENTICATION ====================
    auth: {
        login: (data) => API.post('/auth/login', data),
        register: (data) => API.post('/auth/register', data),
        googleLogin: (idToken) => API.post('/auth/google-login', { idToken }),
        refresh: (refreshToken) => API.post('/auth/refresh', { refreshToken }),
        logout: (refreshToken) => API.post('/auth/logout', { refreshToken }),
        requestPasswordReset: (email) => API.post('/auth/request-password-reset', { email }),
        resetPassword: (data) => API.post('/auth/reset-password', data),
        verifyEmail: (data) => API.post('/auth/verify-email', data),
        resendVerification: () => API.post('/auth/resend-verification'),
        getSessions: () => API.get('/auth/sessions'),
        revokeSession: (id) => API.post(`/auth/sessions/${id}/revoke`)
    },

    // ==================== USER PROFILE ====================
    //  user → users (to match JS files)
    users: {
        getProfile: () => API.get('/user/profile'),
        updateProfile: (data) => API.put('/user/profile', data),
        changePassword: (data) => API.post('/user/change-password', data),
        updateEmail: (data) => API.put('/user/email', data),
        deleteAccount: (password) => API.delete('/user/account', { password })
    },

    // ==================== PRODUCTS ====================
    products: {
        getAll: () => API.get('/producto'),
        getById: (id) => API.get(`/producto/${id}`),
        search: (filters) => API.post('/producto/search', filters),
        getFeatured: () => API.get('/producto/featured'),
        checkStock: (id, cantidad = 1) => API.get(`/producto/${id}/stock?cantidad=${cantidad}`),

        // Admin only
        create: (data) => API.post('/producto', data),
        update: (id, data) => API.put(`/producto/${id}`, data),
        delete: (id) => API.delete(`/producto/${id}`),

        // Images
        getImages: (id) => API.get(`/producto/${id}/images`),
        uploadImage: async (id, file) => {
            const formData = new FormData();
            formData.append('file', file);
            const token = Auth.getToken();

            const response = await fetch(`${CONFIG.API_URL}/producto/${id}/images`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData,
                credentials: 'include'
            });
            return response.json();
        },
        deleteImage: (imagenId) => API.delete(`/producto/images/${imagenId}`),
        setPrimaryImage: (imagenId) => API.put(`/producto/images/${imagenId}/set-primary`),

        // Variations
        getVariation: (id) => API.get(`/producto/variations/${id}`),
        createVariation: (data) => API.post('/producto/variations', data),
        updateVariation: (id, data) => API.put(`/producto/variations/${id}`, data),
        deleteVariation: (id) => API.delete(`/producto/variations/${id}`)
    },

    // ==================== CART ====================
    cart: {
        get: () => API.get('/carrito'),
        addItem: (data) => API.post('/carrito/items', data),
        updateItem: (itemId, data) => API.put(`/carrito/items/${itemId}`, data),
        removeItem: (itemId) => API.delete(`/carrito/items/${itemId}`),
        applyCoupon: (code) => API.post('/carrito/coupon', { codigoCupon: code }),
        removeCoupon: () => API.delete('/carrito/coupon'),
        clear: () => API.delete('/carrito')
    },

    // ==================== CHECKOUT ====================
    checkout: {
        getSummary: (direccionEnvioId) => API.post('/checkout/summary', { direccionEnvioId }),
        confirm: (data) => API.post('/checkout/confirm', data)
    },

    // ==================== ORDERS ====================
    orders: {
        getAll: (page = 1, pageSize = 10) => API.get(`/order?page=${page}&pageSize=${pageSize}`),
        getById: (id) => API.get(`/order/${id}`),
        cancel: (id) => API.post(`/order/${id}/cancel`),

        // Admin - Order Management
        updateStatus: (ordenId, data) => API.put(`/ordermanagement/${ordenId}/status`, data)
    },

    // ==================== CATEGORIES ====================
    categories: {
        getAll: () => API.get('/categoria'),
        getById: (id) => API.get(`/categoria/${id}`),
        create: (data) => API.post('/categoria', data),
        update: (id, data) => API.put(`/categoria/${id}`, data),
        delete: (id) => API.delete(`/categoria/${id}`)
    },

    // ==================== BRANDS ====================
    brands: {
        getAll: () => API.get('/marca'),
        getById: (id) => API.get(`/marca/${id}`),
        create: (data) => API.post('/marca', data),
        update: (id, data) => API.put(`/marca/${id}`, data),
        delete: (id) => API.delete(`/marca/${id}`)
    },

    // ==================== SUPPLIERS ====================
    suppliers: {
        getAll: () => API.get('/proveedor'),
        getById: (id) => API.get(`/proveedor/${id}`),
        create: (data) => API.post('/proveedor', data),
        update: (id, data) => API.put(`/proveedor/${id}`, data),
        delete: (id) => API.delete(`/proveedor/${id}`)
    },

    // ==================== ADDRESSES ====================
    addresses: {
        getAll: () => API.get('/direccion'),
        getById: (id) => API.get(`/direccion/${id}`),
        create: (data) => API.post('/direccion', data),
        update: (id, data) => API.put(`/direccion/${id}`, data),
        delete: (id) => API.delete(`/direccion/${id}`),
        setDefault: (id) => API.post(`/direccion/${id}/set-default`),

        // Google Places validation
        validatePlace: (placeId) => API.post('/address/validate-place', { placeId }),
        getPlaceDetails: (placeId) => API.get(`/address/place-details/${placeId}`),
        searchByPostalCode: (cp) => API.get(`/api/Address/search-by-postal-code/${cp}`)
    },

    // ==================== WISHLIST ====================
    wishlist: {
        get: () => API.get('/wishlist'),
        addItem: (productoId) => API.post(`/wishlist/items/${productoId}`),
        removeItem: (productoId) => API.delete(`/wishlist/items/${productoId}`)
    },

    // ==================== REVIEWS ====================
    reviews: {
        getProductReviews: (productoId, page = 1, pageSize = 10) =>
            API.get(`/review/producto/${productoId}?page=${page}&pageSize=${pageSize}`),
        create: (data) => API.post('/review', data),
        update: (id, data) => API.put(`/review/${id}`, data),
        delete: (id) => API.delete(`/review/${id}`)
    },

    // ==================== NOTIFICATIONS ====================
    notifications: {
        getAll: () => API.get('/notification'),
        markAsRead: (id) => API.put(`/notification/${id}/read`),
        markAllAsRead: () => API.put('/notification/read-all'),
        delete: (id) => API.delete(`/notification/${id}`)
    },

    // ==================== RECOMMENDATIONS ====================
    recommendations: {
        getForUser: () => API.get('/recommendation/for-you'),
        getSimilar: (productoId) => API.get(`/recommendation/similar/${productoId}`)
    },

    // ==================== NEWSLETTER ====================
    newsletter: {
        subscribe: (email) => API.post('/newsletter/subscribe', { email }),
        unsubscribe: (email) => API.post('/newsletter/unsubscribe', { email })
    },

    // ==================== RETURNS ====================
    returns: {
        request: (data) => API.post('/return', data),
        process: (id, data) => API.post(`/return/${id}/process`, data)
    },

    // ==================== REFUNDS ====================
    refunds: {
        request: (data) => API.post('/refund', data),
        process: (id) => API.post(`/refund/${id}/process`)
    },

    // ==================== INVOICES ====================
    invoices: {
        generate: (data) => API.post('/factura', data),
        getByOrder: (ordenId) => API.get(`/factura/order/${ordenId}`),
        downloadPdf: async (id) => {
            const token = Auth.getToken();
            const response = await fetch(`${CONFIG.API_URL}/factura/${id}/pdf`, {
                headers: { 'Authorization': `Bearer ${token}` },
                credentials: 'include'
            });
            return response.blob();
        },
        downloadXml: async (id) => {
            const token = Auth.getToken();
            const response = await fetch(`${CONFIG.API_URL}/factura/${id}/xml`, {
                headers: { 'Authorization': `Bearer ${token}` },
                credentials: 'include'
            });
            return response.blob();
        }
    },

    // ==================== PAYMENTS ====================
    payments: {
        // Stripe
        createStripeIntent: (ordenId, monto) =>
            API.post('/payment/stripe/create-intent', { ordenId, monto }),
        confirmStripePayment: (ordenId, paymentIntentId) =>
            API.post('/payment/stripe/confirm', { ordenId, paymentIntentId }),

        // PayPal
        createPayPalOrder: (ordenId, monto) =>
            API.post('/payment/paypal/create-order', { ordenId, monto }),
        capturePayPalOrder: (ordenId, payPalOrderId) =>
            API.post('/payment/paypal/capture', { ordenId, payPalOrderId })
    },

    // ==================== SHIPPING ====================
    shipping: {
        getQuotes: (data) => API.post('/shipping/quote', data),
        getLocalRate: (direccionId, pesoKg) =>
            API.get(`/shipping/local-rate?direccionId=${direccionId}&pesoKg=${pesoKg}`)
    },

    // ==================== ADMIN ====================
    admin: {
        getDashboard: () => API.get('/admin/dashboard'),
        getDashboardMetrics: () => API.get('/admin/dashboard/metrics'),
        getUsers: (params = {}) => {
            const query = new URLSearchParams(params).toString();
            return API.get(`/admin/users${query ? '?' + query : ''}`);
        },
        getUserDetails: (id) => API.get(`/admin/users/${id}`),
        toggleUserStatus: (id) => API.put(`/admin/users/${id}/toggle-status`),
        getAllOrders: (filters = {}) => {
            const params = new URLSearchParams();
            if (filters.search) params.append('search', filters.search);
            if (filters.status) params.append('status', filters.status);
            if (filters.page) params.append('page', filters.page);
            if (filters.pageSize) params.append('pageSize', filters.pageSize);

            const queryString = params.toString();
            return API.get(`/ordermanagement${queryString ? '?' + queryString : ''}`);
        },

        getOrderById: (id) => API.get(`/ordermanagement/${id}`),

        updateOrderStatus: (ordenId, data) => API.put(`/ordermanagement/${ordenId}/status`, data)
    },

    // ==================== REPORTS (Admin) ====================
    reports: {
        getSalesReport: (fechaInicio, fechaFin) =>
            API.get(`/report/sales?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`),
        getInventoryReport: () => API.get('/report/inventory'),
        exportSales: async (fechaInicio, fechaFin) => {
            const token = Auth.getToken();
            const response = await fetch(
                `${CONFIG.API_URL}/report/sales/export?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`,
                {
                    headers: { 'Authorization': `Bearer ${token}` },
                    credentials: 'include'
                }
            );
            return response.blob();
        }
    },

    // ==================== CONTACT ====================
    contact: {
        send: (data) => API.post('/contact', data)
    },

    // ==================== DISCOUNTS ====================
    discounts: {
        getAll: (filters = {}) => {
            const params = new URLSearchParams();
            if (filters.search) params.append('search', filters.search);
            if (filters.status) params.append('status', filters.status);
            if (filters.page) params.append('page', filters.page);
            if (filters.pageSize) params.append('pageSize', filters.pageSize);

            const queryString = params.toString();
            return API.get(`/descuento${queryString ? '?' + queryString : ''}`);
        },
        getById: (id) => API.get(`/descuento/${id}`),
        create: (data) => API.post('/descuento', data),
        update: (id, data) => API.put(`/descuento/${id}`, data),
        delete: (id) => API.delete(`/descuento/${id}`),
        validate: (codigo) => API.post('/descuento/validate', { codigo })
    },

    // ==================== FAQs ====================
    faqs: {
        getAll: () => API.get('/faq'),
        getById: (id) => API.get(`/faq/${id}`),
        create: (data) => API.post('/faq', data),
        update: (id, data) => API.put(`/faq/${id}`, data),
        delete: (id) => API.delete(`/faq/${id}`)
    },

    // ==================== PREFERENCES ====================
    preferences: {
        get: () => API.get('/preferences'),
        update: (data) => API.put('/preferences', data)
    },

    // ==================== PAYMENT METHODS ====================
    paymentMethods: {
        getAll: () => API.get('/paymentMethods'),
        create: (data) => API.post('/paymentMethods', data),
        update: (id, data) => API.put(`/paymentMethods/${id}`, data),
        delete: (id) => API.delete(`/paymentMethods/${id}`),
        setDefault: (id) => API.put(`/paymentMethods/${id}/predeterminado`)
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { API };
}