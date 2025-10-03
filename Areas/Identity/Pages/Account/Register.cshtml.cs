using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace NotApp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public RegisterModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Ad")]
            public string FirstName { get; set; } = "";

            [Required]
            [Display(Name = "Soyad")]
            public string LastName { get; set; } = "";

            [Required]
            [EmailAddress]
            [RegularExpression(@"^[^@\s]+@ozal\.edu\.tr$",
                ErrorMessage = "E-posta adresi ozal.edu.tr uzantılı olmalıdır.")]
            [Display(Name = "E-posta")]
            public string Email { get; set; } = "";

            [Required]
            [RegularExpression(@"^[1-9][0-9]{9}$",
                ErrorMessage = "Telefon numarası 10 haneli olmalı ve başında 0 olmamalıdır. Örn: 5051234567")]
            [Display(Name = "Telefon")]
            public string PhoneNumber { get; set; } = "";
        }

        public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            if (!ModelState.IsValid) return Page();

            // Şifresiz kullanıcı oluştur
            var user = new IdentityUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return Page();
            }

            // Ad/Soyad claim ekle
            await _userManager.AddClaimsAsync(user, new[]
            {
                new Claim(ClaimTypes.GivenName, Input.FirstName),
                new Claim(ClaimTypes.Surname,  Input.LastName)
            });

            // Şifre belirleme tokeni ve linki
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = user.Email, returnUrl = ReturnUrl },
                protocol: Request.Scheme)!;

            // E-postayı gönder; hata olursa 500 yerine kullanıcıya mesaj ver
            try
            {
                var htmlBody = $@"Merhaba {Input.FirstName},

Hesabınız oluşturuldu. Şifrenizi belirlemek için aşağıdaki bağlantıya tıklayın:

<a href=""{callbackUrl}"">Şifre Belirle</a>

Eğer bu işlemi siz başlatmadıysanız bu mesajı yok sayabilirsiniz.";

                await _emailSender.SendEmailAsync(
                    user.Email!,
                    "MTÜ Not/Ödev Platformu - Şifre Belirleme",
                    htmlBody);

                TempData["ok"] = "Hesap oluşturuldu. Lütfen e-postanızı kontrol edip şifrenizi belirleyin.";
            }
            catch
            {
                TempData["ok"] = "Hesap oluşturuldu; ancak e-posta gönderilemedi. Daha sonra 'Şifre Sıfırla' ile şifre oluşturabilirsiniz.";
            }

            return RedirectToPage("./Login");
        }
    }
}
