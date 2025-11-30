let allFaqs = [];
let currentCategory = 'all';

document.addEventListener('DOMContentLoaded', () => {
    loadComponents();
    loadFaqs();
    setupSearch();
});

function loadComponents() {
    document.getElementById('navbar').innerHTML = Components.navbar();
    document.getElementById('footer').innerHTML = Components.footer();
    document.getElementById('mobileMenu').innerHTML = Components.mobileMenu();
    if (Auth.isAuthenticated()) Components.updateCartBadge();
}

async function loadFaqs() {
    try {
        const response = await API.faqs.getAll();
        if (response.success && response.data) {
            allFaqs = response.data;
            renderFaqs();
        }
    } catch (error) {
        document.getElementById('faqList').innerHTML = '<p class="text-center text-gray-500">Error al cargar preguntas</p>';
    }
}

function renderFaqs(searchTerm = '') {
    let filtered = allFaqs;
    
    if (currentCategory !== 'all') {
        filtered = filtered.filter(f => f.categoria?.toLowerCase() === currentCategory);
    }
    
    if (searchTerm) {
        filtered = filtered.filter(f => 
            f.pregunta.toLowerCase().includes(searchTerm.toLowerCase()) ||
            f.respuesta.toLowerCase().includes(searchTerm.toLowerCase())
        );
    }

    const html = filtered.length ? filtered.map(faq => `
        <details class="group bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700">
            <summary class="flex cursor-pointer items-center justify-between p-4 font-medium">
                <span>${faq.pregunta}</span>
                <span class="material-icons transition-transform group-open:rotate-180">expand_more</span>
            </summary>
            <div class="px-4 pb-4 text-gray-600 dark:text-gray-400">${faq.respuesta}</div>
        </details>
    `).join('') : '<p class="text-center text-gray-500 py-8">No se encontraron resultados</p>';
    
    document.getElementById('faqList').innerHTML = html;
}

function filterByCategory(category) {
    currentCategory = category;
    document.querySelectorAll('.category-btn').forEach(btn => {
        btn.classList.remove('active', 'bg-primary', 'text-white');
        btn.classList.add('bg-gray-200', 'dark:bg-gray-800');
    });
    event.target.classList.add('active', 'bg-primary', 'text-white');
    event.target.classList.remove('bg-gray-200', 'dark:bg-gray-800');
    renderFaqs(document.getElementById('faqSearch').value);
}

function setupSearch() {
    document.getElementById('faqSearch').addEventListener('input', (e) => {
        renderFaqs(e.target.value);
    });
}