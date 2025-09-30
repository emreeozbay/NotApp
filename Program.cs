using Microsoft.EntityFrameworkCore;
using NotApp.Data;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs))); // CharSetBehavior yok

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapPost("/seed", async (ApplicationDbContext db) =>
{
    db.Notes.Add(new Note { Title = "Merhaba MySQL", Content = "İlk kayıt" });
    await db.SaveChangesAsync();
    return Results.Ok();
});
app.MapGet("/notes", async (ApplicationDbContext db) =>
    await db.Notes.OrderByDescending(n => n.Id).ToListAsync());

app.Run();
