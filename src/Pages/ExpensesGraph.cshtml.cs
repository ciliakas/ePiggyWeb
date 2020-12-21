using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesGraphModel : PageModel
    {
        private readonly ILogger<ExpensesGraphModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Expenses { get; set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }
        public string ErrorMessage = "";
        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }
        public string CurrencySymbol { get; private set; }
        private CurrencyConverter CurrencyConverter { get; }
        public Currency Currency { get; set; }
        public bool CurrencyException { get; set; }
        public ExpensesGraphModel(EntryDatabase entryDatabase, ILogger<ExpensesGraphModel> logger,
            IConfiguration configuration, CurrencyConverter currencyConverter)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            Configuration = configuration;
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

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDateUpdated(startDate, endDate, Response);
            StartDate = startDate;
            EndDate = endDate;
            await SetCurrency();
            await SetData();
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
        private async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var entryList = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId,
                    EntryType.Expense);

                try
                {
                    Expenses = await CurrencyConverter.ConvertEntryList(entryList, UserId);
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    Expenses = entryList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = EntryList.RandomList(Configuration, EntryType.Expense);
            }
        }
    }
}
