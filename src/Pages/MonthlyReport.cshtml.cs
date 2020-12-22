using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.MonthlyReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class MonthlyReportModel : PageModel
    {
        private readonly ILogger<SavingSuggestionsModel> _logger;
        public bool WasException { get; private set; }
        private int UserId { get; set; }

        [BindProperty]
        public MonthlyReportResult Data { get; private set; }

        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }
        public bool CurrencyException { get; private set; }
        private CurrencyConverter CurrencyConverter { get; }

        public MonthlyReportModel(ILogger<SavingSuggestionsModel> logger, IGoalDatabase goalDatabase,
            EntryDatabase entryDatabase, CurrencyConverter currencyConverter)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            CurrencyConverter = currencyConverter;
        }
        public async Task<IActionResult> OnGet()
        {
            try
            {
                await SetCurrency();
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var monthlyReportCalculator = new MonthlyReportCalculator(GoalDatabase, EntryDatabase, UserId, CurrencyConverter);
                Data = await monthlyReportCalculator.Calculate();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }
           
            return Page();
        }

        private async Task SetCurrency()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var (currency, exception) = await CurrencyConverter.GetUserCurrency(UserId);
            if (exception != null)
            {
                CurrencyException = true;
            }
            Currency = currency;
            CurrencySymbol = Currency.SymbolString;
        }
    }
}
