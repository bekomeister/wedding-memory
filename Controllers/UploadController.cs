using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.IO;
using Google.Cloud.Firestore;
using Google.Apis.Auth.OAuth2;
using wedding_memory.Models;

namespace wedding_memory.Controllers
{
    public class UploadController : Controller
    {
        private readonly string bucketName = "wedding-memory-46705.firebasestorage.app"; // Firebase Storage bucket adını buraya yaz
        private readonly FirestoreDb _firestore;
        private readonly GoogleCredential _credential;

        public UploadController()
        {
            var credentialPath = GetFirebaseCredentialPath();
            _credential = GoogleCredential.FromFile(credentialPath)
                .CreateScoped(new[] {
                    "https://www.googleapis.com/auth/datastore",
                    "https://www.googleapis.com/auth/devstorage.full_control"
                });
            _firestore = new FirestoreDbBuilder
            {
                ProjectId = "wedding-memory-46705",
                Credential = _credential
            }.Build();
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

        [HttpGet("/Upload/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var doc = await _firestore.Collection("weddings").Document(id).GetSnapshotAsync();
            if (doc.Exists)
            {
                var wedding = doc.ConvertTo<Wedding>();
                ViewBag.BrideName = wedding.BrideName;
                ViewBag.GroomName = wedding.GroomName;
                ViewBag.Theme = wedding.Theme ?? "classic";
            }
            ViewBag.WeddingId = id;
            return View();
        }

        [HttpPost("/Upload/{id}")]
        public async Task<IActionResult> Index(string id, string userName, List<IFormFile> files)
        {
            var doc = await _firestore.Collection("weddings").Document(id).GetSnapshotAsync();
            if (doc.Exists)
            {
                var wedding = doc.ConvertTo<Wedding>();
                ViewBag.BrideName = wedding.BrideName;
                ViewBag.GroomName = wedding.GroomName;
                ViewBag.Theme = wedding.Theme ?? "classic";
            }
            ViewBag.WeddingId = id;

            if (string.IsNullOrWhiteSpace(userName))
            {
                ViewBag.Error = "Lütfen isminizi girin.";
                return View();
            }

            if (files == null || files.Count == 0)
            {
                ViewBag.Error = "Lütfen en az bir dosya seçin.";
                return View();
            }

            var storage = StorageClient.Create(_credential);
            foreach (var file in files)
            {
                var objectName = $"{id}/{userName}/{Path.GetFileName(file.FileName)}";
                using (var stream = file.OpenReadStream())
                {
                    await storage.UploadObjectAsync(bucketName, objectName, file.ContentType, stream);
                }
            }
            ViewBag.Success = "Dosyalar başarıyla yüklendi!";
            return View();
        }
    }
} 