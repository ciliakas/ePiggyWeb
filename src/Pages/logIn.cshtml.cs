using System.Diagnostics;
using ePiggyWeb.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
        }

        [BindProperty] 
        public string Email { get; set; }
        public string Password { get; set; }
        public void OnPost()
        {
            Debug.WriteLine("\n\n\n" + UserAuth.Login(Email, Password));
        }
    }
}
