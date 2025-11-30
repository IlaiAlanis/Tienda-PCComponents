let cart = null;
let addresses = [];
let paymentMethods = [];
let selectedAddressId = null;
let selectedPaymentId = null;
let currentStep = 0;
let autocomplete = null;

const SHIPPING_COST = 149.00;
const TAX_RATE = 0.16;

async function initCheckoutPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    setupModalListeners();
    await loadCheckoutData();
}

async function loadCheckoutData() {
    const loadingState = document.getElementById('loadingState');
    if (loadingState) loadingState.classList.remove('hidden');

    try {
        const [cartResponse, addressesResponse, paymentsResponse] = await Promise.all([
            API.cart.get(),
            API.addresses.getAll(),
            API.paymentMethods.getAll()
        ]);

        if (cartResponse.success && cartResponse.data) {
            cart = cartResponse.data;
            if (!cart.items || cart.items.length === 0) {
                ErrorHandler.showToast('Tu carrito está vacío', 'warning');
                setTimeout(() => window.location.href = '../pages/cart.html', 1500);
                return;
            }
        } else {
            ErrorHandler.handleApiError(cartResponse);
            return;
        }

        if (addressesResponse.success && addressesResponse.data) {
            addresses = addressesResponse.data;
            selectedAddressId = addresses.find(a => a.esPrincipal)?.idDireccion || addresses[0]?.idDireccion;
            renderAddresses();
        }

        if (paymentsResponse.success && paymentsResponse.data) {
            paymentMethods = paymentsResponse.data;
            selectedPaymentId = paymentMethods.find(p => p.esPrincipal)?.idMetodoPago || paymentMethods[0]?.idMetodoPago;
            renderPaymentMethods();
        }

        renderOrderSummary();
        updateProgressBar();
        showStep(0);
        
        const checkoutContent = document.getElementById('checkoutContent');
        if (checkoutContent) checkoutContent.classList.remove('hidden');

    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        setTimeout(() => window.location.href = '../pages/cart.html', 2000);
    } finally {
        const loadingState = document.getElementById('loadingState');
        if (loadingState) loadingState.classList.add('hidden');
    }
}

// ==================== GOOGLE PLACES AUTOCOMPLETE ====================
function initGooglePlacesAutocomplete() {
    const input = document.getElementById('addressSearch');
    if (!input || typeof google === 'undefined') return;

    autocomplete = new google.maps.places.Autocomplete(input, {
        types: ['address'],
        componentRestrictions: { country: 'mx' }
    });

    autocomplete.addListener('place_changed', handlePlaceSelect);
}

async function handlePlaceSelect() {
    const place = autocomplete.getPlace();
    if (!place.place_id) return;

    try {
        // Get detailed address from backend
        const response = await API.addresses.getPlaceDetails(place.place_id);
        
        if (response.success && response.data) {
            fillAddressForm(response.data);
            ErrorHandler.showToast('Dirección cargada', 'success');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        console.error('Error getting place details:', error);
        ErrorHandler.showToast('Error al obtener detalles de dirección', 'error');
    }
}

function fillAddressForm(data) {
    if (data.calle) document.getElementById('calle').value = data.calle;
    if (data.numeroExterior) document.getElementById('numeroExterior').value = data.numeroExterior;
    if (data.colonia) document.getElementById('colonia').value = data.colonia;
    if (data.codigoPostal) document.getElementById('codigoPostal').value = data.codigoPostal;
    if (data.ciudadNombre) document.getElementById('ciudad').value = data.ciudadNombre;
    if (data.estadoNombre) {
        // Try to match state name to select option
        const estadoSelect = document.getElementById('estado');
        const option = Array.from(estadoSelect.options).find(
            opt => opt.text.toLowerCase() === data.estadoNombre.toLowerCase()
        );
        if (option) estadoSelect.value = option.value;
    }
}

// Search by postal code
async function searchByPostalCode() {
    const cp = document.getElementById('codigoPostal').value.trim();
    if (cp.length < 5) {
        ErrorHandler.showToast('Ingresa un código postal válido', 'warning');
        return;
    }

    const btn = document.getElementById('searchCPBtn');
    ErrorHandler.setLoading(btn, true);

    try {
        // TODO: Backend endpoint needed
        // const response = await API.addresses.searchByPostalCode(cp);
        // For now, show message
        ErrorHandler.showToast('Búsqueda por CP próximamente', 'info');
        
        // When backend ready, uncomment:
        // if (response.success && response.data) {
        //     fillAddressForm(response.data);
        // }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

// ==================== PROGRESS BAR ====================
function updateProgressBar() {
    const steps = ['shipping', 'payment', 'confirm'];
    steps.forEach((step, index) => {
        const el = document.getElementById(`step-${step}`);
        if (!el) return;
        
        if (index < currentStep) {
            el.classList.add('step-completed');
            el.classList.remove('step-active', 'step-inactive');
        } else if (index === currentStep) {
            el.classList.add('step-active');
            el.classList.remove('step-completed', 'step-inactive');
        } else {
            el.classList.add('step-inactive');
            el.classList.remove('step-active', 'step-completed');
        }
    });
}

function showStep(step) {
    currentStep = step;
    updateProgressBar();
    
    document.getElementById('shippingSection').classList.toggle('hidden', step !== 0);
    document.getElementById('paymentSection').classList.toggle('hidden', step !== 1);
    document.getElementById('confirmSection').classList.toggle('hidden', step !== 2);
    
    // Update confirm summary
    if (step === 2) {
        updateConfirmationSummary();
    }
}

function nextStep() {
    if (currentStep === 0 && !selectedAddressId) {
        ErrorHandler.showToast('Selecciona una dirección de envío', 'warning');
        return;
    }
    if (currentStep === 1 && !selectedPaymentId) {
        ErrorHandler.showToast('Selecciona un método de pago', 'warning');
        return;
    }
    if (currentStep < 2) showStep(currentStep + 1);
}

function prevStep() {
    if (currentStep > 0) showStep(currentStep - 1);
}

function updateConfirmationSummary() {
    const addr = addresses.find(a => a.idDireccion === selectedAddressId);
    const pm = paymentMethods.find(p => p.idMetodoPago === selectedPaymentId);
    
    if (addr) {
        document.getElementById('selectedAddress').innerHTML = `
            <p><strong>${addr.etiqueta || 'Dirección'}</strong></p>
            <p>${addr.calle} ${addr.numeroExterior}</p>
            <p>${addr.colonia}, ${addr.codigoPostal}</p>
            <p>${addr.ciudadNombre}, ${addr.estadoNombre}</p>
        `;
    }
    
    if (pm) {
        document.getElementById('selectedPayment').innerHTML = `
            <p><strong>•••• •••• •••• ${pm.numeroTarjeta.slice(-4)}</strong></p>
            <p>${pm.titular}</p>
        `;
    }
}

// ==================== MODALS ====================
function setupModalListeners() {
    window.onclick = (event) => {
        if (event.target.classList.contains('modal')) {
            closeModal(event.target.id);
        }
    };
}

function openModal(modalId) {
    document.getElementById(modalId).classList.remove('hidden');
    
    // Initialize Google Places when address modal opens
    if (modalId === 'addressModal' && typeof google !== 'undefined') {
        setTimeout(() => initGooglePlacesAutocomplete(), 100);
    }
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.add('hidden');
    
    if (modalId === 'addressModal') {
        document.getElementById('addressForm').reset();
    } else if (modalId === 'paymentModal') {
        document.getElementById('paymentForm').reset();
    }
}

// ==================== ADDRESS MODAL ====================
async function saveAddress() {
    const btn = document.getElementById('saveAddressBtn');
    ErrorHandler.setLoading(btn, true);

    const data = {
        calle: document.getElementById('calle').value,
        numeroExterior: document.getElementById('numeroExterior').value,
        numeroInterior: document.getElementById('numeroInterior').value || null,
        colonia: document.getElementById('colonia').value,
        codigoPostal: document.getElementById('codigoPostal').value,
        ciudadNombre: document.getElementById('ciudad').value,
        estadoId: parseInt(document.getElementById('estado').value),
        paisId: 1,
        etiqueta: document.getElementById('etiqueta').value,
        esDefault: document.getElementById('esDefault').checked
    };

    try {
        const response = await API.addresses.create(data);
        if (response.success) {
            ErrorHandler.showToast('Dirección agregada', 'success');
            addresses.push(response.data);
            selectedAddressId = response.data.idDireccion;
            renderAddresses();
            closeModal('addressModal');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

// ==================== PAYMENT MODAL ====================
async function savePaymentMethod() {
    const btn = document.getElementById('savePaymentBtn');
    ErrorHandler.setLoading(btn, true);

    const data = {
        numeroTarjeta: document.getElementById('numeroTarjeta').value,
        titular: document.getElementById('titular').value,
        fechaVencimiento: document.getElementById('fechaVencimiento').value,
        cvv: document.getElementById('cvv').value,
        esPrincipal: document.getElementById('esPrincipal').checked
    };

    try {
        const response = await API.paymentMethods.create(data);
        if (response.success) {
            ErrorHandler.showToast('Tarjeta agregada', 'success');
            paymentMethods.push(response.data);
            selectedPaymentId = response.data.idMetodoPago;
            renderPaymentMethods();
            closeModal('paymentModal');
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

// ==================== RENDER ====================
function renderAddresses() {
    const container = document.getElementById('addressesList');
    
    if (addresses.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8">
                <p class="text-gray-500 mb-4">No tienes direcciones guardadas</p>
                <button onclick="openModal('addressModal')" class="text-primary font-medium hover:underline">
                    Agregar dirección
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = addresses.map(addr => `
        <label class="block p-4 border-2 rounded-lg cursor-pointer transition-all ${
            selectedAddressId === addr.idDireccion ? 'border-primary bg-primary/5' : 'border-gray-300 dark:border-gray-600 hover:border-primary/50'
        }">
            <input type="radio" name="address" value="${addr.idDireccion}"
                   ${selectedAddressId === addr.idDireccion ? 'checked' : ''}
                   onchange="selectAddress(${addr.idDireccion})" class="hidden"/>
            <div class="flex items-start justify-between">
                <div>
                    <p class="font-semibold mb-1">${addr.etiqueta || 'Dirección'}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">${addr.calle} ${addr.numeroExterior}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">${addr.colonia}, ${addr.codigoPostal}</p>
                    <p class="text-sm text-gray-600 dark:text-gray-400">${addr.ciudadNombre}, ${addr.estadoNombre}</p>
                </div>
                ${addr.esDefault ? '<span class="text-xs bg-primary text-white px-2 py-1 rounded">Principal</span>' : ''}
            </div>
        </label>
    `).join('');
}

function renderPaymentMethods() {
    const container = document.getElementById('paymentMethodsList');
    
    if (paymentMethods.length === 0) {
        container.innerHTML = `
            <div class="text-center py-8">
                <p class="text-gray-500 mb-4">No tienes métodos de pago guardados</p>
                <button onclick="openModal('paymentModal')" class="text-primary font-medium hover:underline">
                    Agregar tarjeta
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = paymentMethods.map(pm => {
        const maskedNumber = `•••• •••• •••• ${pm.numeroTarjeta.slice(-4)}`;
        return `
            <label class="block p-4 border-2 rounded-lg cursor-pointer transition-all ${
                selectedPaymentId === pm.idMetodoPago ? 'border-primary bg-primary/5' : 'border-gray-300 dark:border-gray-600 hover:border-primary/50'
            }">
                <input type="radio" name="payment" value="${pm.idMetodoPago}"
                       ${selectedPaymentId === pm.idMetodoPago ? 'checked' : ''}
                       onchange="selectPayment(${pm.idMetodoPago})" class="hidden"/>
                <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3">
                        <span class="material-icons text-2xl text-primary">credit_card</span>
                        <div>
                            <p class="font-semibold">${maskedNumber}</p>
                            <p class="text-sm text-gray-600 dark:text-gray-400">${pm.titular}</p>
                            <p class="text-xs text-gray-500">Vence: ${pm.fechaVencimiento}</p>
                        </div>
                    </div>
                    ${pm.esPrincipal ? '<span class="text-xs bg-primary text-white px-2 py-1 rounded">Principal</span>' : ''}
                </div>
            </label>
        `;
    }).join('');
}

function renderOrderSummary() {
    const itemsHtml = cart.items.map(item => {
        const price = item.precioUnitario || 0;
        const imageUrl = item.imagenUrl || 'https://via.placeholder.com/60x60?text=P';
        const productName = item.nombre || 'Producto sin nombre';

        return `
            <li class="flex gap-4 py-3 border-b border-gray-200 dark:border-gray-700 last:border-b-0">
                <img src="${imageUrl}" alt="${productName}" class="w-16 h-16 object-cover rounded"/>
                <div class="flex-1">
                    <p class="font-medium text-sm line-clamp-2">${productName}</p>
                    <p class="text-sm text-gray-500">Cantidad: ${item.cantidad}</p>
                </div>
                <p class="font-semibold">$${(price * item.cantidad).toFixed(2)}</p>
            </li>
        `;
    }).join('');

    document.getElementById('summaryItems').innerHTML = itemsHtml;

    const subtotal = cart.subtotal || cart.items.reduce((sum, item) => sum + (item.precioUnitario * item.cantidad), 0);
    const discount = cart.descuentoTotal || 0;
    const shipping = cart.envioTotal || (subtotal > 1000 ? 0 : SHIPPING_COST);
    const tax = cart.impuestoTotal || ((subtotal - discount + shipping) * TAX_RATE);
    const total = cart.total || (subtotal - discount + shipping + tax);

    document.getElementById('summarySubtotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('summaryShipping').textContent = shipping === 0 ? 'GRATIS' : `$${shipping.toFixed(2)}`;
    document.getElementById('summaryTax').textContent = `$${tax.toFixed(2)}`;
    document.getElementById('summaryTotal').textContent = `$${total.toFixed(2)}`;

    if (discount > 0) {
        document.getElementById('summaryDiscountRow').classList.remove('hidden');
        document.getElementById('summaryDiscount').textContent = `-$${discount.toFixed(2)}`;
    }
}

function selectAddress(addressId) {
    selectedAddressId = addressId;
    renderAddresses();
}

function selectPayment(paymentId) {
    selectedPaymentId = paymentId;
    renderPaymentMethods();
}

async function confirmOrder() {
    if (!selectedAddressId || !selectedPaymentId) {
        ErrorHandler.showToast('Completa todos los pasos', 'warning');
        return;
    }

    const btn = document.getElementById('confirmOrderBtn');
    ErrorHandler.setLoading(btn, true);

    try {
        const response = await API.checkout.confirm({
            direccionEnvioId: selectedAddressId,
            metodoPagoId: selectedPaymentId,
            notas: document.getElementById('orderNotes')?.value || ''
        });

        if (response.success && response.data) {
            ErrorHandler.showToast('¡Pedido realizado con éxito!', 'success');
            Components.updateCartBadge();
            setTimeout(() => {
                window.location.href = `../pages/order-confirmation.html?order=${response.data.idOrden}`;
            }, 1000);
        } else {
            ErrorHandler.handleApiError(response);
            ErrorHandler.setLoading(btn, false);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        ErrorHandler.setLoading(btn, false);
    }
}

initCheckoutPage();