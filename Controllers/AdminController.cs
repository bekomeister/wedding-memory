using Microsoft.AspNetCore.Mvc;
using wedding_memory.Models;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using System.Threading.Tasks;
using System.Linq;
using Google.Cloud.Storage.V1;
using QRCoder;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace wedding_memory.Controllers
{
    public class AdminController : Controller
    {
        // Admin şifresi configuration'dan alınır
        private readonly string _adminPassword;
        private readonly FirestoreDb _firestore;
        private readonly GoogleCredential _credential;

        public AdminController(IConfiguration configuration)
        {
            _adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? 
                           configuration["AdminPassword"] ?? 
                           "beko123";
            
            var credentialPath = GetFirebaseCredentialPath();
            _credential = GoogleCredential.FromFile(credentialPath)
                .CreateScoped(new[] {
                    "https://www.googleapis.com/auth/datastore",
                    "https://www.googleapis.com/auth/devstorage.full_control"
                });
            var builder = new FirestoreDbBuilder
            {
                ProjectId = "wedding-memory-46705",
                Credential = _credential
            };
            _firestore = builder.Build();
        }

        private string GetFirebaseCredentialPath()
        {
            var firebaseKeyJson = Environment.GetEnvironmentVariable("FIREBASE_KEY_JSON");
            if (!string.IsNullOrEmpty(firebaseKeyJson))
            {
                var tempPath = Path.GetTempFileName();
                System.IO.File.WriteAllText(tempPath, firebaseKeyJson);
                return tempPath;
            }
            return Path.Combine(Directory.GetCurrentDirectory(), "firebase-key.json");
        }

        // Giriş sayfası
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string password)
        {
            if (password == _adminPassword)
            {
                // Giriş başarılı, session ile işaretle
                HttpContext.Session.SetString("IsAdmin", "true");
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Şifre yanlış!";
            return View();
        }

        // Admin ana sayfası (çift listesi ve ekleme)
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            var weddings = new List<Wedding>();
            var snapshot = await _firestore.Collection("weddings").OrderBy("CreatedAt").GetSnapshotAsync();
            foreach (var doc in snapshot.Documents)
            {
                var wedding = doc.ConvertTo<Wedding>();
                wedding.Id = doc.Id;
                weddings.Add(wedding);
            }
            return View(weddings);
        }

        // Yeni çift ekleme
        [HttpPost]
        public async Task<IActionResult> AddWedding(string brideName, string groomName, string eventType)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");

            try
            {
                // Benzersiz ID oluştur: gelin-damat-tarih
                var baseId = $"{NormalizeName(brideName)}-{NormalizeName(groomName)}-{DateTime.UtcNow:yyyyMMdd}";
                var uniqueId = await GenerateUniqueId(baseId);
                
                var docRef = _firestore.Collection("weddings").Document(uniqueId);
                var wedding = new Wedding
                {
                    Id = uniqueId,
                    BrideName = brideName,
                    GroomName = groomName,
                    CreatedAt = DateTime.UtcNow,
                    Theme = "classic", // Varsayılan tema
                    EventType = eventType ?? "wedding" // Varsayılan etkinlik türü
                };
                await docRef.SetAsync(wedding);
                
                TempData["Success"] = $"Çift başarıyla eklendi! ID: {uniqueId}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Çift eklenirken hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        // İsim normalizasyonu (Türkçe karakterler ve özel karakterler için)
        private string NormalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "unknown";
            
            return name
                .ToLowerInvariant()
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ı", "i")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u")
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("!", "")
                .Replace("?", "")
                .Replace("&", "and")
                .Replace("+", "plus")
                .Replace("=", "")
                .Replace("@", "")
                .Replace("#", "")
                .Replace("$", "")
                .Replace("%", "")
                .Replace("^", "")
                .Replace("*", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Replace("|", "")
                .Replace("\\", "")
                .Replace("/", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("<", "")
                .Replace(">", "")
                .Replace(",", "")
                .Replace("`", "")
                .Replace("~", "")
                .Trim('-');
        }

        // Benzersiz ID oluştur
        private async Task<string> GenerateUniqueId(string baseId)
        {
            var uniqueId = baseId;
            var counter = 1;
            
            // ID zaten varsa sayı ekle
            while (true)
            {
                var doc = await _firestore.Collection("weddings").Document(uniqueId).GetSnapshotAsync();
                if (!doc.Exists)
                {
                    break; // Benzersiz ID bulundu
                }
                
                uniqueId = $"{baseId}-{counter}";
                counter++;
                
                // Sonsuz döngüyü önle
                if (counter > 100)
                {
                    // Fallback: timestamp ile benzersiz yap
                    uniqueId = $"{baseId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                    break;
                }
            }
            
            return uniqueId;
        }

        // Tema güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateTheme(string id, string theme)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            
            var docRef = _firestore.Collection("weddings").Document(id);
            await docRef.UpdateAsync("Theme", theme);
            return RedirectToAction("Index");
        }

        // Etkinlik türü güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateEventType(string id, string eventType)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            
            var docRef = _firestore.Collection("weddings").Document(id);
            await docRef.UpdateAsync("EventType", eventType);
            return RedirectToAction("Index");
        }

        // QR kod ve yükleme linki gösterimi
        public IActionResult Qr(string id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            string url = Url.Action("Index", "Upload", new { id = id }, Request.Scheme);
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q))
            {
                var pngQr = new PngByteQRCode(qrData);
                var qrBytes = pngQr.GetGraphic(20);
                string base64 = Convert.ToBase64String(qrBytes);
                ViewBag.QrCode = base64;
                ViewBag.Link = url;
            }
            return View();
        }

        public async Task<IActionResult> Files(string id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            var storage = StorageClient.Create(_credential);
            string bucketName = "wedding-memory-46705.appspot.com";
            var files = new List<string>();
            foreach (var obj in storage.ListObjects(bucketName, id + "/"))
            {
                // Public URL oluştur
                string url = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(obj.Name)}?alt=media";
                files.Add(url);
            }
            ViewBag.WeddingId = id;
            return View(files);
        }

        // Arkaplan görseli yükleme
        [HttpPost]
        public async Task<IActionResult> UploadBackground(string id, IFormFile backgroundImage)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            if (backgroundImage == null || backgroundImage.Length == 0)
                return RedirectToAction("Index");

            // Sadece resim dosyalarına izin ver
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(backgroundImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                return RedirectToAction("Index");

            // Görsel oranı kontrolü kaldırıldı, her türlü görsel yüklenebilir

            try
            {
                // İçerik türünü belirle (bazı istemciler null gönderebilir)
                string contentType = backgroundImage.ContentType;
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    contentType = ext switch
                    {
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".webp" => "image/webp",
                        _ => "application/octet-stream"
                    };
                }

                // Firebase Storage'a yükle
                var storage = StorageClient.Create(_credential);
                string bucketName = "wedding-memory-46705.appspot.com";
                string objectName = $"{id}/background{ext}";
                using (var stream = backgroundImage.OpenReadStream())
                {
                    await storage.UploadObjectAsync(bucketName, objectName, contentType, stream);
                }
                string url = $"https://firebasestorage.googleapis.com/v0/b/{bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media&t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

                // Firestore'a URL'i kaydet
                var docRef = _firestore.Collection("weddings").Document(id);
                await docRef.UpdateAsync("BackgroundImageUrl", url);
            }
            catch (Exception ex)
            {
                // Platform loglarına ayrıntı yaz
                Console.WriteLine($"UploadBackground error: {ex}");
                TempData["Error"] = "Arkaplan yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
            }

            return RedirectToAction("Index");
        }

        // Çift silme
        [HttpPost]
        public async Task<IActionResult> DeleteWedding(string id)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            var docRef = _firestore.Collection("weddings").Document(id);
            await docRef.DeleteAsync();

            // Firebase Storage'dan ilgili wedding id klasöründeki tüm dosyaları sil
            var storage = StorageClient.Create(_credential);
            string bucketName = "wedding-memory-46705.appspot.com";
            var objects = storage.ListObjects(bucketName, id + "/");
            foreach (var obj in objects)
            {
                await storage.DeleteObjectAsync(bucketName, obj.Name);
            }

            return RedirectToAction("Index");
        }


    }
} 