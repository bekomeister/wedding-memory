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
        private readonly string bucketName = "wedding-memory-46705.firebasestorage.app";
        private readonly FirestoreDb _firestore;
        private readonly GoogleCredential _credential;
        
        // Dosya sınırlamaları
        private const long MaxFileSize = 200 * 1024 * 1024; // 200MB
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".mp4", ".mov", ".avi", ".mkv", ".webm" };

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
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Error", "Home");
            }
            var doc = await _firestore.Collection("weddings").Document(id).GetSnapshotAsync();
            if (!doc.Exists)
            {
                return NotFound();
            }
            var wedding = doc.ConvertTo<Wedding>();
            ViewBag.BrideName = wedding.BrideName;
            ViewBag.GroomName = wedding.GroomName;
            ViewBag.Theme = wedding.Theme ?? "classic";
            ViewBag.BackgroundImageUrl = wedding.BackgroundImageUrl;
            ViewBag.EventType = wedding.EventType ?? "wedding";
            ViewBag.WeddingId = id;
            return View();
        }

        [HttpPost("/Upload/{id}")]
        public async Task<IActionResult> Index(string id, string userName, List<IFormFile> files)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Error", "Home");
            }
            var doc = await _firestore.Collection("weddings").Document(id).GetSnapshotAsync();
            if (!doc.Exists)
            {
                return NotFound();
            }
            var wedding = doc.ConvertTo<Wedding>();
            ViewBag.BrideName = wedding.BrideName;
            ViewBag.GroomName = wedding.GroomName;
            ViewBag.Theme = wedding.Theme ?? "classic";
            ViewBag.BackgroundImageUrl = wedding.BackgroundImageUrl;
            ViewBag.EventType = wedding.EventType ?? "wedding";
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

            if (files.Count > 15)
            {
                ViewBag.Error = "Bir seferde en fazla 15 dosya yükleyebilirsiniz.";
                return View();
            }

            // Dosya kontrolü
            foreach (var file in files)
            {
                if (file.Length > MaxFileSize)
                {
                    ViewBag.Error = $"Dosya boyutu çok büyük: {file.FileName}. Maksimum 200MB olmalıdır.";
                    return View();
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    ViewBag.Error = $"Desteklenmeyen dosya türü: {file.FileName}. Sadece JPG, PNG, GIF, WEBP, MP4, MOV, AVI, MKV, WEBM dosyaları kabul edilir.";
                    return View();
                }
            }

            try
            {
                var storage = StorageClient.Create(_credential);
                var uploadedFiles = new List<string>();

                foreach (var file in files)
                {
                    var objectName = $"{id}/{userName}/{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Path.GetFileName(file.FileName)}";
                    using (var stream = file.OpenReadStream())
                    {
                        await storage.UploadObjectAsync(bucketName, objectName, file.ContentType, stream);
                        uploadedFiles.Add(file.FileName);
                    }
                }
                
                ViewBag.Success = $"{uploadedFiles.Count} dosya başarıyla yüklendi!";
                ViewBag.UploadedFiles = uploadedFiles;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Dosya yükleme sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                // Log the error for debugging
                Console.WriteLine($"Upload error: {ex.Message}");
            }
            
            return View();
        }
    }
} 