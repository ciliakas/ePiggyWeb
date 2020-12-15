using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ePiggyWeb.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ILogger<ChangePasswordModel> _logger;

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
        
        [BindProperty]
        public string Currency { get; set; }
        [BindProperty] 
        public bool Recalculate { get; set; }

        [BindProperty]
        public List<string> CurrencyOptions { get; private set; }
        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        private CurrencyConverter CurrencyConverter { get;}
        public UserModel UserModel { get; private set; }

        public ChangePasswordModel(PiggyDbContext piggyDbContext, IOptions<EmailSender> emailSenderSettings, ILogger<ChangePasswordModel> logger, CurrencyConverter currencyConverter)
        {
            UserDatabase = new UserDatabase(piggyDbContext);
            UserDatabase.Deleted += OnDeleteUser;
            EmailSender = emailSenderSettings.Value;
             _logger = logger;
             CurrencyConverter = currencyConverter;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }

            await SetCurrency();
            return Page();
        }

        private async Task SetCurrency()
        {
            //Some alert could be displayed that failed to get currency list
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            UserModel = await UserDatabase.GetUserAsync(userId);
            CurrencyOptions = new List<string>();
            try
            {
                var currencyList = await CurrencyConverter.GetList();
                foreach (var currency in currencyList)
                {
                    CurrencyOptions.Add(currency.Code);
                }
            }
            catch (Exception)
            {
                CurrencyOptions.Add(UserModel.Currency);
            }
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
                return Page();
            }
        }

        //public async Task<IActionResult> OnPostCurrency()
        public async Task<IActionResult> OnPostCurrency()
        {
            //bool ar perskaiciuoti - Recalculate
            //int su valiutos pasirinkimu - Currency
            /*
             <option value="1">EUR</option>
             <option value="2">USD</option>
            pvz.: pasirinkus eur grazina 1, pasirinkus usd grazina 1 ir tt.
             */


            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            //UserModel = await UserDatabase.GetUserAsync(userId);
            await UserDatabase.ChangeCurrency(userId, Currency);
            return Redirect("/ChangePassword");
        }

        public async Task<IActionResult> OnPostDeleteAccount()
        {
            await HttpContext.SignOutAsync();
            await UserDatabase.DeleteUserAsync(User.FindFirst(ClaimTypes.Email).Value);
            return Redirect("/index");
        }

        private async void OnDeleteUser(object sender, UserModel user)
        {
            await EmailSender.SendFarewellEmailAsync(user.Email);
        }
    }
}
