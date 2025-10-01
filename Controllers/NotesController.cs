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

        // GET: /Notes
        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? faculty, string? classLevel)
        {
            // --- Base query
            var query = _db.NoteItems
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .AsQueryable();

            // --- Search
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    (x.Title != null && x.Title.Contains(term)) ||
                    (x.Description != null && x.Description.Contains(term)) ||
                    (x.StudentDisplayName != null && x.StudentDisplayName.Contains(term)) ||
                    (x.Department != null && x.Department.Contains(term)) ||
                    (x.CourseCode != null && x.CourseCode.Contains(term)) ||
                    (x.CourseName != null && x.CourseName.Contains(term)));
            }

            // --- Filters
            if (!string.IsNullOrWhiteSpace(faculty))
                query = query.Where(x => x.Faculty == faculty.Trim());

            if (!string.IsNullOrWhiteSpace(classLevel))
                query = query.Where(x => x.ClassLevel == classLevel.Trim());

            var items = await query.ToListAsync();

            // --- Dropdown kaynakları (null/boş ayıklanmış)
            var faculties = await _db.NoteItems
                .AsNoTracking()
                .Select(x => x.Faculty)
                .Where(s => s != null && s != "")
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var classLevels = await _db.NoteItems
                .AsNoTracking()
                .Select(x => x.ClassLevel)
                .Where(s => s != null && s != "")
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

        // Örnek veri tohumlama: POST /seeditems
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
                        CourseCode="YMH219", CourseName="Nesne Tabanlı Programlama"
                    },
                    new NoteItem {
                        Title="EMRE", Description="emre",
                        StudentDisplayName="EMRE", StudentUserName="emre",
                        Faculty="Mühendislik ve Doğa Bilimleri Fakültesi",
                        Department="Bilgisayar Mühendisliği",
                        ClassLevel="3. Sınıf",
                        CourseCode="MAT205", CourseName="Diferansiyel Denklemler"
                    },
                    new NoteItem {
                        Title="ÖRNEK NOT - HOŞ GELDİN", Description="Genel paylaşım alanına örnek içerik.",
                        StudentDisplayName="ÖRNEK", StudentUserName="ornek",
                        Faculty="Mühendislik ve Doğa Bilimleri Fakültesi",
                        Department="Bilgisayar Mühendisliği",
                        ClassLevel="1. Sınıf",
                        CourseCode="GEN101", CourseName="Üniversiteye Giriş"
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
