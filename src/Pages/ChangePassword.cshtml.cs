using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ePiggyWeb.Pages
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ILogger<ChangePasswordModel> _logger;

        [Required(ErrorMessage = "All fields required!")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, at least one number, " +
                           "special character and be longer than six characters.")]
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
        public List<string> CurrencyOptions { get; private set; }
        private UserDatabase UserDatabase { get; }
        private EmailSender EmailSender { get; }
        public string UserCurrencyCode { get; private set; }
        private IMemoryCache Cache { get; }
        public bool FailedToGetCurrencyList { get; set; }
        private CurrencyConverter CurrencyConverter { get; }

        public ChangePasswordModel(PiggyDbContext piggyDbContext, IOptions<EmailSender> emailSenderSettings, 
            ILogger<ChangePasswordModel> logger, IMemoryCache cache, CurrencyConverter currencyConverter)
        {
            UserDatabase = new UserDatabase(piggyDbContext);
            UserDatabase.Deleted += OnDeleteUser;
            EmailSender = emailSenderSettings.Value;
            _logger = logger;
            Cache = cache;
            CurrencyConverter = currencyConverter;
        }

        public async Task<IActionResult> OnGet()
        {
            await SetCurrencyList();
            await SetUserCurrency();
            return Page();
        }

        private async Task SetUserCurrency()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var (currency, exception) = await CurrencyConverter.GetUserCurrency(userId);
            if (exception != null)
            {
                _logger.LogInformation(exception.ToString());
                ErrorMessage = exception switch
                {
                    HttpListenerException _ => "Failed to load currency information!",
                    HttpRequestException _ => "Failed to load currency information!",
                    _ => "Failed to connect to database!"
                };
            }

            UserCurrencyCode = currency.Code;
        }

        private async Task SetCurrencyList()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            CurrencyOptions = new List<string>();
            var (currencyList1, exception) = await CurrencyConverter.GetCurrencyList(userId);

            if (exception != null)
            {
                FailedToGetCurrencyList = true;
                _logger.LogInformation(exception.ToString());
            }

            foreach (var currency in currencyList1)
            {
                CurrencyOptions.Add(currency.Code);
            }
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                await SetCurrencyList();
                await SetUserCurrency();
                return Page();
            }

            if (string.Equals(Password, PasswordConfirm))
            {
                try
                {
                    await UserDatabase.ChangePasswordAsync(User.FindFirst(ClaimTypes.Email).Value, Password);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.ToString());
                }
                return RedirectToPage("/index");
            }

            ErrorMessage = "Passwords did not match!";
            await SetCurrencyList();
            await SetUserCurrency();
            return Page();
        }

        public async Task<IActionResult> OnPostCurrency()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                await UserDatabase.ChangeCurrency(userId, Currency);
                Cache.Remove(CacheKeys.UserCurrency);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
            }
            return Redirect("/ChangePassword");
        }

        public async Task<IActionResult> OnPostDeleteAccount()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("StartDate");
            Response.Cookies.Delete("EndDate");
            Cache.Remove(CacheKeys.UserCurrency);
            await UserDatabase.DeleteUserAsync(User.FindFirst(ClaimTypes.Email).Value);
            return Redirect("/index");
        }

        private async void OnDeleteUser(object sender, UserModel user)
        {
            await EmailSender.SendFarewellEmailAsync(user.Email);
        }
    }
}
