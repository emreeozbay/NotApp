using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotApp.Data;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext (MySQL kullanıyorsun)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// Identity servislerini ekle
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor Pages (Identity UI için)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // <--- önemli
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notes}/{action=Index}/{id?}");

app.MapRazorPages(); // <--- Identity için gerekli

app.Run();
