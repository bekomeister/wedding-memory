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
        // Basit şifre kontrolü için
        private const string AdminPassword = "beko123";
        private readonly FirestoreDb _firestore;

        public AdminController()
        {
            var credentialPath = GetFirebaseCredentialPath();
            var credential = GoogleCredential.FromFile(credentialPath);
            var builder = new FirestoreDbBuilder
            {
                ProjectId = "wedding-memory-46705",
                Credential = credential
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
            if (password == AdminPassword)
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
        public async Task<IActionResult> AddWedding(string brideName, string groomName)
        {
            if (HttpContext.Session.GetString("IsAdmin") != "true")
                return RedirectToAction("Login");
            var docRef = _firestore.Collection("weddings").Document();
            var wedding = new Wedding
            {
                Id = docRef.Id,
                BrideName = brideName,
                GroomName = groomName,
                CreatedAt = DateTime.UtcNow,
                Theme = "classic" // Varsayılan tema
            };
            await docRef.SetAsync(wedding);
            return RedirectToAction("Index");
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
            var storage = StorageClient.Create();
            string bucketName = "wedding-memory-46705.firebasestorage.com";
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
    }
} 