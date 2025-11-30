let userData = null;
let addresses = [];
let editingAddressId = null;

// Location data
let paises = [];
let estados = [];
let ciudades = [];
let googlePlacesEnabled = false;

// Mode: 'dropdown' or 'google-places' or 'manual'
let addressEntryMode = 'dropdown';

async function initAddressesPage() {
    if (!Auth.requireAuth()) return;

    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').outerHTML = `<div id="mobileMenu">${Components.mobileMenu()}</div>`;

    Components.updateCartBadge();
    
    // Check if Google Places is available
    checkGooglePlacesAvailability();
    
    await loadUserData();
    await loadLocationData(); // Load países/estados/ciudades for dropdowns
    await loadAddresses();

    document.getElementById('addressForm').addEventListener('submit', handleAddressSubmit);
    setupModeToggle();
    
    // Initialize Google Places if available
    if (googlePlacesEnabled) {
        setTimeout(initGooglePlaces, 500);
    }
}

function checkGooglePlacesAvailability() {
    googlePlacesEnabled = typeof google !== 'undefined' && google.maps && google.maps.places;
    console.log('Google Places API:', googlePlacesEnabled ? 'Available' : 'Not available');
}

async function loadUserData() {
    try {
        const response = await API.users.getProfile();
        
        if (response.success && response.data) {
            userData = response.data;
            populateForm();
        } else {
            ErrorHandler.handleApiError(response);
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


/**
 * Load location data for dropdowns (País/Estado/Ciudad)
 */
async function loadLocationData() {
    try {
        // Load países (countries)
        const paisesResponse = await fetch(`${CONFIG.API_URL}/location/paises`);
        if (paisesResponse.ok) {
            paises = await paisesResponse.json();
        } else {
            // Fallback: Use default México
            paises = [{idPais: 1, nombrePais: 'México'}];
        }
        
        // Load estados for México by default
        if (paises.length > 0) {
            await loadEstados(paises[0].idPais);
        }
    } catch (error) {
        console.warn('Could not load location data, using manual entry mode', error);
        addressEntryMode = 'manual';
    }
}

async function loadEstados(paisId) {
    try {
        const response = await fetch(`${CONFIG.API_URL}/location/paises/${paisId}/estados`);
        if (response.ok) {
            estados = await response.json();
        }
    } catch (error) {
        console.warn('Could not load estados', error);
    }
}

async function loadCiudades(estadoId) {
    try {
        const response = await fetch(`${CONFIG.API_URL}/location/estados/${estadoId}/ciudades`);
        if (response.ok) {
            ciudades = await response.json();
        }
    } catch (error) {
        console.warn('Could not load ciudades', error);
    }
}

async function loadAddresses() {
    const container = document.getElementById('addressesContainer');
    container.innerHTML = createLoadingSpinner();

    try {
        const response = await API.addresses.getAll();

        if (response.success && response.data) {
            addresses = response.data;
            renderAddresses();
        } else {
            ErrorHandler.handleApiError(response);
            container.innerHTML = createErrorState('No se pudieron cargar las direcciones');
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
        container.innerHTML = createErrorState('Error de conexión al cargar direcciones');
    }
}

function renderAddresses() {
    const container = document.getElementById('addressesContainer');
    
    if (addresses.length === 0) {
        container.innerHTML = `
            <div class="col-span-2 text-center py-12">
                <span class="material-symbols-outlined text-6xl text-gray-400 mb-4">location_off</span>
                <p class="text-gray-500 dark:text-gray-400 mb-4">No tienes direcciones guardadas</p>
                <button onclick="openAddressModal()" class="px-6 py-2 bg-primary text-white rounded-lg hover:bg-primary-hover">
                    Agregar Dirección
                </button>
            </div>
        `;
    } else {
        container.innerHTML = addresses.map(addr => createAddressCard(addr)).join('');
    }
}

function createAddressCard(address) {
    // Build complete address string
    const addressParts = [];
    if (address.calle) addressParts.push(address.calle);
    if (address.numeroExterior) addressParts.push(`#${address.numeroExterior}`);
    if (address.colonia) addressParts.push(address.colonia);
    
    const fullAddress = addressParts.join(', ');
    const location = `${address.ciudad}, ${address.estado} ${address.codigoPostal}`;
    
    // Show coordinates badge if available
    const coordsBadge = address.tieneCoordinadas 
        ? '<span class="inline-block bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 text-xs px-2 py-1 rounded ml-2" title="Con ubicación GPS">📍 GPS</span>'
        : '';
    
    return `
        <div class="bg-surface-light dark:bg-surface-dark p-6 rounded-lg border ${address.esPrincipal ? 'border-primary' : 'border-border-light dark:border-border-dark'}">
            ${address.esPrincipal ? '<span class="inline-block bg-primary text-white text-xs font-bold px-2 py-1 rounded mb-3">Principal</span>' : ''}
            ${coordsBadge}
            <h3 class="font-semibold text-lg mb-2">${address.nombre || 'Dirección'}</h3>
            <p class="text-sm text-muted-light dark:text-muted-dark mb-1">${fullAddress}</p>
            <p class="text-sm text-muted-light dark:text-muted-dark mb-1">${location}</p>
            ${address.referencia ? `<p class="text-sm text-muted-light dark:text-muted-dark mb-1"><span class="font-medium">Referencia:</span> ${address.referencia}</p>` : ''}
            ${address.telefono ? `<p class="text-sm text-muted-light dark:text-muted-dark mb-4">Tel: ${address.telefono}</p>` : '<div class="mb-4"></div>'}
            <div class="flex gap-2">
                <button onclick="editAddress(${address.idDireccion})" class="flex-1 px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 rounded-lg text-sm font-medium">
                    Editar
                </button>
                ${!address.esPrincipal ? `
                    <button onclick="setDefault(${address.idDireccion})" class="px-4 py-2 bg-primary hover:bg-primary-hover text-white rounded-lg text-sm font-medium">
                        Predeterminada
                    </button>
                ` : ''}
                <button onclick="deleteAddress(${address.idDireccion})" class="px-4 py-2 text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg">
                    <span class="material-symbols-outlined">delete</span>
                </button>
            </div>
        </div>
    `;
}

/**
 * Setup mode toggle buttons (Dropdown vs Google Places vs Manual)
 */
function setupModeToggle() {
    const modalContent = document.querySelector('#addressModal .p-6');
    if (!modalContent) return;
    
    // Add mode selector at top of form
    const modeSelector = document.createElement('div');
    modeSelector.className = 'mb-4 flex gap-2';
    modeSelector.innerHTML = `
        <button type="button" onclick="switchMode('dropdown')" id="modeDropdown" 
                class="flex-1 px-3 py-2 rounded-lg text-sm font-medium ${addressEntryMode === 'dropdown' ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-700'}">
            📋 Selección
        </button>
        ${googlePlacesEnabled ? `
            <button type="button" onclick="switchMode('google-places')" id="modeGooglePlaces" 
                    class="flex-1 px-3 py-2 rounded-lg text-sm font-medium ${addressEntryMode === 'google-places' ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-700'}">
                🗺️ Google Maps
            </button>
        ` : ''}
        <button type="button" onclick="switchMode('manual')" id="modeManual" 
                class="flex-1 px-3 py-2 rounded-lg text-sm font-medium ${addressEntryMode === 'manual' ? 'bg-primary text-white' : 'bg-gray-200 dark:bg-gray-700'}">
            ✏️ Manual
        </button>
    `;
    
    const form = document.getElementById('addressForm');
    form.insertBefore(modeSelector, form.firstChild);
}

function switchMode(mode) {
    addressEntryMode = mode;
    
    // Update button styles
    document.querySelectorAll('[id^="mode"]').forEach(btn => {
        btn.classList.remove('bg-primary', 'text-white');
        btn.classList.add('bg-gray-200', 'dark:bg-gray-700');
    });
    
    const activeBtn = document.getElementById(`mode${mode.split('-').map(w => w[0].toUpperCase() + w.slice(1)).join('')}`);
    if (activeBtn) {
        activeBtn.classList.add('bg-primary', 'text-white');
        activeBtn.classList.remove('bg-gray-200', 'dark:bg-gray-700');
    }
    
    // Show/hide appropriate fields
    renderLocationFields();
}

function renderLocationFields() {
    const locationContainer = document.getElementById('locationFieldsContainer');
    if (!locationContainer) {
        // Create container if it doesn't exist
        const form = document.getElementById('addressForm');
        const container = document.createElement('div');
        container.id = 'locationFieldsContainer';
        
        // Insert after basic address fields (nombre, telefono, calle)
        const calleField = form.querySelector('[name="calle"]').parentElement.parentElement;
        calleField.after(container);
    }
    
    const container = document.getElementById('locationFieldsContainer');
    
    if (addressEntryMode === 'dropdown') {
        // DROPDOWN MODE
        container.innerHTML = `
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div>
                    <label class="block text-sm font-medium mb-1.5">País *</label>
                    <select name="paisId" onchange="handlePaisChange(this.value)" required
                            class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700">
                        <option value="">Seleccione...</option>
                        ${paises.map(p => `<option value="${p.idPais}">${p.nombrePais}</option>`).join('')}
                    </select>
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1.5">Estado *</label>
                    <select name="estadoId" onchange="handleEstadoChange(this.value)" required
                            class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700">
                        <option value="">Seleccione país primero...</option>
                    </select>
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1.5">Ciudad *</label>
                    <select name="ciudadId" required
                            class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700">
                        <option value="">Seleccione estado primero...</option>
                    </select>
                </div>
            </div>
        `;
        
        // Pre-select México if available
        if (paises.length > 0) {
            container.querySelector('[name="paisId"]').value = paises[0].idPais;
            handlePaisChange(paises[0].idPais);
        }
        
    } else if (addressEntryMode === 'google-places') {
        // GOOGLE PLACES MODE
        container.innerHTML = `
            <div>
                <label class="block text-sm font-medium mb-1.5">
                    Buscar dirección *
                    <span class="text-xs text-gray-500">(Escribe y selecciona de Google Maps)</span>
                </label>
                <input type="text" id="googlePlacesInput" name="googlePlacesSearch"
                       placeholder="Ej: Av Insurgentes 100, Monterrey"
                       class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700" />
                <input type="hidden" name="googlePlaceId" />
                <input type="hidden" name="latitud" />
                <input type="hidden" name="longitud" />
                <input type="hidden" name="ciudad" />
                <input type="hidden" name="estado" />
                <input type="hidden" name="pais" />
            </div>
            <div id="coordinatesDisplay" class="hidden mt-2 text-sm text-gray-500">
                📍 Coordenadas: <span id="coordsText"></span>
            </div>
        `;
        
        // Re-initialize Google Places on the new input
        setTimeout(() => initGooglePlaces(), 100);
        
    } else {
        // MANUAL MODE
        container.innerHTML = `
            <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <div>
                    <label class="block text-sm font-medium mb-1.5">Ciudad *</label>
                    <input type="text" name="ciudad" required
                           class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700" />
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1.5">Estado *</label>
                    <input type="text" name="estado" required
                           class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700" />
                </div>
                <div>
                    <label class="block text-sm font-medium mb-1.5">País</label>
                    <input type="text" name="pais" value="México"
                           class="w-full px-3 py-2 rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700" />
                </div>
            </div>
        `;
    }
}

async function handlePaisChange(paisId) {
    if (!paisId) return;
    
    const estadoSelect = document.querySelector('[name="estadoId"]');
    const ciudadSelect = document.querySelector('[name="ciudadId"]');
    
    estadoSelect.innerHTML = '<option value="">Cargando...</option>';
    ciudadSelect.innerHTML = '<option value="">Seleccione estado primero...</option>';
    
    await loadEstados(parseInt(paisId));
    
    estadoSelect.innerHTML = '<option value="">Seleccione...</option>' +
        estados.map(e => `<option value="${e.idEstado}">${e.nombreEstado}</option>`).join('');
}

async function handleEstadoChange(estadoId) {
    if (!estadoId) return;
    
    const ciudadSelect = document.querySelector('[name="ciudadId"]');
    ciudadSelect.innerHTML = '<option value="">Cargando...</option>';
    
    await loadCiudades(parseInt(estadoId));
    
    ciudadSelect.innerHTML = '<option value="">Seleccione...</option>' +
        ciudades.map(c => `<option value="${c.idCiudad}">${c.nombreCiudad}</option>`).join('');
}

function openAddressModal(address = null) {
    editingAddressId = address?.idDireccion || null;
    document.getElementById('modalTitle').textContent = address ? 'Editar Dirección' : 'Nueva Dirección';
    
    const form = document.getElementById('addressForm');
    ErrorHandler.clearFormErrors(form);
    
    if (address) {
        form.nombre.value = address.nombre || '';
        form.telefono.value = address.telefono || '';
        form.calle.value = address.calle || '';
        form.numeroExterior.value = address.numeroExterior || '';
        form.numeroInterior.value = address.numeroInterior || '';
        form.colonia.value = address.colonia || '';
        form.codigoPostal.value = address.codigoPostal || '';
        form.referencia.value = address.referencia || '';
        form.esPrincipal.checked = address.esPrincipal || false;
        
        // Set location based on mode
        if (addressEntryMode === 'dropdown' && address.ciudadId) {
            // TODO: Set dropdown values
        } else if (addressEntryMode === 'manual') {
            if (form.ciudad) form.ciudad.value = address.ciudad || '';
            if (form.estado) form.estado.value = address.estado || '';
            if (form.pais) form.pais.value = address.pais || '';
        }
    } else {
        form.reset();
    }
    
    setupModeToggle();
    renderLocationFields();
    
    document.getElementById('addressModal').classList.remove('hidden');
    document.body.style.overflow = 'hidden';
}

function closeAddressModal() {
    document.getElementById('addressModal').classList.add('hidden');
    document.body.style.overflow = '';
    editingAddressId = null;
}

async function handleAddressSubmit(e) {
    e.preventDefault();
    
    const formData = new FormData(e.target);
    
    // Build request based on mode
    const data = {
        nombre: formData.get('nombre')?.trim() || 'Principal',
        telefono: formData.get('telefono')?.trim(),
        calle: formData.get('calle')?.trim(),
        numeroExterior: formData.get('numeroExterior')?.trim(),
        numeroInterior: formData.get('numeroInterior')?.trim(),
        colonia: formData.get('colonia')?.trim(),
        codigoPostal: formData.get('codigoPostal')?.trim(),
        referencia: formData.get('referencia')?.trim(),
        esPrincipal: formData.get('esPrincipal') === 'on'
    };

    // Add location data based on mode
    if (addressEntryMode === 'dropdown') {
        // DROPDOWN: Send IDs
        data.paisId = parseInt(formData.get('paisId'));
        data.estadoId = parseInt(formData.get('estadoId'));
        data.ciudadId = parseInt(formData.get('ciudadId'));
        
        if (!data.ciudadId || !data.estadoId || !data.paisId) {
            ErrorHandler.showToast('Por favor seleccione país, estado y ciudad', 'error');
            return;
        }
        
    } else if (addressEntryMode === 'google-places') {
        // GOOGLE PLACES: Send strings + coordinates
        data.ciudad = formData.get('ciudad');
        data.estado = formData.get('estado');
        data.pais = formData.get('pais');
        data.googlePlaceId = formData.get('googlePlaceId');
        data.latitud = formData.get('latitud') ? parseFloat(formData.get('latitud')) : null;
        data.longitud = formData.get('longitud') ? parseFloat(formData.get('longitud')) : null;
        
        if (!data.ciudad || !data.estado) {
            ErrorHandler.showToast('Por favor seleccione una dirección de Google Maps', 'error');
            return;
        }
        
    } else {
        // MANUAL: Send strings
        data.ciudad = formData.get('ciudad')?.trim();
        data.estado = formData.get('estado')?.trim();
        data.pais = formData.get('pais')?.trim() || 'México';
        
        if (!data.ciudad || !data.estado) {
            ErrorHandler.showToast('Por favor ingrese ciudad y estado', 'error');
            return;
        }
    }

    // Validation
    if (!data.calle || !data.codigoPostal) {
        ErrorHandler.showToast('Calle y código postal son requeridos', 'error');
        return;
    }

    const btn = document.getElementById('saveAddressBtn');
    ErrorHandler.setLoading(btn, true);
    ErrorHandler.clearFormErrors(e.target);

    try {
        const response = editingAddressId 
            ? await API.addresses.update(editingAddressId, data)
            : await API.addresses.create(data);

        if (response.success) {
            ErrorHandler.showToast(
                editingAddressId ? 'Dirección actualizada exitosamente' : 'Dirección agregada exitosamente', 
                'success'
            );
            closeAddressModal();
            await loadAddresses();
        } else {
            ErrorHandler.handleApiError(response, e.target);
        }
    } catch (error) {
        console.error('Address submit error:', error);
        ErrorHandler.handleNetworkError(error);
    } finally {
        ErrorHandler.setLoading(btn, false);
    }
}

function editAddress(id) {
    const address = addresses.find(a => a.idDireccion === id);
    if (address) openAddressModal(address);
}

async function setDefault(id) {
    try {
        const response = await API.addresses.setDefault(id);
        
        if (response.success) {
            ErrorHandler.showToast('Dirección predeterminada actualizada', 'success');
            await loadAddresses();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

async function deleteAddress(id) {
    if (!confirm('¿Estás seguro de que deseas eliminar esta dirección?')) return;
    
    try {
        const response = await API.addresses.delete(id);
        
        if (response.success) {
            ErrorHandler.showToast('Dirección eliminada exitosamente', 'info');
            await loadAddresses();
        } else {
            ErrorHandler.handleApiError(response);
        }
    } catch (error) {
        ErrorHandler.handleNetworkError(error);
    }
}

/**
 * Initialize Google Places Autocomplete
 */
function initGooglePlaces() {
    if (!googlePlacesEnabled) return;

    const addressInput = document.getElementById('googlePlacesInput');
    if (!addressInput) return;

    const autocomplete = new google.maps.places.Autocomplete(addressInput, {
        types: ['address'],
        componentRestrictions: { country: 'mx' },
        fields: ['address_components', 'formatted_address', 'geometry', 'place_id']
    });

    autocomplete.addListener('place_changed', () => {
        const place = autocomplete.getPlace();
        if (!place.address_components) return;

        let street = '';
        let streetNumber = '';
        let city = '';
        let state = '';
        let country = '';
        let postalCode = '';
        let neighborhood = '';

        place.address_components.forEach(component => {
            const types = component.types;
            
            if (types.includes('street_number')) streetNumber = component.long_name;
            if (types.includes('route')) street = component.long_name;
            if (types.includes('sublocality_level_1') || types.includes('neighborhood')) neighborhood = component.long_name;
            if (types.includes('locality')) city = component.long_name;
            if (types.includes('administrative_area_level_1')) state = component.long_name;
            if (types.includes('country')) country = component.long_name;
            if (types.includes('postal_code')) postalCode = component.long_name;
        });

        // Populate visible fields
        const form = document.getElementById('addressForm');
        const fullStreet = streetNumber ? `${street} ${streetNumber}` : street;
        form.calle.value = fullStreet;
        form.colonia.value = neighborhood;
        form.codigoPostal.value = postalCode;

        // Populate hidden fields
        document.querySelector('[name="ciudad"]').value = city;
        document.querySelector('[name="estado"]').value = state;
        document.querySelector('[name="pais"]').value = country;
        document.querySelector('[name="googlePlaceId"]').value = place.place_id;
        
        // Store coordinates
        if (place.geometry && place.geometry.location) {
            const lat = place.geometry.location.lat();
            const lng = place.geometry.location.lng();
            document.querySelector('[name="latitud"]').value = lat;
            document.querySelector('[name="longitud"]').value = lng;
            
            // Show coordinates
            const coordsDisplay = document.getElementById('coordinatesDisplay');
            const coordsText = document.getElementById('coordsText');
            if (coordsDisplay && coordsText) {
                coordsText.textContent = `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
                coordsDisplay.classList.remove('hidden');
            }
        }
        
        ErrorHandler.showToast('Dirección cargada desde Google Maps', 'success');
    });
}

// [Mobile menu and other functions remain the same as before...]
function openMobileProfileMenu() { /* same as before */ }
function closeMobileProfileMenu() { /* same as before */ }
function logout() { if (confirm('¿Estás seguro de cerrar sesión?')) Auth.logout(); }

window.addEventListener('resize', () => {
    if (window.innerWidth >= 1024) {
        closeMobileProfileMenu();
    }
});

initAddressesPage();
