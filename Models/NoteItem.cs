using System;
using System.ComponentModel.DataAnnotations;

namespace NotApp.Models
{
    public class NoteItem
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public string StudentDisplayName { get; set; } = null!;
        public string StudentUserName   { get; set; } = null!;

        public string Faculty    { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string ClassLevel { get; set; } = null!;

        // Ders Kodu alanı kaldırıldı
        // public string CourseCode { get; set; } = null!;

        public string CourseName { get; set; } = null!;

        public string? FileUrl { get; set; }

        [Display(Name = "Belge Türü")]
        public DocumentType DocumentType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
