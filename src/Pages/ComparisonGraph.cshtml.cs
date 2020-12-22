using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ComparisonGraphModel : PageModel
    {
        private readonly ILogger<ComparisonGraphModel> _logger;
        public bool WasException { get; private set; }
        public decimal Income { get; private set; }
        public decimal Expenses { get; private set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        private CurrencyConverter CurrencyConverter { get; }
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }
        public bool CurrencyException { get; private set; }
        public DateTime Today { get; private set; }

        public ComparisonGraphModel(EntryDatabase entryDatabase, ILogger<ComparisonGraphModel> logger,
            CurrencyConverter currencyConverter)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            CurrencyConverter = currencyConverter;
        }

        public async Task OnGet()
        {
            TimeManager.GetDate(Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetCurrency();
            await SetData();
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

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDate(startDate, endDate, Response);
            StartDate = startDate;
            EndDate = endDate;
            await SetCurrency();
            await SetData();
            return Page();
        }

        private async Task SetData()
        {
            try
            {
                Today = DateTime.Today;
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var expenses = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId, EntryType.Expense);

                var income = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId, EntryType.Income);
                try
                {
                    Income = (await CurrencyConverter.ConvertEntryList(income, UserId)).GetSum();
                    Expenses = (await CurrencyConverter.ConvertEntryList(expenses, UserId)).GetSum();
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    throw;
                }
            }
            catch (Exception ex)
            {
                var rand = new Random();
                const int min = 10000;
                const int max = 500001;
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = (decimal)rand.Next(min, max) / 100;
                Income = (decimal)rand.Next(min, max) / 100;
            }
        }
    }
}
