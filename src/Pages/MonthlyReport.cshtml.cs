using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.MonthlyReport;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class MonthlyReportModel : PageModel
    {
        private readonly ILogger<SavingSuggestionsModel> _logger;
        public bool WasException { get; set; }
        private int UserId { get; set; }

        public string ErrorMessage = "";

        [BindProperty]
        public MonthlyReportResult Data { get; set; }

        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        private UserDatabase UserDatabase { get; }
        private CurrencyConverter CurrencyConverter { get; }
        public string CurrencySymbol { get; private set; }
        public decimal CurrencyRate { get; set; }
        public bool CurrencyException { get; set; }
        private IMemoryCache Cache { get; }

        public MonthlyReportModel(ILogger<SavingSuggestionsModel> logger, IGoalDatabase goalDatabase, EntryDatabase entryDatabase, UserDatabase userDatabase, CurrencyConverter currencyConverter, IMemoryCache cache)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            UserDatabase = userDatabase;
            CurrencyConverter = currencyConverter;
            Cache = cache;
        }
        public async Task<IActionResult> OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var monthlyReportCalculator = new MonthlyReportCalculator(GoalDatabase, EntryDatabase, UserId);
            Data = await monthlyReportCalculator.Calculate();
            await SetCurrency();
            return Page();
        }

        private async Task SetCurrency()
        {
            if (!Cache.TryGetValue(CacheKeys.UserCurrency, out Currency userCurrency))
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var userModel = await UserDatabase.GetUserAsync(UserId);
                try
                {
                    userCurrency = await CurrencyConverter.GetCurrency(userModel.Currency);
                }
                catch (Exception)
                {
                    CurrencySymbol = userModel.Currency;
                    CurrencyRate = 1;
                    CurrencyException = true;
                    return;
                }
            }

            CurrencySymbol = userCurrency.GetSymbol();
            CurrencyRate = userCurrency.Rate;
            var options = CacheKeys.DefaultCurrencyCacheOptions();
            Cache.Set(CacheKeys.UserCurrency, userCurrency, options);
        }

    }
}
