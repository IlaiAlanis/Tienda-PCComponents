const CONFIG = {
    // API Configuration
    API_URL: 'http://localhost:5209/api', // Change to your actual API URL in production

    // Storage Keys
    STORAGE_KEYS: {
        ACCESS_TOKEN: 'access_token',
        REFRESH_TOKEN: 'refresh_token',
        USER_DATA: 'user_data',
        TOKEN_EXPIRY: 'token_expiry',
    },

    // API Endpoints (matches  .NET API routes exactly)
    ENDPOINTS: {
        // ==================== AUTH ====================
        LOGIN: '/auth/login',
        REGISTER: '/auth/register',
        GOOGLE_LOGIN: '/auth/google-login',
        REFRESH: '/auth/refresh',
        LOGOUT: '/auth/logout',
        REQUEST_PASSWORD_RESET: '/auth/request-password-reset',
        RESET_PASSWORD: '/auth/reset-password',
        VERIFY_EMAIL: '/auth/verify-email',
        RESEND_VERIFICATION: '/auth/resend-verification',
        GET_SESSIONS: '/auth/sessions',
        REVOKE_SESSION: (id) => `/auth/sessions/${id}/revoke`,

        // ==================== USER ====================
        USER_PROFILE: '/user/profile',
        UPDATE_PROFILE: '/user/profile',
        CHANGE_PASSWORD: '/user/change-password',
        UPDATE_EMAIL: '/user/email',
        DELETE_ACCOUNT: '/user/account',

        // ==================== ADMIN ====================
        ADMIN_DASHBOARD: '/admin/dashboard',
        ADMIN_DASHBOARD_METRICS: '/admin/dashboard/metrics',
        ADMIN_USERS: '/admin/users',
        ADMIN_USER_DETAILS: (id) => `/admin/users/${id}`,
        ADMIN_TOGGLE_USER_STATUS: (id) => `/admin/users/${id}/toggle-status`,

        // ==================== PRODUCTS ====================
        PRODUCTS: '/producto',
        PRODUCT_BY_ID: (id) => `/producto/${id}`,
        PRODUCT_SEARCH: '/producto/search',
        PRODUCT_FEATURED: '/producto/featured',
        PRODUCT_CHECK_STOCK: (id) => `/producto/${id}/stock`,
        
        // Product Images
        PRODUCT_IMAGES: (id) => `/producto/${id}/images`,
        PRODUCT_IMAGE_DELETE: (imagenId) => `/producto/images/${imagenId}`,
        PRODUCT_IMAGE_SET_PRIMARY: (imagenId) => `/producto/images/${imagenId}/set-primary`,
        
        // Product Variations
        PRODUCT_VARIATION: (id) => `/producto/variations/${id}`,
        PRODUCT_VARIATIONS: '/producto/variations',

        // ==================== CATEGORIES ====================
        CATEGORIES: '/categoria',
        CATEGORY_BY_ID: (id) => `/categoria/${id}`,

        // ==================== BRANDS ====================
        BRANDS: '/marca',
        BRAND_BY_ID: (id) => `/marca/${id}`,

        // ==================== SUPPLIERS ====================
        SUPPLIERS: '/proveedor',
        SUPPLIER_BY_ID: (id) => `/proveedor/${id}`,

        // ==================== CART ====================
        CART: '/carrito',
        CART_ITEMS: '/carrito/items',
        CART_ITEM: (id) => `/carrito/items/${id}`,
        CART_COUPON: '/carrito/coupon',

        // ==================== CHECKOUT ====================
        CHECKOUT_SUMMARY: '/checkout/summary',
        CHECKOUT_CONFIRM: '/checkout/confirm',

        // ==================== ORDERS ====================
        ORDERS: '/order',
        ORDER_BY_ID: (id) => `/order/${id}`,
        ORDER_CANCEL: (id) => `/order/${id}/cancel`,
        
        // Admin Order Management
        ORDER_UPDATE_STATUS: (id) => `/ordermanagement/${id}/status`,

        // ==================== ADDRESSES ====================
        ADDRESSES: '/direccion',
        ADDRESS_BY_ID: (id) => `/direccion/${id}`,
        ADDRESS_SET_DEFAULT: (id) => `/direccion/${id}/set-default`,
        
        // Google Places
        ADDRESS_VALIDATE_PLACE: '/address/validate-place',
        ADDRESS_PLACE_DETAILS: (placeId) => `/address/place-details/${placeId}`,

        // ==================== WISHLIST ====================
        WISHLIST: '/wishlist',
        WISHLIST_ITEM: (productId) => `/wishlist/items/${productId}`,

        // ==================== REVIEWS ====================
        REVIEWS: '/review',
        REVIEW_PRODUCT: (productId) => `/review/producto/${productId}`,
        REVIEW_BY_ID: (id) => `/review/${id}`,

        // ==================== NOTIFICATIONS ====================
        NOTIFICATIONS: '/notification',
        NOTIFICATION_READ: (id) => `/notification/${id}/read`,
        NOTIFICATION_READ_ALL: '/notification/read-all',
        NOTIFICATION_DELETE: (id) => `/notification/${id}`,

        // ==================== RECOMMENDATIONS ====================
        RECOMMENDATIONS_FOR_USER: '/recommendation/for-you',
        RECOMMENDATIONS_SIMILAR: (productId) => `/recommendation/similar/${productId}`,

        // ==================== NEWSLETTER ====================
        NEWSLETTER_SUBSCRIBE: '/newsletter/subscribe',
        NEWSLETTER_UNSUBSCRIBE: '/newsletter/unsubscribe',

        // ==================== RETURNS ====================
        RETURNS: '/return',
        RETURN_PROCESS: (id) => `/return/${id}/process`,

        // ==================== REFUNDS ====================
        REFUNDS: '/refund',
        REFUND_PROCESS: (id) => `/refund/${id}/process`,

        // ==================== INVOICES ====================
        INVOICES: '/factura',
        INVOICE_BY_ORDER: (ordenId) => `/factura/order/${ordenId}`,
        INVOICE_DOWNLOAD_PDF: (id) => `/factura/${id}/pdf`,
        INVOICE_DOWNLOAD_XML: (id) => `/factura/${id}/xml`,

        // ==================== PAYMENTS ====================
        PAYMENT_STRIPE_INTENT: '/payment/stripe/create-intent',
        PAYMENT_STRIPE_CONFIRM: '/payment/stripe/confirm',
        PAYMENT_PAYPAL_CREATE: '/payment/paypal/create-order',
        PAYMENT_PAYPAL_CAPTURE: '/payment/paypal/capture',

        // ==================== SHIPPING ====================
        SHIPPING_QUOTE: '/shipping/quote',
        SHIPPING_LOCAL_RATE: '/shipping/local-rate',

        // ==================== REPORTS (Admin) ====================
        REPORTS_SALES: '/report/sales',
        REPORTS_INVENTORY: '/report/inventory',
        REPORTS_SALES_EXPORT: '/report/sales/export',

        // ==================== CONTACT ====================
        CONTACT: '/contact',

        // ==================== DISCOUNTS (Admin CRUD) ====================
        DISCOUNTS: '/descuento',
        DISCOUNT_BY_ID: (id) => `/descuento/${id}`,
        DISCOUNT_VALIDATE: '/descuento/validate',

        // ==================== FAQs ====================
        FAQS: '/faq',
        FAQ_BY_ID: (id) => `/faq/${id}`,
    },

    // App Settings
    TIMEOUT: 30000, // 30 seconds
    TOKEN_REFRESH_THRESHOLD: 2 * 60 * 1000, // Refresh 2 minutes before expiry

    // Google OAuth (if using)
    GOOGLE_CLIENT_ID: '859202039490-h5bgpmmb1ne48kgctd5fd4ojsgnalrib.apps.googleusercontent.com', // ✅ Add your Google Client ID

    // reCAPTCHA (if using)
    RECAPTCHA_SITE_KEY: '6Lf6QhUsAAAAAJ6RtUFA6cabg13tf7ZjMci6A4MF',
    RECAPTCHA_V2_SITE_KEY: '6Lf-TRUsAAAAAMItj0DemVQi6SuijQAvzV0KsoiD',

    // Error Codes (matches your .NET API error codes)
    ERROR_CODES: {
        // General (0-99)
        OK: 0,
        PARAMETRO_INVALIDO: 2,
        FORMATO_INVALIDO: 3,
        DATOS_INCOMPLETOS: 4,
        OPERACION_NO_PERMITIDA: 5,
        REGISTRO_DUPLICADO: 6,

        // Auth (100-199)
        AUTENTICACION_REQUERIDA: 100,
        ACCESO_NO_AUTORIZADO: 101,
        CREDENCIALES_INVALIDAS: 102,
        USUARIO_BLOQUEADO: 103,
        TOKEN_INVALIDO: 104,
        TOKEN_EXPIRADO: 105,
        REFRESH_TOKEN_INVALIDO: 106,
        TOKEN_REVOKED: 107,
        OAUTH_ERROR: 109,

        // Users (200-299)
        USUARIO_NO_EXISTE: 200,
        USUARIO_DUPLICADO: 201,
        USUARIO_INACTIVO: 202,
        CONTRASENA_INCORRECTA: 203,
        ROL_NO_AUTORIZADO: 204,

        // Products (300-399)
        PRODUCTO_NO_EXISTE: 300,
        PRODUCTO_DUPLICADO: 301,
        PRODUCTO_INACTIVO: 302,
        STOCK_INSUFICIENTE: 303,

        // Suppliers (400-449)
        PROVEEDOR_NO_EXISTE: 400,
        PROVEEDOR_DUPLICADO: 401,

        // Categories (450-499)
        CATEGORIA_NO_EXISTE: 450,
        CATEGORIA_DUPLICADA: 451,

        // Brands (500-549)
        MARCA_NO_EXISTE: 500,
        MARCA_DUPLICADA: 501,

        // Cart (600-699)
        CARRITO_NO_EXISTE: 600,
        ITEM_NO_EXISTE: 601,

        // Orders (700-799)
        ORDEN_NO_EXISTE: 700,
        ORDEN_NO_CANCELABLE: 701,

        // Payments (800-849)
        PAGO_FALLIDO: 800,
        STRIPE_ERROR: 805,
        PAYPAL_ERROR: 806,

        // Discounts (850-899)
        DESCUENTO_NO_EXISTE: 850,
        CUPON_INVALIDO: 851,
        CUPON_EXPIRADO: 852,
        CUPON_NO_APLICABLE: 853,
        CUPON_DUPLICADO: 854,
        CUPON_AGOTADO: 855,

        // FAQs (900-949)
        FAQ_NO_EXISTE: 900,

        // Addresses (1000-1049)
        DIRECCION_NO_EXISTE: 1000,

        // System (9000-9999)
        ERROR_SQL: 9000,
        ERROR_TIMEOUT: 9001,
        ERROR_CONCURRENCIA: 9002,
        ERROR_DEPENDENCIA: 9003,
        ERROR_CONFIGURACION: 9004,
        ERROR_DESCONOCIDO: 9999,
    }
};

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = CONFIG;
}