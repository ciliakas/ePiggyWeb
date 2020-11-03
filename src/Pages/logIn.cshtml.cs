using ePiggyWeb.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class LoginModel : PageModel
    {
        public void OnGet()
        {
            ErrorMessage = "";
        }

        [BindProperty] 
        public string Email { get; set; }
        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage = "";
        public void OnPost()
        {
            if (UserAuth.Login(Email, Password))
            {
                Response.Redirect("/Index");
            }

            ErrorMessage = "Invalid E-mail or Password!";
            //  Debug.WriteLine("\n\n\n"+ Email + Password + UserAuth.Login(Email, Password));
        }
    }
}
