using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class RegisterModel : PageModel
    {
        [Required, EmailAddressAttribute(ErrorMessage = "Incorrect e-mail")]
        [BindProperty]
        public string Email { get; set; }

        [Required(ErrorMessage = "All fields required!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "Password must contain at least one uppercase letter, at least one number, special character and be longer than six characters.")]
        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [BindProperty]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        public string ErrorMessage = "";

        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        public RegisterModel(UserDatabase userDatabase, EmailSender emailSender)
        {
            UserDatabase = userDatabase;
            EmailSender = emailSender;
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

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (!Password.Equals(PasswordConfirm))
            {
                ErrorMessage = "Passwords did not match!";
                return Page();
            }

            var id = await UserDatabase.RegisterAsync(Email, Password);
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

                await EmailSender.SendGreetingEmailAsync(Email);

                return Redirect("/index");
            }

            ErrorMessage = "Such User already exists";
            return Page();
        }
    }
}

