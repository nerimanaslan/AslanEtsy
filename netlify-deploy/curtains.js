// ================= ASLAN PERDE PRICING & CATALOG ENGINE =================

// Standard 13 Etsy Variations definition (Width x Length in inches and Pieces)
const ETSY_VARIATIONS = [
    { id: 1, title: 'For Sample Package', type: 'sample', widthInch: 12, lengthInch: 12, pieces: 1, isSample: true, sampleRatio: 0.304 },
    { id: 2, title: 'W39/L51 inch - 1 PC', widthInch: 39, lengthInch: 51, pieces: 1 },
    { id: 3, title: 'W59/L71 inch - 1 PC', widthInch: 59, lengthInch: 71, pieces: 1 },
    { id: 4, title: 'W71/L84 inch - 1 PC', widthInch: 71, lengthInch: 84, pieces: 1 },
    { id: 5, title: 'W79/L96 inch - 1 PC', widthInch: 79, lengthInch: 96, pieces: 1 },
    { id: 6, title: 'W87/L102 inch - 1 PC', widthInch: 87, lengthInch: 102, pieces: 1 },
    { id: 7, title: 'W98/L106 inch - 1 PC', widthInch: 98, lengthInch: 106, pieces: 1 },
    { id: 8, title: 'W118/L106 inch - 1 PC', widthInch: 118, lengthInch: 106, pieces: 1 },
    { id: 9, title: 'W59/L79 inch - 2 PCS', widthInch: 59, lengthInch: 79, pieces: 2 },
    { id: 10, title: 'W79/L98 inch - 2 PCS', widthInch: 79, lengthInch: 98, pieces: 2 },
    { id: 11, title: 'W98/L106 inch - 2 PCS', widthInch: 98, lengthInch: 106, pieces: 2 },
    { id: 12, title: 'W118/L106 inch - 2 PCS', widthInch: 118, lengthInch: 106, pieces: 2 },
    { id: 13, title: 'W138/L106 inch - 2 PCS', widthInch: 138, lengthInch: 106, pieces: 2 }
];

// Currencies definition
const CURRENCIES = {
    'TL': { symbol: '₺', name: 'Türk Lirası' },
    'USD': { symbol: '$', name: 'US Dollar' },
    'EUR': { symbol: '€', name: 'Euro' },
    'GBP': { symbol: '£', name: 'British Pound' }
};

let currentCurrency = 'TL';
let currentMobileTab = 'calculator';
let customUnit = 'inch';
let customPieces = 1;
let currentActiveModelForModal = null;
let currentSelectedImageBase64 = null;

// DOM Ready
document.addEventListener('DOMContentLoaded', () => {
    initDefaultProducts();
    calculateAllPrices();
    calculateCustomSize();
    renderProductsCatalog();
});

// ================= 1. m² TO DIMENSION CALCULATION =================
function inchToMeter(inch) {
    return inch * 0.0254;
}

function calculateAreaM2(widthInch, lengthInch, pieces = 1) {
    const widthM = inchToMeter(widthInch);
    const lengthM = inchToMeter(lengthInch);
    return widthM * lengthM * pieces;
}

function getCurtainPriceCalculation(variation, m2Price, discountRate = 30) {
    let areaM2;
    let salePrice;

    if (variation.isSample) {
        areaM2 = variation.sampleRatio || 0.304;
        salePrice = Math.round(m2Price * areaM2);
        // Base minimum for sample packaging
        if (salePrice < 300 && currentCurrency === 'TL') salePrice = 300;
    } else {
        areaM2 = calculateAreaM2(variation.widthInch, variation.lengthInch, variation.pieces);
        salePrice = Math.round(areaM2 * m2Price);
    }

    // Original list price before discount: SalePrice / (1 - (discountRate / 100))
    const discountMultiplier = 1 - (discountRate / 100);
    const originalPrice = discountMultiplier > 0 ? Math.round(salePrice / discountMultiplier) : salePrice;
    const savings = originalPrice - salePrice;

    return {
        areaM2: Number(areaM2.toFixed(2)),
        salePrice,
        originalPrice,
        savings,
        discountRate
    };
}

// Calculate all 13 prices in real-time
function calculateAllPrices() {
    const m2Input = document.getElementById('m2PriceInput');
    const discountInput = document.getElementById('discountRateInput');
    const container = document.getElementById('variationsContainer');
    
    if (!m2Input || !container) return;

    const m2Price = parseFloat(m2Input.value) || 0;
    const discountRate = parseFloat(discountInput?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    container.innerHTML = ETSY_VARIATIONS.map((v, index) => {
        const calc = getCurtainPriceCalculation(v, m2Price, discountRate);
        const widthCm = Math.round(v.widthInch * 2.54);
        const lengthCm = Math.round(v.lengthInch * 2.54);

        return `
            <div class="bg-slate-850 hover:bg-slate-800/90 transition border border-slate-800 rounded-2xl p-3.5 flex items-center justify-between shadow-sm">
                <!-- Left: Variation Name & Dimensions -->
                <div class="flex items-center space-x-3">
                    <div class="w-8 h-8 rounded-xl bg-orange-500/10 text-orange-400 font-extrabold text-xs flex items-center justify-center border border-orange-500/20">
                        ${index + 1}
                    </div>
                    <div>
                        <div class="font-bold text-white text-xs flex items-center gap-1.5">
                            <span>${v.title}</span>
                            ${v.pieces > 1 ? '<span class="text-[10px] px-1.5 py-0.2 rounded bg-indigo-500/20 text-indigo-300 font-semibold border border-indigo-500/30">Çift Kanat</span>' : ''}
                        </div>
                        <div class="text-[11px] text-slate-400 mt-0.5">
                            ${v.isSample ? 'Kumaş Numune Paketi' : `${widthCm} × ${lengthCm} cm • ${calc.areaM2} m²`}
                        </div>
                    </div>
                </div>

                <!-- Right: Prices (Original vs Discounted) -->
                <div class="text-right flex items-center gap-3">
                    <div>
                        <div class="text-[11px] text-slate-400 font-semibold line-through">
                            ${formatNumber(calc.originalPrice)} ${symbol}
                        </div>
                        <div class="text-sm font-black text-emerald-400">
                            ${formatNumber(calc.salePrice)} ${symbol}
                        </div>
                    </div>
                    <button onclick="copySingleVariationPrice('${v.title}', ${calc.originalPrice}, ${calc.salePrice})" class="p-2 text-slate-400 hover:text-orange-400 hover:bg-slate-700/60 rounded-lg transition" title="Fiyatı Kopyala">
                        <i class="fa-solid fa-copy text-xs"></i>
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

function setM2Price(price) {
    const input = document.getElementById('m2PriceInput');
    if (input) {
        input.value = price;
        calculateAllPrices();
        calculateCustomSize();
    }
}

// ================= 2. CUSTOM SIZE CALCULATOR =================
function setCustomUnit(unit) {
    customUnit = unit;
    const btnInch = document.getElementById('unitBtnInch');
    const btnCm = document.getElementById('unitBtnCm');
    const labelW = document.getElementById('customWidthUnitLabel');
    const labelL = document.getElementById('customLengthUnitLabel');

    if (unit === 'inch') {
        btnInch.className = 'px-3 py-1 bg-orange-500 text-white font-bold rounded-lg transition';
        btnCm.className = 'px-3 py-1 text-slate-400 font-medium rounded-lg hover:text-white transition';
        if (labelW) labelW.innerText = 'inch';
        if (labelL) labelL.innerText = 'inch';
    } else {
        btnCm.className = 'px-3 py-1 bg-orange-500 text-white font-bold rounded-lg transition';
        btnInch.className = 'px-3 py-1 text-slate-400 font-medium rounded-lg hover:text-white transition';
        if (labelW) labelW.innerText = 'cm';
        if (labelL) labelL.innerText = 'cm';
    }
    calculateCustomSize();
}

function setCustomPieceCount(count) {
    customPieces = count;
    const btn1 = document.getElementById('btnPiece1');
    const btn2 = document.getElementById('btnPiece2');

    if (count === 1) {
        btn1.className = 'px-4 py-1.5 bg-orange-500 text-white font-bold text-xs rounded-xl border border-orange-400 shadow';
        btn2.className = 'px-4 py-1.5 bg-slate-800 text-slate-300 font-medium text-xs rounded-xl border border-slate-700';
    } else {
        btn2.className = 'px-4 py-1.5 bg-orange-500 text-white font-bold text-xs rounded-xl border border-orange-400 shadow';
        btn1.className = 'px-4 py-1.5 bg-slate-800 text-slate-300 font-medium text-xs rounded-xl border border-slate-700';
    }
    calculateCustomSize();
}

function calculateCustomSize() {
    const wVal = parseFloat(document.getElementById('customWidthInput')?.value) || 0;
    const lVal = parseFloat(document.getElementById('customLengthInput')?.value) || 0;
    const m2Price = parseFloat(document.getElementById('m2PriceInput')?.value) || 0;
    const discountRate = parseFloat(document.getElementById('discountRateInput')?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    if (wVal <= 0 || lVal <= 0) {
        document.getElementById('customDisplayDimensions').innerText = 'Lütfen Ölçü Girin';
        document.getElementById('customDisplayMetric').innerText = 'En ve boy girildiğinde hesaplanır';
        document.getElementById('customDisplayArea').innerText = '0.00 m²';
        document.getElementById('customDisplayOriginalPrice').innerText = `0 ${symbol}`;
        document.getElementById('customDisplaySalePrice').innerText = `0 ${symbol}`;
        return;
    }

    let widthM, lengthM, widthInch, lengthInch, widthCm, lengthCm;

    if (customUnit === 'inch') {
        widthInch = wVal;
        lengthInch = lVal;
        widthM = inchToMeter(wVal);
        lengthM = inchToMeter(lVal);
        widthCm = Math.round(wVal * 2.54);
        lengthCm = Math.round(lVal * 2.54);
    } else {
        widthCm = wVal;
        lengthCm = lVal;
        widthM = wVal / 100;
        lengthM = lVal / 100;
        widthInch = Math.round(wVal / 2.54);
        lengthInch = Math.round(lVal / 2.54);
    }

    const totalAreaM2 = widthM * lengthM * customPieces;
    const salePrice = Math.round(totalAreaM2 * m2Price);
    const discountMultiplier = 1 - (discountRate / 100);
    const originalPrice = discountMultiplier > 0 ? Math.round(salePrice / discountMultiplier) : salePrice;

    document.getElementById('customDisplayDimensions').innerText = `W${widthInch} / L${lengthInch} inch (${customPieces} ${customPieces > 1 ? 'PCS' : 'PC'})`;
    document.getElementById('customDisplayMetric').innerText = `${widthCm} × ${lengthCm} cm • ${customPieces} Adet Kanat`;
    document.getElementById('customDisplayArea').innerText = `${totalAreaM2.toFixed(2)} m²`;
    document.getElementById('customDisplayOriginalPrice').innerText = `${formatNumber(originalPrice)} ${symbol}`;
    document.getElementById('customDisplaySalePrice').innerText = `${formatNumber(salePrice)} ${symbol}`;
}

// ================= 3. PRODUCTS & CATALOG MANAGEMENT =================
const STORAGE_KEY_PRODUCTS = 'ASLAN_PERDE_PRODUCTS';

function initDefaultProducts() {
    const existing = localStorage.getItem(STORAGE_KEY_PRODUCTS);
    if (!existing) {
        const defaultProducts = [
            {
                id: 1,
                name: 'Kırmızı Çizgili Keten Fon Perde',
                m2Price: 4000,
                fabric: '%100 Saf Pamuk & Keten Karışımı',
                note: 'Etsy Çok Satan • Rustik & Modern Çizgili Doku',
                imageUrl: 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600&auto=format&fit=crop&q=80'
            },
            {
                id: 2,
                name: 'Bergonya / Bordo Keten Dökümlü Perde',
                m2Price: 4500,
                fabric: '%100 Doğal Taşlanmış Keten',
                note: 'Özel Koyu Şarap / Cranberry Renk Tonu',
                imageUrl: 'https://images.unsplash.com/photo-1520699049698-acd2fccb8cc8?w=600&auto=format&fit=crop&q=80'
            },
            {
                id: 3,
                name: 'Naturel Bej Keten Tül Perde',
                m2Price: 3200,
                fabric: 'Doğal İpek & Keten Dokulu Tül',
                note: 'Güneş Işığını Yumuşatan Ferah Doku',
                imageUrl: 'https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=600&auto=format&fit=crop&q=80'
            }
        ];
        localStorage.setItem(STORAGE_KEY_PRODUCTS, JSON.stringify(defaultProducts));
    }
}

function getStoredProducts() {
    try {
        return JSON.parse(localStorage.getItem(STORAGE_KEY_PRODUCTS)) || [];
    } catch {
        return [];
    }
}

function saveProducts(products) {
    localStorage.setItem(STORAGE_KEY_PRODUCTS, JSON.stringify(products));
    renderProductsCatalog();
}

function renderProductsCatalog() {
    const grid = document.getElementById('productsCatalogGrid');
    if (!grid) return;

    const products = getStoredProducts();
    const symbol = CURRENCIES[currentCurrency].symbol;

    if (products.length === 0) {
        grid.innerHTML = `<div class="p-8 text-center text-slate-500 text-xs">Henüz eklenmiş perde modeli yok. "+ Model Ekle" butonuna dokunarak başlayabilirsiniz.</div>`;
        return;
    }

    grid.innerHTML = products.map(p => `
        <div class="bg-slate-850 border border-slate-800 rounded-3xl p-4 flex flex-col sm:flex-row gap-4 shadow-xl">
            <!-- Product Image -->
            <div class="w-full sm:w-32 h-36 rounded-2xl bg-slate-800 overflow-hidden relative flex-shrink-0">
                <img src="${p.imageUrl || 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600'}" alt="${p.name}" class="w-full h-full object-cover">
                <div class="absolute bottom-2 left-2 bg-slate-900/80 backdrop-blur-md px-2 py-0.5 rounded-md text-[10px] font-bold text-orange-400">
                    ${formatNumber(p.m2Price)} ${symbol} / m²
                </div>
            </div>

            <!-- Product Details -->
            <div class="flex-1 flex flex-col justify-between space-y-2">
                <div>
                    <div class="flex items-start justify-between">
                        <h3 class="font-extrabold text-white text-sm">${p.name}</h3>
                        <button onclick="deleteProduct(${p.id})" class="text-slate-500 hover:text-rose-400 text-xs p-1"><i class="fa-solid fa-trash-can"></i></button>
                    </div>
                    ${p.fabric ? `<div class="text-[11px] text-orange-400 font-semibold mt-0.5"><i class="fa-solid fa-layer-group"></i> ${p.fabric}</div>` : ''}
                    ${p.note ? `<p class="text-[11px] text-slate-400 mt-1 line-clamp-2">${p.note}</p>` : ''}
                </div>

                <!-- Action: Open Price List for this product -->
                <div class="pt-2 flex items-center gap-2">
                    <button onclick="openProductPricingModal(${p.id})" class="flex-1 py-2.5 bg-gradient-to-r from-orange-500 to-amber-500 hover:from-orange-600 text-white font-bold text-xs rounded-xl shadow flex items-center justify-center gap-1.5 transition">
                        <i class="fa-solid fa-list-ol"></i>
                        <span>13 Varyasyon Fiyatını Aç</span>
                    </button>
                    <button onclick="loadProductToCalculator(${p.id})" class="px-3 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 font-semibold text-xs rounded-xl border border-slate-700 transition" title="Hesaplayıcıya Yükle">
                        <i class="fa-solid fa-calculator"></i>
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function openNewProductModal() {
    document.getElementById('editProductId').value = '';
    document.getElementById('modalProductTitle').innerText = 'Yeni Perde Modeli Ekle';
    document.getElementById('prodNameInput').value = '';
    document.getElementById('prodM2PriceInput').value = document.getElementById('m2PriceInput').value || 4000;
    document.getElementById('prodFabricInput').value = '';
    document.getElementById('prodNoteInput').value = '';
    
    // Reset image preview
    currentSelectedImageBase64 = null;
    document.getElementById('imagePreview').classList.add('hidden');
    document.getElementById('imagePlaceholder').classList.remove('hidden');

    document.getElementById('productModal')?.classList.remove('hidden');
}

function closeProductModal() {
    document.getElementById('productModal')?.classList.add('hidden');
}

function handleImageSelected(e) {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function(event) {
        currentSelectedImageBase64 = event.target.result;
        const preview = document.getElementById('imagePreview');
        const placeholder = document.getElementById('imagePlaceholder');

        if (preview && placeholder) {
            preview.src = currentSelectedImageBase64;
            preview.classList.remove('hidden');
            placeholder.classList.add('hidden');
        }
    };
    reader.readAsDataURL(file);
}

function handleSaveProduct(e) {
    e.preventDefault();
    const name = document.getElementById('prodNameInput').value.trim();
    const m2Price = parseFloat(document.getElementById('prodM2PriceInput').value) || 0;
    const fabric = document.getElementById('prodFabricInput').value.trim();
    const note = document.getElementById('prodNoteInput').value.trim();

    if (!name || m2Price <= 0) {
        showToast('Lütfen model adı ve m² fiyatını girin!', 'warning');
        return;
    }

    const products = getStoredProducts();
    const newProduct = {
        id: Date.now(),
        name,
        m2Price,
        fabric,
        note,
        imageUrl: currentSelectedImageBase64 || 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600'
    };

    products.unshift(newProduct);
    saveProducts(products);
    closeProductModal();
    showToast('Perde modeli başarıyla kaydedildi!', 'success');
}

function deleteProduct(productId) {
    if (!confirm('Bu perde modelini silmek istediğinize emin misiniz?')) return;
    let products = getStoredProducts();
    products = products.filter(p => p.id !== productId);
    saveProducts(products);
    showToast('Model silindi.', 'info');
}

function loadProductToCalculator(productId) {
    const products = getStoredProducts();
    const prod = products.find(p => p.id === productId);
    if (!prod) return;

    document.getElementById('m2PriceInput').value = prod.m2Price;
    calculateAllPrices();
    calculateCustomSize();
    switchMobileTab('calculator');
    showToast(`"${prod.name}" için m² fiyatı yüklendi!`, 'success');
}

function openProductPricingModal(productId) {
    const products = getStoredProducts();
    const prod = products.find(p => p.id === productId);
    if (!prod) return;

    currentActiveModelForModal = prod;
    const discountRate = parseFloat(document.getElementById('discountRateInput')?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    document.getElementById('modalDetailTitle').innerText = prod.name;
    document.getElementById('modalDetailM2').innerText = `Birim Fiyat: ${formatNumber(prod.m2Price)} ${symbol} / m² (%${discountRate} İndirimli)`;

    const list = document.getElementById('modalDetailList');
    if (list) {
        list.innerHTML = ETSY_VARIATIONS.map((v, i) => {
            const calc = getCurtainPriceCalculation(v, prod.m2Price, discountRate);
            return `
                <div class="p-2.5 bg-slate-900/70 border border-slate-800 rounded-xl flex items-center justify-between">
                    <div>
                        <div class="font-bold text-white text-xs">${v.title}</div>
                        <div class="text-[10px] text-slate-400">${calc.areaM2} m²</div>
                    </div>
                    <div class="text-right">
                        <div class="text-[10px] text-slate-400 line-through">${formatNumber(calc.originalPrice)} ${symbol}</div>
                        <div class="text-xs font-black text-emerald-400">${formatNumber(calc.salePrice)} ${symbol}</div>
                    </div>
                </div>
            `;
        }).join('');
    }

    document.getElementById('variationDetailModal')?.classList.remove('hidden');
}

function closeVariationDetailModal() {
    document.getElementById('variationDetailModal')?.classList.add('hidden');
    currentActiveModelForModal = null;
}

// ================= 4. CLIPBOARD EXPORT LOGIC =================
function copyAllPricesToClipboard() {
    const m2Price = parseFloat(document.getElementById('m2PriceInput')?.value) || 0;
    const discountRate = parseFloat(document.getElementById('discountRateInput')?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    let text = `🦁 ASLAN PERDE - ETSY FİYAT LİSTESİ\n`;
    text += `Birim Fiyat: ${formatNumber(m2Price)} ${symbol}/m² | İndirim: %${discountRate}\n`;
    text += `=====================================\n\n`;

    ETSY_VARIATIONS.forEach((v, index) => {
        const calc = getCurtainPriceCalculation(v, m2Price, discountRate);
        text += `${index + 1}. ${v.title} (${calc.areaM2} m²)\n`;
        text += `   - Liste Fiyatı: ${formatNumber(calc.originalPrice)} ${symbol}\n`;
        text += `   - %${discountRate} İndirimli Satış: ${formatNumber(calc.salePrice)} ${symbol}\n\n`;
    });

    navigator.clipboard.writeText(text).then(() => {
        showToast('Tüm fiyat listesi panoya kopyalandı!', 'success');
    });
}

function copySingleVariationPrice(title, original, sale) {
    const symbol = CURRENCIES[currentCurrency].symbol;
    const text = `${title} -> Liste: ${formatNumber(original)} ${symbol} | İndirimli: ${formatNumber(sale)} ${symbol}`;
    navigator.clipboard.writeText(text).then(() => {
        showToast(`"${title}" fiyatı kopyalandı!`, 'success');
    });
}

function copyCustomPriceToClipboard() {
    const dim = document.getElementById('customDisplayDimensions').innerText;
    const area = document.getElementById('customDisplayArea').innerText;
    const original = document.getElementById('customDisplayOriginalPrice').innerText;
    const sale = document.getElementById('customDisplaySalePrice').innerText;

    const text = `Özel Ölçü: ${dim} (${area})\n- Liste Fiyatı: ${original}\n- %30 İndirimli: ${sale}`;
    navigator.clipboard.writeText(text).then(() => {
        showToast('Özel ölçü fiyatı kopyalandı!', 'success');
    });
}

function copyModelPricesToClipboard() {
    if (!currentActiveModelForModal) return;
    const prod = currentActiveModelForModal;
    const discountRate = parseFloat(document.getElementById('discountRateInput')?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    let text = `🦁 ${prod.name.toUpperCase()}\n`;
    text += `Birim: ${formatNumber(prod.m2Price)} ${symbol}/m²\n\n`;

    ETSY_VARIATIONS.forEach(v => {
        const calc = getCurtainPriceCalculation(v, prod.m2Price, discountRate);
        text += `${v.title}: ${formatNumber(calc.salePrice)} ${symbol} (Liste: ${formatNumber(calc.originalPrice)} ${symbol})\n`;
    });

    navigator.clipboard.writeText(text).then(() => {
        showToast('Model fiyat listesi kopyalandı!', 'success');
    });
}

// ================= 5. TAB SWITCHING & HELPERS =================
function switchMobileTab(tab) {
    currentMobileTab = tab;

    // Reset bottom nav icons
    document.querySelectorAll('nav button').forEach(btn => {
        btn.classList.remove('text-orange-500', 'font-bold');
        btn.classList.add('text-slate-400');
    });

    const activeNav = document.getElementById(`nav-${tab}`);
    if (activeNav) {
        activeNav.classList.add('text-orange-500', 'font-bold');
        activeNav.classList.remove('text-slate-400');
    }

    // Hide all sections
    document.getElementById('tab-calculator')?.classList.add('hidden');
    document.getElementById('tab-custom')?.classList.add('hidden');
    document.getElementById('tab-catalog')?.classList.add('hidden');

    // Show active section
    document.getElementById(`tab-${tab}`)?.classList.remove('hidden');
}

function onCurrencyChange() {
    currentCurrency = document.getElementById('currencySelect').value;
    const symbol = CURRENCIES[currentCurrency].symbol;
    document.getElementById('currencySymbolSuffix').innerText = `${symbol} / m²`;
    calculateAllPrices();
    calculateCustomSize();
    renderProductsCatalog();
}

function formatNumber(num) {
    return Number(num || 0).toLocaleString('tr-TR', { maximumFractionDigits: 0 });
}

function showToast(message, type = 'success') {
    const toast = document.getElementById('toastNotification');
    const msg = document.getElementById('toastMessage');
    const icon = document.getElementById('toastIcon');

    if (!toast || !msg) return;

    msg.innerText = message;
    if (type === 'success') icon.className = 'fa-solid fa-circle-check text-emerald-400';
    else if (type === 'warning') icon.className = 'fa-solid fa-triangle-exclamation text-amber-400';
    else icon.className = 'fa-solid fa-circle-info text-blue-400';

    toast.classList.remove('hidden');
    setTimeout(() => {
        toast.classList.add('hidden');
    }, 2800);
}
