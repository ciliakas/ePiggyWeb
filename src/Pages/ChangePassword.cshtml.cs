using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Pages.ErrorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ILogger<ChangePasswordModel> _logger;
        public bool WasException { get; set; }

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
        public ChangePasswordModel(UserDatabase userDatabase, ILogger<ChangePasswordModel> logger)
        {
            UserDatabase = userDatabase;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                if (string.Equals(Password, PasswordConfirm))
                {
                    await UserDatabase.ChangePasswordAsync(User.FindFirst(ClaimTypes.Email).Value, Password);
                    return RedirectToPage("/index");
                }
                else
                {
                    ErrorMessage = "Passwords did not match!";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return Page();
            }
           
        }
    }
}
