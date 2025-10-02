using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotApp.Data;
using NotApp.Models;

namespace NotApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _db;

        // 20 MB sınır, kabul edilebilir uzantılar
        private static readonly string[] AllowedExt = [".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"];
        private const long MaxFileBytes = 20 * 1024 * 1024;

        public NotesController(ApplicationDbContext db) => _db = db;

        // -------------------- LISTE --------------------
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? faculty, string? classLevel)
        {
            var query = _db.NoteItems
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    (x.Title ?? "").Contains(term) ||
                    (x.Description ?? "").Contains(term) ||
                    (x.StudentDisplayName ?? "").Contains(term) ||
                    (x.Department ?? "").Contains(term) ||
                    (x.CourseCode ?? "").Contains(term) ||
                    (x.CourseName ?? "").Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(faculty))
                query = query.Where(x => x.Faculty == faculty.Trim());

            if (!string.IsNullOrWhiteSpace(classLevel))
                query = query.Where(x => x.ClassLevel == classLevel.Trim());

            var items = await query.ToListAsync();

            var faculties = await _db.NoteItems.AsNoTracking()
                .Select(x => x.Faculty)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var classLevels = await _db.NoteItems.AsNoTracking()
                .Select(x => x.ClassLevel)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var vm = new NotesIndexVM
            {
                Items = items,
                Q = q,
                Faculty = faculty,
                ClassLevel = classLevel,
                Faculties = faculties,
                ClassLevels = classLevels
            };

            return View(vm);
        }

        // -------------------- YARDIMCI: DROPDOWN --------------------
        private async Task FillDropdownsAsync()
        {
            ViewBag.Faculties = await _db.NoteItems.AsNoTracking()
                .Select(x => x.Faculty)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewBag.ClassLevels = await _db.NoteItems.AsNoTracking()
                .Select(x => x.ClassLevel)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        // -------------------- CREATE (GET) --------------------
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await FillDropdownsAsync();
            return View(new NoteItem());
        }

        // -------------------- CREATE (POST) --------------------
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NoteItem model, IFormFile? file)
        {
            // Dosya kontrolü (varsa)
            if (file is not null && file.Length > 0)
            {
                if (file.Length > MaxFileBytes)
                    ModelState.AddModelError(nameof(file), "Dosya boyutu en fazla 20 MB olabilir.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExt.Contains(ext))
                    ModelState.AddModelError(nameof(file), $"Bu dosya türü desteklenmiyor ({ext}).");
            }

            if (!ModelState.IsValid)
            {
                await FillDropdownsAsync();
                return View(model);
            }

            // Dosya yükleme
            if (file is not null && file.Length > 0)
            {
                var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsRoot);

                var baseName = Path.GetFileNameWithoutExtension(file.FileName);
                var ext = Path.GetExtension(file.FileName);
                var finalName = $"{Sanitize(baseName)}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadsRoot, finalName);

                using var stream = System.IO.File.Create(fullPath);
                await file.CopyToAsync(stream);

                model.FileUrl = $"/uploads/{finalName}";
            }

            model.CreatedAt = DateTime.UtcNow;

            _db.NoteItems.Add(model);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Not başarıyla yüklendi.";
            return RedirectToAction(nameof(Index));
        }

        // Basit dosya adı temizleyici
        private static string Sanitize(string input)
        {
            // Türkçe karakterleri ve boşlukları sadeleştir
            var s = input.Trim()
                         .Replace(' ', '_')
                         .Replace('ç', 'c').Replace('Ç', 'C')
                         .Replace('ğ', 'g').Replace('Ğ', 'G')
                         .Replace('ı', 'i').Replace('İ', 'I')
                         .Replace('ö', 'o').Replace('Ö', 'O')
                         .Replace('ş', 's').Replace('Ş', 'S')
                         .Replace('ü', 'u').Replace('Ü', 'U');

            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch, '_');

            return string.IsNullOrWhiteSpace(s) ? "dosya" : s;
        }

        // -------------------- SEED (örnek veri) --------------------
        [HttpPost("/seeditems")]
        public async Task<IActionResult> Seed()
        {
            if (!await _db.NoteItems.AnyAsync())
            {
                var sample = new List<NoteItem>
                {
                    new NoteItem {
                        Title="SUDE", Description="sude",
                        StudentDisplayName="SUDE", StudentUserName="sude",
                        Faculty="Mühendislik ve Doğa Bilimleri Fakültesi",
                        Department="Bilgisayar Mühendisliği",
                        ClassLevel="4. Sınıf",
                        CourseCode="YMH219", CourseName="Nesne Tabanlı Programlama",
                        CreatedAt=DateTime.UtcNow
                    },
                    new NoteItem {
                        Title="EMRE", Description="emre",
                        StudentDisplayName="EMRE", StudentUserName="emre",
                        Faculty="Mühendislik ve Doğa Bilimleri Fakültesi",
                        Department="Bilgisayar Mühendisliği",
                        ClassLevel="3. Sınıf",
                        CourseCode="MAT205", CourseName="Diferansiyel Denklemler",
                        CreatedAt=DateTime.UtcNow
                    },
                    new NoteItem {
                        Title="ÖRNEK NOT - HOŞ GELDİN", Description="Genel paylaşım alanına örnek içerik.",
                        StudentDisplayName="ÖRNEK", StudentUserName="ornek",
                        Faculty="Mühendislik ve Doğa Bilimleri Fakültesi",
                        Department="Bilgisayar Mühendisliği",
                        ClassLevel="1. Sınıf",
                        CourseCode="GEN101", CourseName="Üniversiteye Giriş",
                        CreatedAt=DateTime.UtcNow
                    }
                };

                _db.NoteItems.AddRange(sample);
                await _db.SaveChangesAsync();
            }

            return Ok(new { ok = true });
        }
    }

    // --- ViewModel ---
    public class NotesIndexVM
    {
        public List<NoteItem> Items { get; set; } = new();
        public string? Q { get; set; }
        public string? Faculty { get; set; }
        public string? ClassLevel { get; set; }
        public List<string> Faculties { get; set; } = new();
        public List<string> ClassLevels { get; set; } = new();
    }
}
