using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        [Required]
        [BindProperty]
        public string EnteredCode { get; set; }

        [Required(ErrorMessage = "All fields required!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "Password must contain at least one uppercase letter, at least one number, special character and be longer than six characters.")]
        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [BindProperty]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        [BindProperty]
        public string Email { get; set; }

        public string ErrorMessage = "";

        public bool CodeSent;
        public bool Expired;


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
            var emailSender = new EmailSender();
            var recoveryCode = (await emailSender.SendRecoveryCodeAsync(Email)).ToString();

            var option = new CookieOptions() { Expires = DateTime.Now.AddMinutes(15) };
            Response.Cookies.Append("recoveryCode", recoveryCode, option);
            CodeSent = true;

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var value = Request.Cookies["recoveryCode"];
            if (string.Equals(EnteredCode, value ))
            {
                if (string.Equals(Password, PasswordConfirm))
                {
                    UserDatabase.ChangePassword(Email, Password);
                    Response.Cookies.Delete("recoveryCode");
                    Response.Cookies.Delete("Email");
                    return RedirectToPage("/login");
                }
                else
                {
                    ErrorMessage = "Password did not match!";
                }
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
