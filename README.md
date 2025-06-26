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

```
FIREBASE_KEY_JSON={"type":"service_account","project_id":"wedding-memory-46705",...}
```

Firebase key JSON dosyanızın tüm içeriğini buraya yapıştırın.

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
3. `dotnet restore` komutunu çalıştırın
4. `dotnet run` ile uygulamayı başlatın

## Kullanım

### Admin Paneli
- `/Admin/Login` - Admin girişi (şifre: beko123)
- `/Admin` - Düğün çiftleri listesi
- `/Admin/Qr/{id}` - QR kod oluşturma
- `/Admin/Files/{id}` - Yüklenen dosyaları görüntüleme

### Kullanıcı Yükleme
- `/Upload/{id}` - Fotoğraf yükleme sayfası

## Güvenlik Notları

- Admin şifresini production'da değiştirin
- Firebase key'inizi güvenli tutun
- HTTPS kullanın

## Destek

Herhangi bir sorun yaşarsanız, lütfen issue açın veya iletişime geçin. 
