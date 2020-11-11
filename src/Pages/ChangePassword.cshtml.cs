using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ChangePasswordModel : PageModel
    {
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", ErrorMessage = "Password must contain at least one uppercase letter, at least one number, special character and be longer than six characters.")]
        [BindProperty]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [BindProperty]
        [DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }

        public string ErrorMessage = "";

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.Equals(Password, PasswordConfirm))
            {
                UserDatabase.ChangePassword(User.FindFirst(ClaimTypes.Email).Value, Password);
                return RedirectToPage("/index");
            }
            else
            {
                ErrorMessage = "Passwords did not match!";
                return Page();
            }
        }
    }
}
