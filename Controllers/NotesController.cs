using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotApp.Data;
using NotApp.Models;

namespace NotApp.Controllers
{
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _db;

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
                .Distinct().OrderBy(s => s).ToListAsync();

            var classLevels = await _db.NoteItems.AsNoTracking()
                .Select(x => x.ClassLevel)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct().OrderBy(s => s).ToListAsync();

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
                .Distinct().OrderBy(s => s).ToListAsync();

            ViewBag.ClassLevels = await _db.NoteItems.AsNoTracking()
                .Select(x => x.ClassLevel)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct().OrderBy(s => s).ToListAsync();
        }

        // -------------------- CREATE (GET) --------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await FillDropdownsAsync();
            return View(new NoteItem());
        }

        // -------------------- CREATE (POST) --------------------
// CREATE (POST) – dosya yüklemeli
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(NoteItem model, IFormFile? file)
{
    if (!ModelState.IsValid)
        return View(model);

    // Dosya yüklendiyse kaydet
    if (file != null && file.Length > 0)
    {
        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);

        var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var ext = Path.GetExtension(file.FileName);
        var finalName = $"{safeFileName}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, finalName);

        using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream);

        model.FileUrl = $"/uploads/{finalName}";
    }

    model.CreatedAt = DateTime.UtcNow;
    _db.NoteItems.Add(model);
    await _db.SaveChangesAsync();

    TempData["ok"] = "Not başarıyla yüklendi.";
    return RedirectToAction(nameof(Index));
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
