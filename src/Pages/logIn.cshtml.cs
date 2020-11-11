using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
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


        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/Index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl)
        {
            var id = UserDatabase.Authenticate(Email, Password);
            if (id > -1)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, id.ToString()),
                    new Claim(ClaimTypes.Email, Email)
                };
                var claimsIdentity = new ClaimsIdentity(claims, "Login");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return Redirect(returnUrl ?? "/Index");
            };

            ErrorMessage = "Invalid E-mail or Password!";
            return Page();
        }

        public IActionResult OnPostForgotPassword()
        {
            if (!ModelState.IsValid) return Page();
            var option = new CookieOptions() { Expires = DateTime.Now.AddMinutes(15) };
            Response.Cookies.Append("Email", Email, option);
            return RedirectToPage("/forgotPassword");
        }
    }
}
