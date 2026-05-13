using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ExchangeRateDSP.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;


        public async Task<IActionResult> OnPostAsync()
        {
            // Hardcoded heslo podle zadání: pouze 1 uživatel, bez registrace
            if (Username == "admin" && Password == "Heslo1234.")
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, Username) };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity));

                return RedirectToPage("/Index");
            }

            ErrorMessage = "Neplatné jméno nebo heslo.";
            return Page();
        }
    }
}
