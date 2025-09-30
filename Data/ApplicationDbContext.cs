using Microsoft.EntityFrameworkCore;

namespace NotApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Note> Notes => Set<Note>();

        // ApplicationDbContext içinde:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pomelo EF Core 9 ile önerilen yol: global charset/collation'ı model builder üzerinden ver.
            modelBuilder.HasCharSet("utf8mb4");
            modelBuilder.UseCollation("utf8mb4_unicode_ci");
        }
    }

    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
