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

        public async Task<IActionResult> Index(string? q, string? faculty, string? classLevel)
        {
            var query = _db.NoteItems.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.Title.Contains(term) ||
                    (x.Description != null && x.Description.Contains(term)) ||
                    x.StudentDisplayName.Contains(term) ||
                    x.Department.Contains(term) ||
                    x.CourseCode.Contains(term) ||
                    x.CourseName.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(faculty))
                query = query.Where(x => x.Faculty == faculty);

            if (!string.IsNullOrWhiteSpace(classLevel))
                query = query.Where(x => x.ClassLevel == classLevel);

            query = query.OrderByDescending(x => x.Id);

            var items = await query.ToListAsync();

            var vm = new NotesIndexVM
            {
                Items = items,
                Q = q,
                Faculty = faculty,
                ClassLevel = classLevel,
                Faculties = await _db.NoteItems.Select(x => x.Faculty).Distinct().OrderBy(x => x).ToListAsync(),
                ClassLevels = await _db.NoteItems.Select(x => x.ClassLevel).Distinct().OrderBy(x => x).ToListAsync()
            };

            return View(vm);
        }

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
