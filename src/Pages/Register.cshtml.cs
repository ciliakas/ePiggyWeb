using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ePiggyWeb.Authentication;
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


        public async Task<IActionResult> OnPost()
        {
            if (!Password.Equals(PasswordConfirm))
            {
                ErrorMessage = "Passwords did not match!";
                return Page();
            }

             if (UserAuth.Registration(Email, Password))
             {
                 var claims = new List<Claim>
                 {
                     new Claim(ClaimTypes.UserData, "0"),
                     new Claim(ClaimTypes.Email, Email)
                 };
                 var claimsIdentity = new ClaimsIdentity(claims, "Login");
                 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                     new ClaimsPrincipal(claimsIdentity));
 
                 return Redirect("/Index");
 
 
             };
 
 
             ErrorMessage = "Invalid E-mail or Password!";
             return Page();
        }
    }
}

