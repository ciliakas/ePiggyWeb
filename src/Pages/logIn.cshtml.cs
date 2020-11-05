using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] 
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage = "";

        public void OnGet()
        {
            ErrorMessage = "";
        }
        public async Task<IActionResult> OnPost(string ReturnUrl)
        {
            var Id = UserAuth.Login(Email, Password);
            if (Id > -1)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, Id.ToString()),
                    new Claim(ClaimTypes.Email, Email)
                };
                var claimsIdentity = new ClaimsIdentity(claims, "Login");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Redirect(ReturnUrl ?? "/Index");
                

            };


            ErrorMessage = "Invalid E-mail or Password!";
            return Page();
        }
    }
}
