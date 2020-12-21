using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesModel : PageModel
    {
        /*DI objects*/
        private readonly ILogger<ExpensesModel> _logger;
        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }
        private CurrencyConverter CurrencyConverter { get; }

        /*New Entry vars*/
        [Required(ErrorMessage = "Title Required.")]
        [BindProperty]
        [StringLength(25)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Amount Required.")]
        [BindProperty]
        [Range(0, 99999999.99, ErrorMessage = "Amount out of range!")]
        public decimal Amount { get; set; }
        [BindProperty]
        public DateTime Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Importance Required.")]
        public int Importance { get; set; }
        [BindProperty]
        public bool Recurring { get; set; }

        /*Data filter vars*/
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        /*Pagination vars*/
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; } = 1;

        private static int PageSize => 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Expenses.Count, PageSize));
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;

        /*Currency vars*/
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }

        /*Exception handling vars*/
        [BindProperty(SupportsGet = true)]
        public bool WasException { get; private set; }
        [BindProperty(SupportsGet = true)]
        public bool CurrencyException { get; private set; }
        public bool LoadingException { get; private set; }

        /*Display*/
        public IEntryList Expenses { get; private set; }
        public IEntryList ExpensesToDisplay => Expenses.GetPage(CurrentPage, PageSize);
        public decimal TotalExpenses => Expenses.GetSum();
        private int UserId { get; set; }

        public ExpensesModel(EntryDatabase entryDatabase, ILogger<ExpensesModel> logger, IConfiguration configuration,
            CurrencyConverter currencyConverter)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            Configuration = configuration;
            CurrencyConverter = currencyConverter;
        }

        public async Task OnGet()
        {
            TimeManager.GetDate(Request, out var startDate, out var endDate);
            await LoadData(startDate, endDate);
        }

        private async Task LoadData(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            await SetCurrency();
            await SetData();
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDate(startDate, endDate, Response);
            await LoadData(startDate, endDate);
            return Page();
        }

        public async Task<IActionResult> OnPostNewEntry()
        {
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }

            await SetCurrency();
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var entry = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance, Currency.Code);
            try
            {
                await EntryDatabase.CreateAsync(entry, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/expenses", new { WasException, CurrencyException });
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var selected = Request.Form["chkEntry"].ToString();
            if (string.IsNullOrEmpty(selected))
            {
                return RedirectToPage("/expenses");
            }
            var selectedList = selected.Split(',');
            var entryIdList = selectedList.Select(temp => Convert.ToInt32(temp)).ToList();
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                await EntryDatabase.DeleteListAsync(entryIdList, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/expenses", new { WasException, CurrencyException });
        }

        private async Task SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                var expenseList = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId, EntryType.Expense, orderByDate: true);

                try
                {
                    Expenses = await CurrencyConverter.ConvertEntryList(expenseList, UserId);
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    Expenses = expenseList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                LoadingException = true;
                Expenses = EntryList.RandomList(Configuration, EntryType.Expense);
            }
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
