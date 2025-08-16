# Wedding Memory - Düğün Fotoğraf Paylaşım Uygulaması

Bu uygulama, düğün fotoğraflarını paylaşmak için tasarlanmış bir ASP.NET Core web uygulamasıdır.

## 🚀 Özellikler

- 🔥 Firebase Firestore veritabanı entegrasyonu
- 📁 Firebase Storage dosya yükleme
- 📱 QR kod oluşturma
- 👨‍💼 Admin paneli
- 📸 Çoklu dosya yükleme
- 🎨 Responsive tasarım
- 🔒 Güvenli admin authentication
- 🌍 Dinamik etkinlik türleri (Düğün, Nişan, Söz, Sünnet, Kına)

## 🛠️ Teknolojiler

- ASP.NET Core 9.0
- Firebase Admin SDK
- Google Cloud Storage
- QRCoder
- Bootstrap 5

## 🚀 Deployment

### Render'da Deployment

#### 1. Render Dashboard'a Giriş
- [Render Dashboard](https://dashboard.render.com)'a giriş yapın
- "New +" butonuna tıklayın
- "Web Service" seçin

#### 2. GitHub Repository Bağlantısı
- GitHub repository'nizi bağlayın
- Repository'yi seçin

#### 3. Servis Ayarları
- **Name**: `wedding-memory` (veya istediğiniz isim)
- **Environment**: `Docker`
- **Region**: Size en yakın bölgeyi seçin
- **Branch**: `main` (veya ana branch'iniz)

#### 4. Environment Variables (ZORUNLU)
Aşağıdaki environment variable'ları ekleyin:

```bash
# Firebase service account key (ZORUNLU)
FIREBASE_KEY_JSON={"type":"service_account","project_id":"your_project_id",...}

# Admin paneli şifresi (ZORUNLU - güvenlik için)
ADMIN_PASSWORD=your_secure_admin_password_here
```

**⚠️ GÜVENLİK UYARISI:** 
- Firebase key JSON dosyanızın tüm içeriğini `FIREBASE_KEY_JSON` olarak yapıştırın
- Admin şifresi en az 12 karakter, büyük/küçük harf, sayı ve özel karakter içermeli
- Bu bilgileri asla GitHub'a yüklemeyin!

#### 5. Build & Deploy
- "Create Web Service" butonuna tıklayın
- Render otomatik olarak Dockerfile'ı kullanarak build edecek
- Deployment tamamlandığında URL'iniz hazır olacak

### Railway'de Deployment

#### 1. Railway Dashboard'a Giriş
- [Railway Dashboard](https://railway.app)'a giriş yapın
- "New Project" butonuna tıklayın
- "Deploy from GitHub repo" seçin

#### 2. Environment Variables
Railway'de aynı environment variable'ları ekleyin:
- `FIREBASE_KEY_JSON`
- `ADMIN_PASSWORD`

## 💻 Yerel Geliştirme

### Gereksinimler
- .NET 9.0 SDK
- Firebase projesi ve service account key

### Kurulum
1. Repository'yi klonlayın
2. `firebase-key.json` dosyasını proje root'una ekleyin
3. `appsettings.Development.json` dosyasını oluşturun:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AdminPassword": "your_development_password"
}
```

4. `dotnet restore` komutunu çalıştırın
5. `dotnet run` ile uygulamayı başlatın

## 📖 Kullanım

### Admin Paneli
- `/Admin/Login` - Admin girişi (şifre environment variable'dan alınır)
- `/Admin` - Düğün çiftleri listesi ve yönetimi
- `/Admin/Qr/{id}` - QR kod oluşturma
- `/Admin/Files/{id}` - Yüklenen dosyaları görüntüleme

### Kullanıcı Yükleme
- `/Upload/{id}` - Fotoğraf ve video yükleme sayfası

## 🔒 Güvenlik

### Kritik Güvenlik Notları
- **Admin şifresini production'da mutlaka değiştirin**
- **Firebase key'inizi asla GitHub'a yüklemeyin**
- **HTTPS kullanın**
- **Environment variable kullanın**
- **Güçlü şifreler seçin**

### Güvenlik Özellikleri
- ✅ Şifreler kod içinde saklanmaz
- ✅ Environment variable desteği
- ✅ Session-based authentication
- ✅ Firebase güvenlik kuralları
- ✅ Dosya yükleme güvenliği

## 🌍 Environment Variables

### Production'da Zorunlu
```bash
# Admin paneli şifresi (güvenli olmalı)
ADMIN_PASSWORD=Wedding2024!@#$%^&*()

# Firebase service account key
FIREBASE_KEY_JSON={"type":"service_account","project_id":"your_project",...}
```

### Development'ta Opsiyonel
```json
// appsettings.Development.json
{
  "AdminPassword": "dev_password_123"
}
```

## 📁 Proje Yapısı

```
wedding-memory/
├── Controllers/          # MVC Controllers
├── Models/              # Data Models
├── Views/               # Razor Views
├── wwwroot/            # Static Files
├── Dockerfile          # Docker configuration
└── appsettings.json    # Configuration
```

## 🚨 Güvenlik Kontrol Listesi

- [ ] Admin şifresi environment variable'da
- [ ] Firebase key GitHub'da yok
- [ ] HTTPS aktif
- [ ] Güçlü şifre kullanılıyor
- [ ] .gitignore güncel
- [ ] Hassas dosyalar commit edilmiyor

## 🆘 Destek

Herhangi bir güvenlik sorunu yaşarsanız:
1. Hemen admin şifresini değiştirin
2. Firebase key'i yenileyin
3. Environment variable'ları güncelleyin
4. Issue açın veya iletişime geçin

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

---

**⚠️ GÜVENLİK UYARISI: Bu uygulamayı production'da kullanmadan önce tüm güvenlik önlemlerini aldığınızdan emin olun!** 
