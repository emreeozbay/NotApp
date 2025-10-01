using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotApp.Models;

namespace NotApp.Data
{
    // Identity tabloları için IdentityDbContext'ten türetiyoruz
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<NoteItem> NoteItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity şeması vs. için mutlaka önce base
            base.OnModelCreating(modelBuilder);

            // Pomelo MySQL - varsayılan karakter seti ve kolasyon
            modelBuilder.HasCharSet("utf8mb4");
            modelBuilder.UseCollation("utf8mb4_unicode_ci");
        }
    }
}
