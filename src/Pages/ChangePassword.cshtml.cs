using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        private CurrencyApiAgent CurrencyApiAgent { get; }
        public string UserCurrencyCode { get; set; }
        private IMemoryCache Cache { get; }
        public bool FailedToGetCurrencyList { get; set; }
        private CurrencyConverter CurrencyConverter { get; }

        public ChangePasswordModel(PiggyDbContext piggyDbContext, IOptions<EmailSender> emailSenderSettings, ILogger<ChangePasswordModel> logger, CurrencyApiAgent currencyApiAgent, IMemoryCache cache, CurrencyConverter currencyConverter)
        {
            UserDatabase = new UserDatabase(piggyDbContext);
            UserDatabase.Deleted += OnDeleteUser;
            EmailSender = emailSenderSettings.Value;
            _logger = logger;
            CurrencyApiAgent = currencyApiAgent;
            Cache = cache;
            CurrencyConverter = currencyConverter;
        }

        public async Task<IActionResult> OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToPage("/index");
            }

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
                switch (exception)
                {
                    case HttpListenerException ex:
                        _logger.LogInformation(ex.ToString());
                        ErrorMessage = "Failed to load currency information!";
                        break;
                    case HttpRequestException ex:
                        _logger.LogInformation(ex.ToString());
                        ErrorMessage = "Failed to load currency information!";
                        break;
                    default:
                        // Failed to even get the user currency from the database, the big one
                        _logger.LogInformation(exception.ToString());
                        ErrorMessage = "Failed to connect to database!";
                        break;
                }
            }

            UserCurrencyCode = currency.Code;
        }

        private async Task SetCurrencyList()
        {
            // The user model thing needs to get fixed
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            CurrencyOptions = new List<string>();
            var (currencyList1, exception) = await CurrencyConverter.GetCurrencyList(userId);

            if (exception != null)
            {
                FailedToGetCurrencyList = true;
                _logger.LogInformation(exception.ToString());
                if (exception is HttpRequestException || exception is HttpListenerException)
                {
                    // Something wrong with API6
                }
                else
                {
                    // Something wrong with database
                }
            }

            foreach (var currency in currencyList1)
            {
                CurrencyOptions.Add(currency.Code);
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

                ErrorMessage = "Passwords did not match!";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                return Page();
            }
        }

        // We need to set up error handling here
        public async Task<IActionResult> OnPostCurrency()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            if (Recalculate)
            {
                var currentCurrencyCode = (await UserDatabase.GetUserAsync(userId)).Currency;
                var rate = await CurrencyApiAgent.GetRate(currentCurrencyCode, Currency);
                await UserDatabase.ChangeCurrency(userId, Currency, rate);
            }
            else
            {
                await UserDatabase.ChangeCurrency(userId, Currency);
            }
            Cache.Remove(CacheKeys.UserCurrency);
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
