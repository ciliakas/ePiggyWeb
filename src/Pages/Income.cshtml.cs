using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
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
    public class IncomesModel : PageModel
    {
        /*DI objects*/
        private readonly ILogger<IncomeModel> _logger;
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
        public int CurrentPage { get; set; } = 1;

        private static int PageSize => 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(Income.Count, PageSize));
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
        public IEntryList Income { get; private set; }
        public IEntryList IncomeToDisplay => Income.GetPage(CurrentPage, PageSize);
        public decimal TotalIncome => Income.GetSum();
        private int UserId { get; set; }
        public DateTime Today { get; private set; }


        public IncomesModel(EntryDatabase entryDatabase, ILogger<IncomeModel> logger, IConfiguration configuration,
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
                await EntryDatabase.CreateAsync(entry, UserId, EntryType.Income);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/income", new { WasException, CurrencyException });
        }

        public async Task<IActionResult> OnPostDelete()
        {
            var selected = Request.Form["chkEntry"].ToString();
            if (string.IsNullOrEmpty(selected))
            {
                return RedirectToPage("/income");
            }

            var selectedList = selected.Split(',');
            var entryIdList = selectedList.Select(temp => Convert.ToInt32(temp)).ToList();
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                await EntryDatabase.DeleteListAsync(entryIdList, UserId, EntryType.Income);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/income", new { WasException, CurrencyException });
        }

        private async Task SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                Today = DateTime.Today;
                var incomeList = await EntryDatabase.ReadListAsync(x => x.Date >= StartDate && x.Date <= EndDate,
                    UserId, EntryType.Income, orderByDate: true);
                try
                {
                    Income = await CurrencyConverter.ConvertEntryList(incomeList, UserId);
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    Income = incomeList;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                LoadingException = true;
                Income = EntryList.RandomList(Configuration, EntryType.Income);
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
