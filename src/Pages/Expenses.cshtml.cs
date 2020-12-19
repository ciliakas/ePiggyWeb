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
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesModel : PageModel
    {
        private readonly ILogger<ExpensesModel> _logger;
        public bool WasException { get; set; }
        public IEntryList Expenses { get; set; }
        public IEnumerable<IEntry> ExpensesToDisplay { get; set; }

        [Required(ErrorMessage = "Title Required.")]
        [BindProperty]
        [StringLength(25, ErrorMessage = "Too long title!")]
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

        private int UserId { get; set; }

        public decimal AllExpenses { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";

        private EntryDatabase EntryDatabase { get; }
        private IConfiguration Configuration { get; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize = 10;
        public int TotalPages { get; set; }
        public bool ShowPrevious => CurrentPage > 1;
        public bool ShowNext => CurrentPage < TotalPages;
        private UserDatabase UserDatabase { get; }
        private CurrencyApiAgent CurrencyApiAgent { get; }
        public Currency Currency { get; set; }
        public decimal CurrencyRate { get; set; }
        public string CurrencySymbol { get; private set; }
        public bool CurrencyException { get; set; }
        private IMemoryCache Cache { get; }
        private CurrencyConverter CurrencyConverter { get; }
        public ExpensesModel(EntryDatabase entryDatabase, ILogger<ExpensesModel> logger, IConfiguration configuration, UserDatabase userDatabase, CurrencyApiAgent currencyApiAgent, IMemoryCache cache, CurrencyConverter currencyConverter)
        {
            EntryDatabase = entryDatabase;
            _logger = logger;
            Configuration = configuration;
            UserDatabase = userDatabase;
            CurrencyApiAgent = currencyApiAgent;
            Cache = cache;
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
                WasException = true;
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
                        _logger.LogInformation(exception.ToString());
                        ErrorMessage = "Failed to connect to database!";
                        break;
                }
            }

            Currency = currency;
            CurrencySymbol = Currency.SymbolString;
            CurrencyRate = Currency.Rate;
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate)
        {
            TimeManager.SetDate(startDate, endDate, ref ErrorMessage, Response, Request, out var tempStartDate, out var tempEndDate);
            StartDate = tempStartDate;
            EndDate = tempEndDate;
            await SetData();
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
            Amount *= 1 / CurrencyRate;
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var entry1 = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance, Currency.Code);
            try
            {
                await EntryDatabase.CreateAsync(entry1, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return Page();
            }

            return RedirectToPage("/expenses");
        }

        public async Task<IActionResult> OnPostDelete()
        {
            try
            {
                var selected = Request.Form["chkEntry"].ToString();
                var selectedList = selected.Split(',');
                var entryIdList = selectedList.Select(temp => Convert.ToInt32(temp)).ToList();

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);

                await EntryDatabase.DeleteListAsync(entryIdList, UserId, EntryType.Expense);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/expenses");
        }

        private async Task SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                //var (expenseList, totalPages) = await EntryDatabase.ReadByPage(x => x.Date >= StartDate && x.Date <= EndDate, UserId,
                //    EntryType.Expense, CurrentPage, PageSize);
                //ExpensesToDisplay = expenseList;
                //TotalPages = totalPages;

                Expenses = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId, EntryType.Expense);
                TotalPages = (int)Math.Ceiling(decimal.Divide(Expenses.Count, PageSize));

                var expensesToDisplay = Expenses.OrderByDescending(x => x.Date)
                    .ToIEntryList().GetPage(CurrentPage, PageSize);
                try
                {
                    ExpensesToDisplay = await CurrencyConverter.ConvertEntryList(expensesToDisplay, UserId);
                }
                catch (Exception ex)
                {
                    // Failed to convert currency
                    _logger.LogInformation(ex.ToString());
                }
                AllExpenses = Expenses.GetSum();
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Expenses = EntryList.RandomList(Configuration, EntryType.Expense);
                ExpensesToDisplay = Expenses;
                AllExpenses = Expenses.GetSum();
            }
        }
    }
}
