using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
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

        [Required]
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


        public void OnGet()
        {
            Email = Request.Cookies["Email"];
            var recoveryCode = EmailSender.SendRecoveryCode(Email).ToString();

            Response.Cookies.Append("recoveryCode", recoveryCode);
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var value = Request.Cookies["recoveryCode"];
            Debug.WriteLine("\n\n\n\n\n1." + EnteredCode + " 2." + value);
            if (string.Equals(EnteredCode, value ))
            {
                if (string.Equals(Password, PasswordConfirm))
                {
                    Debug.WriteLine("\n\n\n\n\n" + Email + " " + Password);
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

    }
}
