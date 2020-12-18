using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        private CurrencyConverter CurrencyConverter { get; }
        public UserModel UserModel { get; private set; }
        private IMemoryCache Cache { get; }
        public bool FailedToGetCurrencyList { get; set; }

        public ChangePasswordModel(PiggyDbContext piggyDbContext, IOptions<EmailSender> emailSenderSettings, ILogger<ChangePasswordModel> logger, CurrencyConverter currencyConverter, IMemoryCache cache)
        {
            UserDatabase = new UserDatabase(piggyDbContext);
            UserDatabase.Deleted += OnDeleteUser;
            EmailSender = emailSenderSettings.Value;
            _logger = logger;
            CurrencyConverter = currencyConverter;
            Cache = cache;
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
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            UserModel = await UserDatabase.GetUserAsync(userId);
            CurrencyOptions = new List<string>();
            if (Cache.TryGetValue(CacheKeys.CurrencyList, out List<string> cachedCurrencyList))
            {
                CurrencyOptions.AddRange(cachedCurrencyList);
                //Check if user's currency is cached
                if (Cache.TryGetValue(CacheKeys.UserCurrency, out Currency cachedCurrency))
                {
                    if (UserModel.Currency.Equals(cachedCurrency.Code)) return;
                }

                //If we the code gets here, it means that we don't have the the correct user currency cached
                try
                {
                    var userCurrency = await CurrencyConverter.GetCurrency(UserModel.Currency);
                    var options = CacheKeys.DefaultCurrencyCacheOptions();
                    Cache.Set(CacheKeys.UserCurrency, userCurrency, options);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.ToString());
                }
                return;
            }

            try
            {
                var currencyList = await CurrencyConverter.GetList();
                var currencyCodeList = currencyList.Select(currency => currency.Code).ToList();

                var options = CacheKeys.DefaultCurrencyCacheOptions();

                Cache.Set(CacheKeys.CurrencyList, currencyCodeList, options);
                CurrencyOptions.AddRange(currencyCodeList);

                //Also set user currency 
                var userCurrency = currencyList.First(x => x.Code == UserModel.Currency);
                Cache.Set(CacheKeys.UserCurrency, userCurrency, options);
            }
            catch (Exception e)
            {
                CurrencyOptions.Add(UserModel.Currency);
                FailedToGetCurrencyList = true;
                _logger.LogInformation(e.ToString());
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
                var rate = await CurrencyConverter.GetRate(currentCurrencyCode, Currency);
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
            await UserDatabase.DeleteUserAsync(User.FindFirst(ClaimTypes.Email).Value);
            return Redirect("/index");
        }

        private async void OnDeleteUser(object sender, UserModel user)
        {
            await EmailSender.SendFarewellEmailAsync(user.Email);
        }
    }
}
