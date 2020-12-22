using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
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
        private MonthlyReportCalculator MonthlyReportCalculator { get; }
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }
        public bool CurrencyException { get; private set; }
        private CurrencyConverter CurrencyConverter { get; }

        public MonthlyReportModel(ILogger<SavingSuggestionsModel> logger, CurrencyConverter currencyConverter, 
            MonthlyReportCalculator monthlyReportCalculator)
        {
            _logger = logger;
            CurrencyConverter = currencyConverter;
            MonthlyReportCalculator = monthlyReportCalculator;
        }
        public async Task<IActionResult> OnGet()
        {
            try
            {
                await SetCurrency();
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                Data = await MonthlyReportCalculator.Calculate(UserId);
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
