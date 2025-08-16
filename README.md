# Wedding Memory - Düğün Fotoğraf Paylaşım Uygulaması

Bu uygulama, düğün fotoğraflarını paylaşmak için tasarlanmış bir ASP.NET Core web uygulamasıdır.

## Özellikler

- 🔥 Firebase Firestore veritabanı entegrasyonu
- 📁 Firebase Storage dosya yükleme
- 📱 QR kod oluşturma
- 👨‍💼 Admin paneli
- 📸 Çoklu dosya yükleme
- 🎨 Responsive tasarım

## Teknolojiler

- ASP.NET Core 9.0
- Firebase Admin SDK
- Google Cloud Storage
- QRCoder
- Bootstrap 5

## Render'da Deployment

### 1. Render Dashboard'a Giriş
- [Render Dashboard](https://dashboard.render.com)'a giriş yapın
- "New +" butonuna tıklayın
- "Web Service" seçin

### 2. GitHub Repository Bağlantısı
- GitHub repository'nizi bağlayın
- Repository'yi seçin

### 3. Servis Ayarları
- **Name**: `wedding-memory` (veya istediğiniz isim)
- **Environment**: `Docker`
- **Region**: Size en yakın bölgeyi seçin
- **Branch**: `main` (veya ana branch'iniz)

### 4. Environment Variables
Aşağıdaki environment variable'ları ekleyin:

```bash
# Firebase service account key (zorunlu)
FIREBASE_KEY_JSON={"type":"service_account","project_id":"wedding-memory-46705",...}

# Admin paneli şifresi (zorunlu - güvenlik için)
ADMIN_PASSWORD=your_secure_admin_password_here
```

**Önemli:** Firebase key JSON dosyanızın tüm içeriğini `FIREBASE_KEY_JSON` olarak yapıştırın.

### 5. Build & Deploy
- "Create Web Service" butonuna tıklayın
- Render otomatik olarak Dockerfile'ı kullanarak build edecek
- Deployment tamamlandığında URL'iniz hazır olacak

## Yerel Geliştirme

### Gereksinimler
- .NET 9.0 SDK
- Firebase projesi ve service account key

### Kurulum
1. Repository'yi klonlayın
2. `firebase-key.json` dosyasını proje root'una ekleyin
3. `appsettings.Development.json` dosyasını oluşturun (admin şifresi için)
4. `dotnet restore` komutunu çalıştırın
5. `dotnet run` ile uygulamayı başlatın

## Kullanım

### Admin Paneli
- `/Admin/Login` - Admin girişi (şifre environment variable'dan alınır)
- `/Admin` - Düğün çiftleri listesi
- `/Admin/Qr/{id}` - QR kod oluşturma
- `/Admin/Files/{id}` - Yüklenen dosyaları görüntüleme

### Kullanıcı Yükleme
- `/Upload/{id}` - Fotoğraf yükleme sayfası

## Güvenlik Notları

- **Admin şifresini production'da değiştirin**
- **Firebase key'inizi güvenli tutun**
- **HTTPS kullanın**
- **Environment variable kullanın:** `ADMIN_PASSWORD=your_secure_password`

## Environment Variables

Production'da aşağıdaki environment variable'ları ayarlayın:

```bash
# Admin paneli şifresi
ADMIN_PASSWORD=your_secure_password_here

# Firebase key
FIREBASE_KEY_JSON={"type":"service_account",...}
```

## Destek

Herhangi bir sorun yaşarsanız, lütfen issue açın veya iletişime geçin. 
