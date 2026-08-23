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

// Standard 8 Etsy Bedding (Nevresim) Variations
const ETSY_BEDDING_VARIATIONS = [
    { id: 1, title: 'Crib IN', sizeDesc: 'Duvet: 39×55" (100×140cm) • 1x Pillow: 14×18"', multiplier: 1.0000 },
    { id: 2, title: 'Toddler IN', sizeDesc: 'Duvet: 47×59" (120×150cm) • 1x Pillow: 16×24"', multiplier: 1.0976 },
    { id: 3, title: 'Twin IN', sizeDesc: 'Duvet: 66×86" (170×220cm) • 1x Pillow: 20×30"', multiplier: 1.1957 },
    { id: 4, title: 'Double IN', sizeDesc: 'Duvet: 80×86" (200×220cm) • 2x Pillows: 20×30"', multiplier: 1.2959 },
    { id: 5, title: 'Queen IN', sizeDesc: 'Duvet: 90×90" (230×230cm) • 2x Pillows: 20×30"', multiplier: 1.4013 },
    { id: 6, title: 'King IN', sizeDesc: 'Duvet: 104×90" (260×230cm) • 2x Pillows: 20×36"', multiplier: 1.4747 },
    { id: 7, title: 'Extra 1× Pillowcase IN', sizeDesc: '1 Adet Yastık Kılıfı: 20×30" (50×75cm)', multiplier: 0.0658 },
    { id: 8, title: 'Extra 2× Pillowcase IN', sizeDesc: '2 Adet Yastık Kılıfı: 20×30" (50×75cm)', multiplier: 0.1077 }
];

// Currencies definition
const CURRENCIES = {
    'TL': { symbol: '₺', name: 'Türk Lirası' },
    'USD': { symbol: '$', name: 'US Dollar' },
    'EUR': { symbol: '€', name: 'Euro' },
    'GBP': { symbol: '£', name: 'British Pound' }
};

let currentCurrency = 'TL';
let selectedCategory = 'Curtain'; // 'Curtain' or 'Bedding'
let currentMobileTab = 'catalog';
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

function getBeddingPriceCalculation(variation, baseCribPrice, discountRate = 30) {
    const originalPrice = Math.round(baseCribPrice * variation.multiplier);
    const salePrice = Math.round(originalPrice * (1 - (discountRate / 100)));
    const savings = originalPrice - salePrice;
    return {
        originalPrice,
        salePrice,
        savings,
        discountRate
    };
}

function switchCategory(cat) {
    selectedCategory = cat;
    const curtainBtn = document.getElementById('catBtnCurtain');
    const beddingBtn = document.getElementById('catBtnBedding');
    const m2Input = document.getElementById('m2PriceInput');
    const heroPriceLabel = document.getElementById('heroPriceLabel');
    const tabLabelCalc = document.getElementById('tabLabelCalculator');

    if (cat === 'Bedding') {
        if (curtainBtn) curtainBtn.className = 'flex-1 py-2 rounded-xl text-xs font-semibold text-slate-400 hover:text-white flex items-center justify-center gap-1.5 transition';
        if (beddingBtn) beddingBtn.className = 'flex-1 py-2 rounded-xl text-xs font-extrabold bg-pink-600 text-white flex items-center justify-center gap-1.5 transition shadow';
        if (m2Input && (m2Input.value === '4000' || m2Input.value === '')) m2Input.value = '22000';
        if (heroPriceLabel) heroPriceLabel.innerText = 'CRIB BAZ TAKIM FİYATI';
        if (tabLabelCalc) tabLabelCalc.innerText = '📐 8 Yatak Ölçüsü';
    } else {
        if (curtainBtn) curtainBtn.className = 'flex-1 py-2 rounded-xl text-xs font-extrabold bg-orange-600 text-white flex items-center justify-center gap-1.5 transition shadow';
        if (beddingBtn) beddingBtn.className = 'flex-1 py-2 rounded-xl text-xs font-semibold text-slate-400 hover:text-white flex items-center justify-center gap-1.5 transition';
        if (m2Input && m2Input.value === '22000') m2Input.value = '4000';
        if (heroPriceLabel) heroPriceLabel.innerText = 'm² BİRİM FİYATI';
        if (tabLabelCalc) tabLabelCalc.innerText = '📐 13 Ölçü Tablosu';
    }

    calculateAllPrices();
    renderProductsCatalog();
}

// Calculate all variations in real-time
function calculateAllPrices() {
    const m2Input = document.getElementById('m2PriceInput');
    const discountInput = document.getElementById('discountRateInput');
    const container = document.getElementById('variationsContainer');
    
    if (!m2Input || !container) return;

    const basePrice = parseFloat(m2Input.value) || 0;
    const discountRate = parseFloat(discountInput?.value) || 30;
    const symbol = CURRENCIES[currentCurrency].symbol;

    if (selectedCategory === 'Bedding') {
        container.innerHTML = ETSY_BEDDING_VARIATIONS.map((v, index) => {
            const calc = getBeddingPriceCalculation(v, basePrice, discountRate);
            return `
                <div class="bg-slate-850 hover:bg-slate-800/90 transition border border-slate-800 rounded-2xl p-3.5 flex items-center justify-between shadow-sm">
                    <div class="flex items-center space-x-3">
                        <div class="w-8 h-8 rounded-xl bg-pink-500/15 text-pink-400 font-extrabold text-xs flex items-center justify-center border border-pink-500/30">
                            ${index + 1}
                        </div>
                        <div>
                            <div class="font-bold text-white text-xs flex items-center gap-1.5">
                                <span>${v.title}</span>
                            </div>
                            <div class="text-[11px] text-slate-400 mt-0.5">
                                ${v.sizeDesc}
                            </div>
                        </div>
                    </div>

                    <div class="text-right flex items-center gap-3">
                        <div>
                            <div class="text-[11px] text-slate-400 font-semibold line-through">
                                ${formatNumber(calc.originalPrice)} ${symbol}
                            </div>
                            <div class="text-sm font-black text-pink-400">
                                ${formatNumber(calc.salePrice)} ${symbol}
                            </div>
                        </div>
                        <button onclick="copySingleVariationPrice('${v.title}', ${calc.originalPrice}, ${calc.salePrice})" class="p-2 text-slate-400 hover:text-pink-400 hover:bg-slate-700/60 rounded-lg transition" title="Fiyatı Kopyala">
                            <i class="fa-solid fa-copy text-xs"></i>
                        </button>
                    </div>
                </div>
            `;
        }).join('');
    } else {
        container.innerHTML = ETSY_VARIATIONS.map((v, index) => {
            const calc = getCurtainPriceCalculation(v, basePrice, discountRate);
            const widthCm = Math.round(v.widthInch * 2.54);
            const lengthCm = Math.round(v.lengthInch * 2.54);

            return `
                <div class="bg-slate-850 hover:bg-slate-800/90 transition border border-slate-800 rounded-2xl p-3.5 flex items-center justify-between shadow-sm">
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

// ================= 3. PRODUCTS & CLOUD DATABASE SYNC =================
const STORAGE_KEY_PRODUCTS = 'ASLAN_PERDE_PRODUCTS';

function getCurtainsApiUrl() {
    if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
        return `${window.location.protocol}//${window.location.host}/api/curtains`;
    }
    return 'https://aslanetsy.onrender.com/api/curtains';
}

async function syncWithCloud() {
    try {
        const res = await fetch(getCurtainsApiUrl());
        if (res.ok) {
            const cloudList = await res.json();
            const formatted = cloudList.map(p => ({
                id: p.id,
                name: p.name,
                category: p.category || 'Curtain',
                m2Price: Number(p.m2Price),
                fabric: p.fabric || '',
                note: p.note || '',
                imageUrl: p.imageUrl || 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600'
            }));
            localStorage.setItem(STORAGE_KEY_PRODUCTS, JSON.stringify(formatted));
            renderProductsCatalog();
        }
    } catch (e) {
        console.log('Cloud sync offline or error:', e);
    }
}

const DEFAULT_WEB_PRODUCTS = [
    {
        id: 1,
        category: 'Curtain',
        name: 'Organic Thick Bamboo Ruffle Curtains - Custom Size, Sold in Pairs.',
        m2Price: 4000,
        fabric: '%100 Organic Thick Bamboo • Fırfırlı (Ruffle)',
        note: 'Etsy Özel Sipariş • Beyaz Bambu Kumaş',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/organic_thick_bamboo_ruffle.jpg'
    },
    {
        id: 2,
        category: 'Bedding',
        name: 'Heart Pattern Organic Cotton Bedding Set - Custom Size Duvet Cover',
        m2Price: 22000,
        fabric: '%100 Organic Cotton • Kırmızı Kalp Desenli Nevresim Takımı',
        note: 'Crib, Toddler, Twin, Double, Queen, King Yatak Ölçüleri',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/heart_pattern_organic_cotton_bedding.png'
    },
    {
        id: 21,
        category: 'Bedding',
        name: 'Duvet Cover Set 100% Cotton, Lace, Embroidered Organic Fabric',
        m2Price: 22000,
        fabric: '%100 Organic Cotton • Fransız Güpürlü / Dantelli (Lace) & Nakışlı Pembe Nevresim Takımı',
        note: 'Pudra Pembe Dantel & Nakış İşlemeli Lüks Pamuk Nevresim Takımı',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/duvet_cover_set_pink_lace_embroidered.webp'
    },
    {
        id: 22,
        category: 'Bedding',
        name: 'Ruffled Duvet Cover Set 100% Cotton, Embroidered Organic Fabric',
        m2Price: 22000,
        fabric: '%100 Organic Cotton • Fırfırlı (Ruffled) & Nakışlı Gri / Vizon Nevresim Takımı',
        note: 'Gri / Vizon Kenarları Fırfırlı Lüks Pamuk Nevresim Takımı',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/ruffled_duvet_cover_set_grey_embroidered.webp'
    },
    {
        id: 3,
        category: 'Curtain',
        name: 'Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Çizgili Karartma (Blackout)',
        note: 'Bordo / Krem Çizgili Rustik Karartma Fon Perde',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_blackout.jpg'
    },
    {
        id: 4,
        category: 'Curtain',
        name: 'Classic Linen Striped Blackout Curtains Organic Fabric - Custom Size (Boydan)',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Boydan Dökümlü Karartma',
        note: 'Bordo / Bej Çizgili Boydan Görünüm',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/classic_linen_striped_full_length.jpg'
    },
    {
        id: 5,
        category: 'Curtain',
        name: 'Densely Pleated Linen Blackout Curtain Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Sık Pileli (Densely Pleated)',
        note: 'Vizon / Taupe Sık Pileli Dökümlü Karartma Keten',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/densely_pleated_linen_blackout.jpg'
    },
    {
        id: 6,
        category: 'Curtain',
        name: 'American Style Dense Pleated Linen Blackout Curtain in Organic Fabric - Customized Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Amerikan / Pinch Pleat Pileli',
        note: 'Amerikan Pileli Rustik Keten Karartma Fon Perde',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/american_style_dense_pleated_linen.jpg'
    },
    {
        id: 7,
        category: 'Curtain',
        name: 'Decorative Frequent Pleated Linen Blackout Curtain Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Düğme Detaylı Sık Pileli',
        note: 'Naturel Bej Keten Düğmeli Dekoratif Karartma',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/decorative_frequent_pleated_linen.jpg'
    },
    {
        id: 8,
        category: 'Curtain',
        name: 'Linen Blackout Lining Curtains, Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Kuşaklı (Tab Top) Karartma Astarlı',
        note: 'Bej/Keten Kuşaklı Karartma Astarlı Fon Perde',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/linen_blackout_lining_curtains.jpg'
    },
    {
        id: 9,
        category: 'Curtain',
        name: 'Tufted Liner Linen Blackout Curtains, Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Yan Püsküllü (Tufted/Tassel)',
        note: 'Pudra / Toz Gül Keten Püsküllü Karartma Fon Perde',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/tufted_liner_linen_blackout.jpg'
    },
    {
        id: 10,
        category: 'Curtain',
        name: 'Dusty Blue Pompom Trim Linen Curtains, Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: '%100 Organic Linen • Buz Mavisi Ponponlu Fırfırlı',
        note: 'Buz Mavisi / Dusty Blue Ponponlu Doğal Keten',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/dusty_blue_pompom_trim_linen.jpg'
    },
    {
        id: 11,
        category: 'Curtain',
        name: 'Suede Velvet Blackout Lining Curtain Organic Fabric - Custom Size',
        m2Price: 4000,
        fabric: 'Lüks Süet Kadife • Karartma Astarlı (Blackout Lining)',
        note: 'Antrasit / Koyu Gri Lüks Kadife Karartma Fon Perde',
        imageUrl: 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/curtains/suede_velvet_blackout_lining.jpg'
    }
];

function initDefaultProducts() {
    const existing = localStorage.getItem(STORAGE_KEY_PRODUCTS);
    if (!existing || JSON.parse(existing).length < 5) {
        localStorage.setItem(STORAGE_KEY_PRODUCTS, JSON.stringify(DEFAULT_WEB_PRODUCTS));
    }
    renderProductsCatalog();
    syncWithCloud();
}

function getStoredProducts() {
    try {
        const list = JSON.parse(localStorage.getItem(STORAGE_KEY_PRODUCTS));
        if (list && list.length > 0) return list;
        return DEFAULT_WEB_PRODUCTS;
    } catch {
        return DEFAULT_WEB_PRODUCTS;
    }
}

function saveProducts(products) {
    localStorage.setItem(STORAGE_KEY_PRODUCTS, JSON.stringify(products));
    renderProductsCatalog();
}

function renderProductsCatalog() {
    const grid = document.getElementById('productsCatalogGrid');
    if (!grid) return;

    const allProducts = getStoredProducts();
    const products = allProducts.filter(p => (p.category || 'Curtain') === selectedCategory);
    const symbol = CURRENCIES[currentCurrency].symbol;

    if (products.length === 0) {
        grid.innerHTML = `<div class="p-8 text-center text-slate-500 text-xs">Bu kategoride henüz kayıtlı model yok. "+ Model Ekle" butonuna dokunarak hemen ekleyebilirsiniz.</div>`;
        return;
    }

    grid.innerHTML = products.map(p => `
        <div class="bg-slate-850 border border-slate-800 rounded-3xl p-4 flex flex-col sm:flex-row gap-4 shadow-xl">
            <!-- Product Image -->
            <div class="w-full sm:w-32 h-36 rounded-2xl bg-slate-800 overflow-hidden relative flex-shrink-0">
                <img src="${p.imageUrl || 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600'}" alt="${p.name}" class="w-full h-full object-cover">
                <button onclick="openEditProductModal(${p.id})" class="absolute bottom-2 left-2 bg-slate-900/90 backdrop-blur-md px-2 py-0.5 rounded-md text-[10px] font-bold ${p.category === 'Bedding' ? 'text-pink-400 border-pink-500/30' : 'text-orange-400 border-orange-500/30'} border flex items-center gap-1 hover:bg-orange-500 hover:text-white transition" title="Fiyatı Değiştir">
                    <span>${formatNumber(p.m2Price)} ${symbol} ${p.category === 'Bedding' ? '/ Baz Takım' : '/ m²'}</span>
                    <i class="fa-solid fa-pencil text-[9px]"></i>
                </button>
            </div>

            <!-- Product Details -->
            <div class="flex-1 flex flex-col justify-between space-y-2">
                <div>
                    <div class="flex items-start justify-between">
                        <h3 class="font-extrabold text-white text-sm">${p.name}</h3>
                        <div class="flex items-center gap-1">
                            <button onclick="openEditProductModal(${p.id})" class="text-orange-400 hover:text-orange-300 p-1.5 bg-orange-500/10 rounded-lg" title="Fiyatı & Modeli Düzenle">
                                <i class="fa-solid fa-pencil text-xs"></i>
                            </button>
                            <button onclick="deleteProduct(${p.id})" class="text-slate-500 hover:text-rose-400 p-1.5 bg-rose-500/10 rounded-lg" title="Sil">
                                <i class="fa-solid fa-trash-can text-xs"></i>
                            </button>
                        </div>
                    </div>
                    ${p.fabric ? `<div class="text-[11px] text-slate-300 font-semibold mt-0.5"><i class="fa-solid fa-layer-group text-orange-400"></i> ${p.fabric}</div>` : ''}
                    ${p.note ? `<p class="text-[11px] text-slate-400 mt-1 line-clamp-2">${p.note}</p>` : ''}
                </div>

                <!-- Action: Open Price List for this product -->
                <div class="pt-2 flex items-center gap-2">
                    <button onclick="openProductPricingModal(${p.id})" class="flex-1 py-2.5 bg-gradient-to-r from-orange-500 to-amber-500 hover:from-orange-600 text-white font-bold text-xs rounded-xl shadow flex items-center justify-center gap-1.5 transition">
                        <i class="fa-solid fa-list-ol"></i>
                        <span>13 Varyasyon Fiyatı</span>
                    </button>
                    <button onclick="openEditProductModal(${p.id})" class="px-3 py-2.5 bg-slate-800 hover:bg-slate-700 text-orange-400 font-bold text-xs rounded-xl border border-orange-500/40 transition flex items-center gap-1">
                        <i class="fa-solid fa-tag"></i>
                        <span>Fiyatı Güncelle</span>
                    </button>
                </div>
            </div>
        </div>
    `).join('');
}

function openEditProductModal(productId) {
    const products = getStoredProducts();
    const prod = products.find(p => p.id === productId);
    if (!prod) return;

    document.getElementById('editProductId').value = prod.id;
    document.getElementById('modalProductTitle').innerHTML = '<i class="fa-solid fa-pencil text-orange-400"></i><span>m² Fiyatı & Modeli Güncelle</span>';
    document.getElementById('prodNameInput').value = prod.name;
    document.getElementById('prodM2PriceInput').value = prod.m2Price;
    document.getElementById('prodFabricInput').value = prod.fabric || '';
    document.getElementById('prodNoteInput').value = prod.note || '';

    currentSelectedImageBase64 = prod.imageUrl;
    const preview = document.getElementById('imagePreview');
    const placeholder = document.getElementById('imagePlaceholder');
    if (preview && placeholder && prod.imageUrl) {
        preview.src = prod.imageUrl;
        preview.classList.remove('hidden');
        placeholder.classList.add('hidden');
    }

    document.getElementById('productModal')?.classList.remove('hidden');
}

function openNewProductModal() {
    document.getElementById('editProductId').value = '';
    document.getElementById('modalProductTitle').innerHTML = '<i class="fa-solid fa-plus-circle text-orange-400"></i><span>Yeni Perde Modeli Ekle</span>';
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

async function handleSaveProductDirect() {
    const editId = document.getElementById('editProductId')?.value;
    const name = document.getElementById('prodNameInput').value.trim();
    const m2Price = parseFloat(document.getElementById('prodM2PriceInput').value) || 0;
    const fabric = document.getElementById('prodFabricInput').value.trim();
    const note = document.getElementById('prodNoteInput')?.value?.trim() || '';

    if (!name || m2Price <= 0) {
        showToast('Lütfen model adı ve m² fiyatını girin!', 'warning');
        return;
    }

    const payload = {
        name,
        category: selectedCategory,
        m2Price,
        fabric: fabric || (selectedCategory === 'Bedding' ? '%100 Organik Pamuk Saten' : 'Doğal Kumaş'),
        note,
        imageUrl: currentSelectedImageBase64 || (selectedCategory === 'Bedding' 
            ? 'https://raw.githubusercontent.com/nerimanaslan/AslanEtsy/main/AslanEtsy.WebApi/wwwroot/images/bedding/heart_pattern_organic_cotton_bedding.png'
            : 'https://images.unsplash.com/photo-1513694203232-719a280e022f?w=600')
    };

    try {
        if (editId) {
            // Update
            await fetch(`${getCurtainsApiUrl()}/${editId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            showToast(`"${name}" bulutta güncellendi! ☁️`, 'success');
        } else {
            // Create
            await fetch(getCurtainsApiUrl(), {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            showToast(`"${name}" buluta kaydedildi! ☁️🎉`, 'success');
        }
        await syncWithCloud();
    } catch (e) {
        // Fallback local
        const products = getStoredProducts();
        if (editId) {
            const idx = products.findIndex(p => p.id == editId);
            if (idx !== -1) products[idx] = { ...products[idx], ...payload };
        } else {
            products.unshift({ id: Date.now(), ...payload });
        }
        saveProducts(products);
        showToast(`"${name}" kaydedildi!`, 'success');
    }

    // Clear inputs
    if (document.getElementById('editProductId')) document.getElementById('editProductId').value = '';
    document.getElementById('prodNameInput').value = '';
    document.getElementById('prodFabricInput').value = '';
    currentSelectedImageBase64 = null;
    document.getElementById('imagePreview')?.classList.add('hidden');
    document.getElementById('imagePlaceholder')?.classList.remove('hidden');
    closeProductModal();
}

function handleSaveProduct(e) {
    if (e) e.preventDefault();
    handleSaveProductDirect();
}

async function deleteProduct(productId) {
    if (!confirm('Bu perde modelini silmek istediğinize emin misiniz?')) return;
    try {
        await fetch(`${getCurtainsApiUrl()}/${productId}`, { method: 'DELETE' });
        await syncWithCloud();
        showToast('Model buluttan silindi.', 'info');
    } catch {
        let products = getStoredProducts();
        products = products.filter(p => p.id !== productId);
        saveProducts(products);
        showToast('Model silindi.', 'info');
    }
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

// Touch Swipe Navigation for Mobile Web
let touchStartX = 0;
let touchEndX = 0;
const TABS_ORDER = ['catalog', 'calculator', 'custom'];

document.addEventListener('touchstart', (e) => {
    touchStartX = e.changedTouches[0].screenX;
}, false);

document.addEventListener('touchend', (e) => {
    touchEndX = e.changedTouches[0].screenX;
    handleSwipeGesture();
}, false);

function handleSwipeGesture() {
    const swipeDistance = touchEndX - touchStartX;
    if (Math.abs(swipeDistance) < 50) return; // Ignore small movements

    const currentIndex = TABS_ORDER.indexOf(currentMobileTab);
    if (swipeDistance < -50 && currentIndex < TABS_ORDER.length - 1) {
        // Swiped Left -> Go to Next Tab
        switchMobileTab(TABS_ORDER[currentIndex + 1]);
    } else if (swipeDistance > 50 && currentIndex > 0) {
        // Swiped Right -> Go to Prev Tab
        switchMobileTab(TABS_ORDER[currentIndex - 1]);
    }
}

// ================= 5. TAB SWITCHING & HELPERS =================
function switchMobileTab(tab) {
    currentMobileTab = tab;

    // Reset bottom and top nav buttons
    document.querySelectorAll('nav button, .top-tab-btn').forEach(btn => {
        btn.classList.remove('text-orange-500', 'font-bold', 'bg-orange-500', 'text-white');
        btn.classList.add('text-slate-400');
    });

    const activeNav = document.getElementById(`nav-${tab}`);
    if (activeNav) {
        activeNav.classList.add('text-orange-500', 'font-bold');
        activeNav.classList.remove('text-slate-400');
    }

    const activeTopNav = document.getElementById(`top-nav-${tab}`);
    if (activeTopNav) {
        activeTopNav.classList.add('bg-orange-500', 'text-white', 'font-extrabold');
        activeTopNav.classList.remove('text-slate-400');
    }

    // Hide all sections
    document.getElementById('tab-calculator')?.classList.add('hidden');
    document.getElementById('tab-custom')?.classList.add('hidden');
    document.getElementById('tab-catalog')?.classList.add('hidden');

    // Show active section with smooth fade/slide in
    const activeSection = document.getElementById(`tab-${tab}`);
    if (activeSection) {
        activeSection.classList.remove('hidden');
    }
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
