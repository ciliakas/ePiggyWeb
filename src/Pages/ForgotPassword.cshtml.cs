using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace ePiggyWeb.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        [BindProperty]
        public string EnteredCode { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "Password must contain at least one uppercase letter, at least one number, special character and be longer than six characters.")]
        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        [BindProperty]
        public string Email { get; set; }

        public string ErrorMessage = "";

        public bool CodeSent;

        private Lazy<EmailSender> EmailSender { get; }
        private UserDatabase UserDatabase { get; }
        private IConfiguration Configuration { get; }
        public ForgotPasswordModel(UserDatabase userDatabase, IConfiguration configuration)
        {
            UserDatabase = userDatabase;
            Configuration = configuration;
            EmailSender = new Lazy<EmailSender>(() => new EmailSender());
        }

        public async Task<IActionResult> OnGet()
        {
            if (!Request.Cookies.ContainsKey("Email") || User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }

            if (Request.Cookies.ContainsKey("recoveryCode"))
            {
                return Page();
            }
            Email = Request.Cookies["Email"];

            Configuration.GetSection(Utilities.EmailSender.Email).Bind(EmailSender.Value);
            var recoveryCode = (await EmailSender.Value.SendRecoveryCodeAsync(Email)).ToString();

            var option = new CookieOptions() { Expires = DateTime.Now.AddMinutes(15) };
            Response.Cookies.Append("recoveryCode", recoveryCode, option);
            CodeSent = true;

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var value = Request.Cookies["recoveryCode"];
            if (string.Equals(EnteredCode, value))
            {
                if (string.Equals(Password, PasswordConfirm))
                {
                    await UserDatabase.ChangePasswordAsync(Email, Password);
                    Response.Cookies.Delete("recoveryCode");
                    Response.Cookies.Delete("Email");
                    return RedirectToPage("/login");
                }
                ErrorMessage = "Password did not match!";
            }
            else
            {
                ErrorMessage = "Wrong code!";
            }
            return Page();
        }

        public IActionResult OnPostCancel()
        {
            Response.Cookies.Delete("recoveryCode");
            Response.Cookies.Delete("Email");
            return RedirectToPage("/login");
        }

    }
}
