using Google.Cloud.Firestore;
using System;

namespace wedding_memory.Models
{
    [FirestoreData]
    public class Wedding
    {
        public Wedding() {}

        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string? BrideName { get; set; }

        [FirestoreProperty]
        public string? GroomName { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty]
        public string? Theme { get; set; } = "classic"; // Varsayılan tema
    }
} 