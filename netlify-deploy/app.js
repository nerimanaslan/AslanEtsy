// State management
let currentTab = 'dashboard';
let currentPage = 1;
let totalPages = 1;
let currentSelectedOrderId = null;
let allShopsCache = [];

// API Helper & Safe Fetch
function getApiUrl(path) {
    const customApi = localStorage.getItem('ASLAN_API_URL');
    if (customApi && customApi.trim() !== '') {
        return customApi.trim().replace(/\/$/, '') + path;
    }
    return path;
}

async function safeFetchJson(url, options = {}) {
    try {
        const res = await fetch(url, options);
        const contentType = res.headers.get('content-type') || '';

        if (!res.ok) {
            let errorMsg = `İşlem başarısız (${res.status})`;
            if (contentType.includes('application/json')) {
                try {
                    const errData = await res.json();
                    errorMsg = errData.message || errorMsg;
                } catch { }
            } else {
                const text = await res.text();
                if (text && text.length < 200 && !text.includes('<!DOCTYPE')) errorMsg = text;
            }
            throw new Error(errorMsg);
        }

        if (contentType.includes('application/json')) {
            return await res.json();
        }
        return null;
    } catch (err) {
        throw err;
    }
}

// DOM Ready
document.addEventListener('DOMContentLoaded', () => {
    checkUrlParams();
    initApiSettings();
    checkAuth();
});

// ================= AUTHENTICATION LOGIC =================
const VALID_USERNAME = "neriman";
const VALID_PASSWORD = "1217";

function checkAuth() {
    const isAuth = localStorage.getItem('ASLAN_USER_AUTH') || sessionStorage.getItem('ASLAN_USER_AUTH');
    const loginScreen = document.getElementById('loginScreen');
    const appContainer = document.getElementById('appContainer');

    if (isAuth) {
        loginScreen?.classList.add('hidden');
        appContainer?.classList.remove('hidden');
        loadDashboardStats();
        loadShopsList();
        loadOrders();
    } else {
        loginScreen?.classList.remove('hidden');
        appContainer?.classList.add('hidden');
    }
}

function handleLogin(e) {
    e.preventDefault();
    const usernameInput = document.getElementById('loginUsername').value.trim().toLowerCase();
    const passwordInput = document.getElementById('loginPassword').value.trim();
    const rememberMe = document.getElementById('rememberMe').checked;
    const errorBox = document.getElementById('loginError');

    if (usernameInput === VALID_USERNAME.toLowerCase() && passwordInput === VALID_PASSWORD) {
        errorBox?.classList.add('hidden');
        
        if (rememberMe) {
            localStorage.setItem('ASLAN_USER_AUTH', 'neriman');
        } else {
            sessionStorage.setItem('ASLAN_USER_AUTH', 'neriman');
        }

        showAlert('Giriş başarılı. Hoş geldiniz, Neriman!', 'success');
        checkAuth();
    } else {
        errorBox?.classList.remove('hidden');
        document.getElementById('loginErrorMsg').innerText = 'Kullanıcı adı veya şifre hatalı!';
    }
}

function handleLogout() {
    if (confirm('Çıkış yapmak istediğinize emin misiniz?')) {
        localStorage.removeItem('ASLAN_USER_AUTH');
        sessionStorage.removeItem('ASLAN_USER_AUTH');
        document.getElementById('loginPassword').value = '';
        checkAuth();
    }
}

function togglePasswordVisibility() {
    const input = document.getElementById('loginPassword');
    const icon = document.getElementById('eyeIcon');
    if (input.type === 'password') {
        input.type = 'text';
        icon.className = 'fa-solid fa-eye-slash text-sm text-orange-500';
    } else {
        input.type = 'password';
        icon.className = 'fa-solid fa-eye text-sm';
    }
}

// ================= APP INITIALIZATION =================
function checkUrlParams() {
    const params = new URLSearchParams(window.location.search);
    if (params.get('oauth') === 'success') {
        showAlert('Etsy mağaza bağlantısı (OAuth 2.0) başarıyla tamamlandı!', 'success');
        window.history.replaceState({}, document.title, window.location.pathname);
    } else if (params.get('oauth') === 'error') {
        showAlert('Etsy mağaza bağlantısı başarısız oldu veya reddedildi.', 'error');
        window.history.replaceState({}, document.title, window.location.pathname);
    }
}

function initApiSettings() {
    const customApi = localStorage.getItem('ASLAN_API_URL') || '';
    const input = document.getElementById('inputBackendUrl');
    if (input) input.value = customApi;
}

function openApiSettingsModal() {
    const input = document.getElementById('inputBackendUrl');
    if (input) input.value = localStorage.getItem('ASLAN_API_URL') || '';
    document.getElementById('apiSettingsModal')?.classList.remove('hidden');
}

function closeApiSettingsModal() {
    document.getElementById('apiSettingsModal')?.classList.add('hidden');
}

function saveApiSettings() {
    const url = document.getElementById('inputBackendUrl').value.trim();
    if (url) {
        localStorage.setItem('ASLAN_API_URL', url);
        showAlert('Backend API adresi kaydedildi: ' + url, 'success');
    } else {
        localStorage.removeItem('ASLAN_API_URL');
        showAlert('API adresi varsayılan (aynı sunucu) olarak ayarlandı.', 'info');
    }
    closeApiSettingsModal();
    loadDashboardStats();
    loadShopsList();
    loadOrders();
}

// Tab Switching
function switchTab(tabId) {
    currentTab = tabId;
    
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active', 'bg-orange-500', 'text-white');
        btn.classList.add('text-slate-600');
    });
    
    const activeBtn = document.getElementById(`tab-${tabId}`);
    if (activeBtn) {
        activeBtn.classList.add('active', 'bg-orange-500', 'text-white');
        activeBtn.classList.remove('text-slate-600');
    }

    document.getElementById('view-dashboard')?.classList.add('hidden');
    document.getElementById('view-orders')?.classList.add('hidden');
    document.getElementById('view-shops')?.classList.add('hidden');
    document.getElementById('view-logs')?.classList.add('hidden');

    document.getElementById(`view-${tabId}`)?.classList.remove('hidden');

    if (tabId === 'dashboard') loadDashboardStats();
    else if (tabId === 'orders') loadOrders();
    else if (tabId === 'shops') loadShopsView();
    else if (tabId === 'logs') loadSyncLogs();
}

// 1. Dashboard Stats
async function loadDashboardStats() {
    try {
        const data = await safeFetchJson(getApiUrl('/api/dashboard/stats'));
        if (!data) return;

        document.getElementById('statActiveShops').innerText = `${data.activeShops || 0} / ${data.totalShops || 0}`;
        document.getElementById('statTotalOrders').innerText = data.totalOrders || 0;
        document.getElementById('statUnfulfilledOrders').innerText = data.unfulfilledOrders || 0;
        document.getElementById('statTotalRevenue').innerText = formatCurrency(data.totalRevenue, data.defaultCurrency);

        const summaryList = document.getElementById('shopSummaryList');
        if (summaryList) {
            if (data.shopSummaries && data.shopSummaries.length > 0) {
                summaryList.innerHTML = data.shopSummaries.map(s => `
                    <div class="p-3 bg-slate-50 rounded-xl border border-slate-200/80 flex items-center justify-between">
                        <div>
                            <div class="font-semibold text-slate-800 text-sm flex items-center gap-2">
                                <span>${s.shopName}</span>
                                ${s.isConnected 
                                    ? '<span class="w-2 h-2 rounded-full bg-emerald-500" title="Bağlı"></span>' 
                                    : '<span class="w-2 h-2 rounded-full bg-rose-400" title="Bağlantı Bekliyor"></span>'}
                            </div>
                            <div class="text-xs text-slate-500 mt-0.5">${s.totalOrders} Sipariş • ${s.openOrders} Açık</div>
                        </div>
                        <div class="text-right">
                            <div class="text-xs font-bold text-emerald-600">${formatCurrency(s.totalRevenue, 'USD')}</div>
                            <button onclick="syncSingleShop(${s.accountId})" class="text-[11px] text-orange-600 hover:underline mt-0.5">Senkronize Et</button>
                        </div>
                    </div>
                `).join('');
            } else {
                summaryList.innerHTML = `<div class="text-slate-400 text-xs text-center py-4">Henüz mağaza eklenmemiş.</div>`;
            }
        }

        const recentTable = document.getElementById('recentOrdersTable');
        if (recentTable) {
            if (data.recentOrders && data.recentOrders.length > 0) {
                recentTable.innerHTML = data.recentOrders.map(o => `
                    <tr class="hover:bg-slate-50 transition cursor-pointer" onclick="openOrderDetailModal(${o.id})">
                        <td class="py-3">
                            <div class="font-semibold text-slate-800">#${o.receiptId}</div>
                            <div class="text-xs text-slate-400">${o.shopName}</div>
                        </td>
                        <td class="py-3">
                            <div class="text-slate-700">${o.buyerName}</div>
                            <div class="text-xs text-slate-400">${o.shippingCountryIso || ''}</div>
                        </td>
                        <td class="py-3 font-semibold text-slate-800">
                            ${formatCurrency(o.grandTotalAmount, o.currencyCode)}
                        </td>
                        <td class="py-3">
                            ${getStatusBadge(o.status, o.customStatus)}
                        </td>
                        <td class="py-3 text-right">
                            <button onclick="event.stopPropagation(); openOrderDetailModal(${o.id})" class="px-2.5 py-1 text-xs bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-md font-medium">Detay</button>
                        </td>
                    </tr>
                `).join('');
            } else {
                recentTable.innerHTML = `<tr><td colspan="5" class="py-6 text-center text-slate-400">Henüz sipariş kaydı yok.</td></tr>`;
            }
        }

    } catch (err) {
        console.error(err);
    }
}

// 2. Orders View & Filtering
async function loadOrders() {
    const shopId = document.getElementById('filterShop')?.value;
    const status = document.getElementById('filterStatus')?.value;
    const customStatus = document.getElementById('filterCustomStatus')?.value;
    const search = document.getElementById('orderSearchInput')?.value;

    const params = new URLSearchParams({
        pageNumber: currentPage,
        pageSize: 15
    });

    if (shopId) params.append('etsyAccountId', shopId);
    if (status) params.append('status', status);
    if (customStatus) params.append('customStatus', customStatus);
    if (search) params.append('searchTerm', search);

    try {
        const data = await safeFetchJson(getApiUrl(`/api/orders?${params.toString()}`));
        if (!data) return;

        totalPages = data.totalPages || 1;
        const pageDisp = document.getElementById('currentPageDisplay');
        if (pageDisp) pageDisp.innerText = data.pageNumber;

        const pagInfo = document.getElementById('paginationInfo');
        if (pagInfo) {
            pagInfo.innerText = data.totalCount > 0 
                ? `Toplam ${data.totalCount} siparişten ${(data.pageNumber - 1) * data.pageSize + 1} - ${Math.min(data.pageNumber * data.pageSize, data.totalCount)} arası`
                : '0 kayıt gösteriliyor';
        }
        
        const prevBtn = document.getElementById('btnPrevPage');
        if (prevBtn) prevBtn.disabled = !data.hasPreviousPage;

        const nextBtn = document.getElementById('btnNextPage');
        if (nextBtn) nextBtn.disabled = !data.hasNextPage;

        const tbody = document.getElementById('ordersTableBody');
        if (tbody) {
            if (data.items && data.items.length > 0) {
                tbody.innerHTML = data.items.map(o => `
                    <tr class="hover:bg-slate-50 transition cursor-pointer" onclick="openOrderDetailModal(${o.id})">
                        <td class="py-3 px-4 font-semibold text-slate-800">
                            #${o.receiptId}
                            ${o.tags ? `<div class="text-[10px] text-orange-600 font-medium">${o.tags}</div>` : ''}
                        </td>
                        <td class="py-3 px-4 text-xs font-medium text-slate-600">${o.shopName}</td>
                        <td class="py-3 px-4">
                            <div class="font-medium text-slate-800 text-xs">${o.buyerName}</div>
                            <div class="text-[11px] text-slate-400">${o.shippingCity || ''} ${o.shippingCountryIso ? `(${o.shippingCountryIso})` : ''}</div>
                        </td>
                        <td class="py-3 px-4 text-xs text-slate-600">${o.itemCount} ürün</td>
                        <td class="py-3 px-4 font-bold text-slate-800 text-xs">
                            ${formatCurrency(o.grandTotalAmount, o.currencyCode)}
                        </td>
                        <td class="py-3 px-4">${getEtsyStatusBadge(o.status)}</td>
                        <td class="py-3 px-4">${getCustomPipelineBadge(o.customStatus)}</td>
                        <td class="py-3 px-4 text-xs">
                            ${o.hasTracking 
                                ? `<span class="inline-flex items-center gap-1 text-emerald-600 font-medium"><i class="fa-solid fa-circle-check text-[10px]"></i> ${o.latestCarrierName}: ${o.latestTrackingCode}</span>` 
                                : `<span class="text-slate-400">Takip Yok</span>`}
                        </td>
                        <td class="py-3 px-4 text-xs text-slate-500">${formatDate(o.orderDateUtc)}</td>
                        <td class="py-3 px-4 text-center">
                            <button onclick="event.stopPropagation(); openOrderDetailModal(${o.id})" class="px-3 py-1 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-md text-xs font-semibold">İncele</button>
                        </td>
                    </tr>
                `).join('');
            } else {
                tbody.innerHTML = `<tr><td colspan="10" class="py-12 text-center text-slate-400">Henüz sipariş bulunmuyor.</td></tr>`;
            }
        }

    } catch (err) {
        console.error(err);
        showAlert(err.message || 'Sipariş listesi yüklenirken hata oluştu.', 'error');
    }
}

function applyOrderFilters() {
    currentPage = 1;
    loadOrders();
}

function changePage(delta) {
    currentPage += delta;
    loadOrders();
}

// 3. Shop Management & OAuth Flow
async function loadShopsList() {
    try {
        const shops = await safeFetchJson(getApiUrl('/api/accounts'));
        allShopsCache = shops || [];

        const select = document.getElementById('filterShop');
        if (select) {
            select.innerHTML = '<option value="">Tüm Mağazalar</option>' + (shops || []).map(s => `
                <option value="${s.id}">${s.shopName}</option>
            `).join('');
        }

        if (currentTab === 'shops') {
            loadShopsView();
        }
    } catch (err) {
        console.error(err);
    }
}

function loadShopsView() {
    const grid = document.getElementById('shopsGrid');
    if (!grid) return;

    if (!allShopsCache || allShopsCache.length === 0) {
        grid.innerHTML = `<div class="col-span-3 py-12 text-center text-slate-400">Henüz eklenmiş Etsy mağazası bulunmuyor. "Yeni Mağaza Ekle" butonu ile başlayabilirsiniz.</div>`;
        return;
    }

    grid.innerHTML = allShopsCache.map(s => `
        <div class="bg-white rounded-2xl border border-slate-200/80 shadow-sm p-6 flex flex-col justify-between space-y-4">
            <div>
                <div class="flex items-start justify-between">
                    <div class="flex items-center space-x-3">
                        <div class="w-12 h-12 rounded-xl bg-orange-100 text-orange-600 flex items-center justify-center text-xl font-bold">
                            <i class="fa-brands fa-etsy"></i>
                        </div>
                        <div>
                            <h3 class="font-bold text-slate-800 text-base">${s.shopName}</h3>
                            <div class="text-xs text-slate-400">Shop ID: ${s.shopId}</div>
                        </div>
                    </div>
                    <span class="px-2.5 py-1 text-[11px] font-semibold rounded-full ${s.isConnected ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700'}">
                        ${s.isConnected ? '● Bağlı' : '○ Bağlantı Yok'}
                    </span>
                </div>

                <div class="mt-4 pt-4 border-t border-slate-100 space-y-2 text-xs text-slate-600">
                    <div class="flex justify-between">
                        <span>API Keystring:</span>
                        <span class="font-mono text-slate-700">${s.keystring ? s.keystring.substring(0, 8) + '...' : '-'}</span>
                    </div>
                    <div class="flex justify-between">
                        <span>Kayıtlı Sipariş:</span>
                        <span class="font-semibold text-slate-800">${s.orderCount || 0} adet</span>
                    </div>
                    <div class="flex justify-between">
                        <span>Son Senkronizasyon:</span>
                        <span class="text-slate-500">${s.lastSyncAtUtc ? formatDate(s.lastSyncAtUtc) : 'Henüz yapılmadı'}</span>
                    </div>
                    ${s.lastSyncError ? `<div class="text-[11px] text-rose-600 bg-rose-50 p-2 rounded-lg mt-2">${s.lastSyncError}</div>` : ''}
                </div>
            </div>

            <div class="pt-3 border-t border-slate-100 flex items-center gap-2">
                ${!s.isConnected 
                    ? `<button onclick="connectShopOAuth(${s.id})" class="flex-1 py-2 bg-orange-600 hover:bg-orange-700 text-white rounded-lg text-xs font-semibold shadow transition flex items-center justify-center gap-1.5">
                         <i class="fa-solid fa-plug"></i> Etsy Hesabını Bağla (OAuth)
                       </button>`
                    : `<button onclick="syncSingleShop(${s.id})" class="flex-1 py-2 bg-slate-900 hover:bg-slate-800 text-white rounded-lg text-xs font-semibold shadow transition flex items-center justify-center gap-1.5">
                         <i class="fa-solid fa-rotate"></i> Siparişleri Çek
                       </button>`}
                <button onclick="deleteShop(${s.id})" class="p-2 text-slate-400 hover:text-rose-600 rounded-lg hover:bg-rose-50 transition" title="Mağazayı Sil">
                    <i class="fa-solid fa-trash-can"></i>
                </button>
            </div>
        </div>
    `).join('');
}

function openNewShopModal() {
    document.getElementById('newShopModal')?.classList.remove('hidden');
}

function closeNewShopModal() {
    document.getElementById('newShopModal')?.classList.add('hidden');
}

async function submitNewShop() {
    const shopName = document.getElementById('newShopName').value.trim();
    const shopId = parseInt(document.getElementById('newShopId').value.trim()) || 1;
    const keystring = document.getElementById('newShopKeystring').value.trim();
    const sharedSecret = document.getElementById('newShopSecret').value.trim();
    const autoSync = document.getElementById('newShopAutoSync').checked;

    if (!shopName || !keystring) {
        showAlert('Lütfen Mağaza Adı ve Keystring (Client ID) alanlarını doldurun.', 'warning');
        return;
    }

    try {
        await safeFetchJson(getApiUrl('/api/accounts'), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                shopName,
                shopId,
                keystring,
                sharedSecret,
                autoSyncEnabled: autoSync
            })
        });

        showAlert('Mağaza başarıyla eklendi!', 'success');
        closeNewShopModal();
        await loadShopsList();
        loadDashboardStats();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function connectShopOAuth(shopId) {
    try {
        const data = await safeFetchJson(getApiUrl(`/api/accounts/${shopId}/oauth/authorize`));
        if (data && data.authorizationUrl) {
            window.location.href = data.authorizationUrl;
        } else {
            throw new Error('OAuth yetkilendirme linki oluşturulamadı.');
        }
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function deleteShop(shopId) {
    if (!confirm('Bu mağazayı silmek istediğinize emin misiniz?')) return;

    try {
        await safeFetchJson(getApiUrl(`/api/accounts/${shopId}`), { method: 'DELETE' });
        showAlert('Mağaza silindi.', 'info');
        await loadShopsList();
        loadDashboardStats();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

// 4. Order Detail & Tracking Modal
async function openOrderDetailModal(orderId) {
    currentSelectedOrderId = orderId;
    try {
        const o = await safeFetchJson(getApiUrl(`/api/orders/${orderId}`));
        if (!o) throw new Error('Sipariş detayı alınamadı');

        document.getElementById('modalReceiptId').innerText = o.receiptId;
        document.getElementById('modalOrderShopBadge').innerText = o.shopName;
        document.getElementById('modalOrderDate').innerText = `Sipariş Tarihi: ${formatDate(o.orderDateUtc)}`;
        
        document.getElementById('modalBuyerName').innerText = o.buyerName;
        document.getElementById('modalBuyerEmail').innerText = o.buyerEmail;
        document.getElementById('modalShippingAddress').innerText = o.shippingAddressFormatted || 'Adres bilgisi mevcut değil.';

        document.getElementById('modalSubtotal').innerText = formatCurrency(o.subtotalAmount, o.currencyCode);
        document.getElementById('modalShippingAmt').innerText = formatCurrency(o.shippingAmount, o.currencyCode);
        document.getElementById('modalTaxAmt').innerText = formatCurrency(o.taxAmount, o.currencyCode);
        document.getElementById('modalDiscountAmt').innerText = formatCurrency(o.discountAmount, o.currencyCode);
        document.getElementById('modalGrandTotal').innerText = formatCurrency(o.grandTotalAmount, o.currencyCode);

        document.getElementById('modalEditCustomStatus').value = o.customStatus;
        document.getElementById('modalEditTags').value = o.tags || '';
        document.getElementById('modalEditInternalNote').value = o.internalNote || '';

        const itemsList = document.getElementById('modalOrderItemsList');
        if (itemsList) {
            if (o.items && o.items.length > 0) {
                itemsList.innerHTML = o.items.map(item => `
                    <div class="p-3 bg-slate-50 rounded-xl border border-slate-200 flex items-center justify-between">
                        <div class="flex items-center space-x-3">
                            <div class="w-10 h-10 rounded-lg bg-slate-200 flex items-center justify-center text-slate-500 font-bold text-xs">
                                <i class="fa-solid fa-cube"></i>
                            </div>
                            <div>
                                <div class="font-semibold text-slate-800 text-xs">${item.title}</div>
                                ${item.sku ? `<span class="text-[10px] bg-slate-200 text-slate-700 px-1.5 py-0.5 rounded font-mono">SKU: ${item.sku}</span>` : ''}
                                ${item.variationsSummary ? `<div class="text-[11px] text-slate-500 mt-0.5">${item.variationsSummary}</div>` : ''}
                                ${item.buyerPersonalization ? `<div class="text-[11px] text-amber-700 font-medium mt-0.5"><i class="fa-solid fa-signature"></i> Kişiselleştirme: ${item.buyerPersonalization}</div>` : ''}
                            </div>
                        </div>
                        <div class="text-right">
                            <div class="font-semibold text-slate-800 text-xs">${item.quantity} x ${formatCurrency(item.unitPrice, item.currencyCode)}</div>
                            <div class="text-xs font-bold text-emerald-600">${formatCurrency(item.totalPrice, item.currencyCode)}</div>
                        </div>
                    </div>
                `).join('');
            } else {
                itemsList.innerHTML = `<div class="text-xs text-slate-400">Ürün kaydı yok.</div>`;
            }
        }

        renderTrackingsList(o.trackings);
        document.getElementById('orderDetailModal')?.classList.remove('hidden');
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

function closeOrderDetailModal() {
    document.getElementById('orderDetailModal')?.classList.add('hidden');
    currentSelectedOrderId = null;
}

function renderTrackingsList(trackings) {
    const container = document.getElementById('modalTrackingList');
    if (!container) return;

    if (!trackings || trackings.length === 0) {
        container.innerHTML = `<div class="text-xs text-slate-500">Henüz kargo takip kodu eklenmedi.</div>`;
        return;
    }

    container.innerHTML = trackings.map(t => `
        <div class="p-2.5 bg-white rounded-lg border border-orange-200 flex items-center justify-between text-xs">
            <div class="flex items-center space-x-2">
                <i class="fa-solid fa-truck text-orange-500"></i>
                <span class="font-semibold text-slate-800">${t.carrierName}:</span>
                <span class="font-mono text-slate-700 bg-slate-100 px-2 py-0.5 rounded">${t.trackingCode}</span>
                ${t.isSyncedToEtsy 
                    ? '<span class="text-[10px] text-emerald-600 font-medium px-1.5 py-0.5 bg-emerald-50 rounded"><i class="fa-solid fa-check"></i> Etsy Senkron</span>' 
                    : '<span class="text-[10px] text-amber-600 font-medium px-1.5 py-0.5 bg-amber-50 rounded">Etsy\'ye İletilmedi</span>'}
            </div>
            <div class="flex items-center space-x-2">
                ${!t.isSyncedToEtsy ? `<button onclick="resyncTracking(${t.id})" class="text-[11px] text-blue-600 hover:underline">Tekrar İlet</button>` : ''}
                <button onclick="deleteTracking(${t.id})" class="text-slate-400 hover:text-rose-600 text-xs"><i class="fa-solid fa-trash"></i></button>
            </div>
        </div>
    `).join('');
}

function toggleTrackingForm() {
    const form = document.getElementById('addTrackingForm');
    form?.classList.toggle('hidden');
}

async function submitOrderTracking() {
    if (!currentSelectedOrderId) return;

    const carrier = document.getElementById('inputTrackingCarrier').value.trim();
    const code = document.getElementById('inputTrackingCode').value.trim();
    const sendToEtsy = document.getElementById('checkSendToEtsy').checked;

    if (!carrier || !code) {
        showAlert('Kargo firması ve takip numarasını girin.', 'warning');
        return;
    }

    try {
        await safeFetchJson(getApiUrl(`/api/orders/${currentSelectedOrderId}/tracking`), {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                carrierName: carrier,
                trackingCode: code,
                sendToEtsyImmediately: sendToEtsy
            })
        });

        showAlert('Kargo takip kodu başarıyla kaydedildi!', 'success');
        document.getElementById('inputTrackingCode').value = '';
        document.getElementById('addTrackingForm')?.classList.add('hidden');
        
        openOrderDetailModal(currentSelectedOrderId);
        loadOrders();
        loadDashboardStats();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function resyncTracking(trackingId) {
    try {
        await safeFetchJson(getApiUrl(`/api/orders/tracking/${trackingId}/resync`), { method: 'POST' });
        showAlert('Takip kodu Etsy\'ye iletildi.', 'success');
        if (currentSelectedOrderId) openOrderDetailModal(currentSelectedOrderId);
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function deleteTracking(trackingId) {
    if (!confirm('Bu takip kaydını silmek istediğinize emin misiniz?')) return;
    try {
        await safeFetchJson(getApiUrl(`/api/orders/tracking/${trackingId}`), { method: 'DELETE' });
        if (currentSelectedOrderId) openOrderDetailModal(currentSelectedOrderId);
        loadOrders();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function saveOrderEdits() {
    if (!currentSelectedOrderId) return;

    const customStatus = document.getElementById('modalEditCustomStatus').value;
    const tags = document.getElementById('modalEditTags').value.trim();
    const internalNote = document.getElementById('modalEditInternalNote').value.trim();

    try {
        await safeFetchJson(getApiUrl(`/api/orders/${currentSelectedOrderId}`), {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                customStatus,
                tags,
                internalNote
            })
        });

        showAlert('Sipariş bilgileri güncellendi!', 'success');
        closeOrderDetailModal();
        loadOrders();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

// 5. Sync Actions
async function syncAllShops() {
    const btn = document.getElementById('btnSyncAll');
    const icon = document.getElementById('syncIcon');
    if (btn) btn.disabled = true;
    if (icon) icon.classList.add('fa-spin');

    try {
        const results = await safeFetchJson(getApiUrl('/api/sync/all'), { method: 'POST' });
        const successCount = (results || []).filter(r => r.status === 'Success').length;
        showAlert(`Senkronizasyon tamamlandı: ${successCount} mağaza güncellendi.`, 'success');
        
        loadDashboardStats();
        loadOrders();
        loadShopsList();
    } catch (err) {
        showAlert(err.message, 'error');
    } finally {
        if (btn) btn.disabled = false;
        if (icon) icon.classList.remove('fa-spin');
    }
}

async function syncSingleShop(shopId) {
    try {
        showAlert('Mağaza senkronizasyonu başlatıldı...', 'info');
        const result = await safeFetchJson(getApiUrl(`/api/sync/account/${shopId}`), { method: 'POST' });
        if (result) {
            showAlert(`${result.shopName}: ${result.ordersFetched} sipariş çekildi (${result.ordersCreated} yeni, ${result.ordersUpdated} güncel).`, 'success');
        }
        loadDashboardStats();
        loadOrders();
        loadShopsList();
    } catch (err) {
        showAlert(err.message, 'error');
    }
}

async function loadSyncLogs() {
    const tbody = document.getElementById('syncLogsTableBody');
    if (!tbody) return;

    try {
        const logs = await safeFetchJson(getApiUrl('/api/sync/logs'));
        if (logs && logs.length > 0) {
            tbody.innerHTML = logs.map(l => `
                <tr class="hover:bg-slate-50 text-xs">
                    <td class="py-3 px-4 font-mono text-slate-500">${formatDate(l.startedAtUtc)}</td>
                    <td class="py-3 px-4 font-semibold text-slate-800">${l.shopName}</td>
                    <td class="py-3 px-4">
                        <span class="px-2 py-0.5 rounded-full font-semibold ${l.status === 'Success' ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700'}">
                            ${l.status}
                        </span>
                    </td>
                    <td class="py-3 px-4 font-medium text-slate-700">${l.ordersFetched}</td>
                    <td class="py-3 px-4 text-emerald-600 font-semibold">+${l.ordersCreated}</td>
                    <td class="py-3 px-4 text-blue-600 font-semibold">${l.ordersUpdated}</td>
                    <td class="py-3 px-4 text-slate-500">${l.errorMessage || 'Başarılı'}</td>
                </tr>
            `).join('');
        } else {
            tbody.innerHTML = `<tr><td colspan="7" class="py-8 text-center text-slate-400">Henüz senkronizasyon kaydı bulunmuyor.</td></tr>`;
        }
    } catch (err) {
        console.error(err);
    }
}

// Helpers & Formatting
function showAlert(message, type = 'info') {
    const banner = document.getElementById('alertBanner');
    const msgEl = document.getElementById('alertMessage');
    const iconEl = document.getElementById('alertIcon');

    if (!banner || !msgEl || !iconEl) return;

    banner.className = 'mb-6 p-4 rounded-xl text-sm font-medium flex items-center justify-between shadow-sm';
    
    if (type === 'success') {
        banner.classList.add('bg-emerald-50', 'text-emerald-800', 'border', 'border-emerald-200');
        iconEl.className = 'fa-solid fa-circle-check text-emerald-600 text-lg';
    } else if (type === 'error') {
        banner.classList.add('bg-rose-50', 'text-rose-800', 'border', 'border-rose-200');
        iconEl.className = 'fa-solid fa-triangle-exclamation text-rose-600 text-lg';
    } else if (type === 'warning') {
        banner.classList.add('bg-amber-50', 'text-amber-800', 'border', 'border-amber-200');
        iconEl.className = 'fa-solid fa-circle-exclamation text-amber-600 text-lg';
    } else {
        banner.classList.add('bg-blue-50', 'text-blue-800', 'border', 'border-blue-200');
        iconEl.className = 'fa-solid fa-circle-info text-blue-600 text-lg';
    }

    msgEl.innerText = message;
    banner.classList.remove('hidden');

    setTimeout(() => {
        banner.classList.add('hidden');
    }, 6000);
}

function hideAlert() {
    document.getElementById('alertBanner')?.classList.add('hidden');
}

function formatCurrency(amount, currency = 'USD') {
    const symbols = { 'USD': '$', 'EUR': '€', 'GBP': '£', 'TRY': '₺' };
    const sym = symbols[currency] || (currency + ' ');
    return `${sym}${Number(amount || 0).toFixed(2)}`;
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    const d = new Date(dateStr);
    return d.toLocaleString('tr-TR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function getEtsyStatusBadge(status) {
    const badges = {
        'Paid': '<span class="px-2 py-0.5 text-[11px] font-semibold bg-emerald-100 text-emerald-700 rounded-full">Ödendi</span>',
        'Shipped': '<span class="px-2 py-0.5 text-[11px] font-semibold bg-blue-100 text-blue-700 rounded-full">Kargolandı</span>',
        'Open': '<span class="px-2 py-0.5 text-[11px] font-semibold bg-slate-100 text-slate-700 rounded-full">Açık</span>',
        'Completed': '<span class="px-2 py-0.5 text-[11px] font-semibold bg-indigo-100 text-indigo-700 rounded-full">Tamamlandı</span>',
        'Canceled': '<span class="px-2 py-0.5 text-[11px] font-semibold bg-rose-100 text-rose-700 rounded-full">İptal</span>'
    };
    return badges[status] || `<span class="px-2 py-0.5 text-[11px] font-semibold bg-slate-100 text-slate-700 rounded-full">${status}</span>`;
}

function getCustomPipelineBadge(status) {
    const badges = {
        'New': '<span class="px-2 py-0.5 text-[11px] font-medium bg-slate-100 text-slate-600 rounded">Yeni</span>',
        'PendingProduction': '<span class="px-2 py-0.5 text-[11px] font-medium bg-amber-100 text-amber-800 rounded">Üretim Bekliyor</span>',
        'InProduction': '<span class="px-2 py-0.5 text-[11px] font-medium bg-orange-100 text-orange-800 rounded">Üretimde</span>',
        'QualityControl': '<span class="px-2 py-0.5 text-[11px] font-medium bg-purple-100 text-purple-800 rounded">Kalite Kontrol</span>',
        'ReadyToShip': '<span class="px-2 py-0.5 text-[11px] font-medium bg-cyan-100 text-cyan-800 rounded">Kargoya Hazır</span>',
        'Shipped': '<span class="px-2 py-0.5 text-[11px] font-medium bg-blue-100 text-blue-800 rounded">Kargolandı</span>',
        'Delivered': '<span class="px-2 py-0.5 text-[11px] font-medium bg-emerald-100 text-emerald-800 rounded">Teslim Edildi</span>',
        'OnHold': '<span class="px-2 py-0.5 text-[11px] font-medium bg-rose-100 text-rose-800 rounded">Beklemede</span>',
        'ActionRequired': '<span class="px-2 py-0.5 text-[11px] font-medium bg-red-100 text-red-800 rounded">Aksiyon Gerekli</span>'
    };
    return badges[status] || `<span class="px-2 py-0.5 text-[11px] font-medium bg-slate-100 text-slate-600 rounded">${status}</span>`;
}

function getStatusBadge(status, customStatus) {
    return `
        <div class="flex flex-col gap-1 items-start">
            ${getEtsyStatusBadge(status)}
            ${getCustomPipelineBadge(customStatus)}
        </div>
    `;
}
