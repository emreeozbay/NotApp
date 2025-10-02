using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotApp.Data;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext (MySQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// Identity
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC + Razor Pages (Identity UI)
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();   // 🔒 Sadece prod'da
}
else
{
    // Development
    // app.UseDeveloperExceptionPage();  // istersen aç
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Varsayılan rota: Notes/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Notes}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
