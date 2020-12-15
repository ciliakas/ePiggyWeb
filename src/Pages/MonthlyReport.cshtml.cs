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
        public bool WasException { get; set; }
        private int UserId { get; set; }

        public string ErrorMessage = "";

        [BindProperty]
        public MonthlyReportResult Data { get; set; }

        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        private UserDatabase UserDatabase { get; }
        private CurrencyConverter CurrencyConverter { get; }
        public string CurrencySymbol { get; private set; }

        public MonthlyReportModel(ILogger<SavingSuggestionsModel> logger, GoalDatabase goalDatabase, EntryDatabase entryDatabase, UserDatabase userDatabase, CurrencyConverter currencyConverter)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            UserDatabase = userDatabase;
            CurrencyConverter = currencyConverter;
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
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var userModel = await UserDatabase.GetUserAsync(UserId);
            CurrencySymbol = await CurrencyConverter.GetCurrencySymbol(userModel.Currency);
        }

    }
}
