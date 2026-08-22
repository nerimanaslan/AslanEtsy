# Netlify Dağıtım (Deployment) Kılavuzu

Bu klasör (`netlify-deploy`), Aslan Etsy Yönetim Paneli ön yüzünü (UI Dashboard) Netlify üzerine tek tıkla yüklemeniz için hazırlanmıştır.

---

## 🚀 Netlify'a Yükleme Adımları (2 Yöntem)

### Yöntem 1: Sürükle & Bırak (En Kolay)
1. **[app.netlify.com](https://app.netlify.com)** adresine gidin ve giriş yapın.
2. **Sites** sekmesine tıklayın.
3. Sayfanın altındaki **"Drag and drop your site output folder here"** alanına bu `netlify-deploy` klasörünü sürükleyip bırakın.
4. Saniyeler içinde siteniz canlıya alınacaktır!

### Yöntem 2: GitHub / Git Üzerinden
1. Projenizi GitHub'a push edin.
2. Netlify üzerinde **"Add new site" -> "Import an existing project"** seçeneğini seçin.
3. **Publish directory** kısmına `netlify-deploy` (veya `AslanEtsy.WebApi/wwwroot`) yazın.
4. **Deploy Site** butonuna basın.

---

## ⚙️ Backend API Bağlantısı Nasıl Yapılır?
- Siteniz Netlify üzerinde açıldıktan sonra sağ üst köşedeki **Çark (Ayarlar)** ikonuna tıklayın.
- **Backend API URL** alanına .NET Web API sunucunuzun adresini (Örn: `https://api.siteniz.com`) girip kaydedin.
- Artık Netlify üzerindeki arayüzünüz doğrudan arka plan API'niz ve Etsy ile haberleşecektir!
