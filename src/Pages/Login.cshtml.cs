using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [Required, EmailAddress(ErrorMessage = "Incorrect e-mail")]
        [BindProperty]
        public string EmailRecovery { get; set; }

        public string ErrorMessage = "";
        public bool FailedToSendEmail;

        private UserDatabase UserDatabase { get; }
        public LoginModel(UserDatabase userDatabase)
        {
            UserDatabase = userDatabase;
        }

        public IActionResult OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }

            if (Request.Cookies.ContainsKey("recoveryCode"))
            {
                return RedirectToPage("/forgotPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl)
        {
            var id = await UserDatabase.AuthenticateAsync(Email, Password);
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

                Response.Cookies.Delete("recoveryCode");
                Response.Cookies.Delete("Email");
                return Redirect(returnUrl ?? "/index");
            }

            ErrorMessage = "Invalid E-mail or Password!";
            return Page();
        }

        public IActionResult OnPostForgotPassword()
        {
            if (!ModelState.IsValid)
            {
                FailedToSendEmail = true;
                return Page();
            }
            var option = new CookieOptions() { Expires = DateTime.Now.AddMinutes(15) };
            Response.Cookies.Append("Email", EmailRecovery, option);
            return RedirectToPage("/forgotPassword");
        }
    }
}
