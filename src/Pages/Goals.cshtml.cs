using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class GoalsModel : PageModel
    {
        /*DI objects*/
        private readonly ILogger<GoalsModel> _logger;
        private CurrencyConverter CurrencyConverter { get; }
        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        private HttpClient HttpClient { get; }
        private IConfiguration Configuration { get; }
        private readonly Lazy<InternetParser> _internetParser;

        /*New Entry vars*/
        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [StringLength(25)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [Range(0, 99999999.99)]
        public decimal Amount { get; set; }

        /*Currency vars*/
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }

        /*Exception handling vars*/
        [BindProperty(SupportsGet = true)]
        public bool WasException { get; private set; }
        [BindProperty(SupportsGet = true)]
        public bool CurrencyException { get; private set; }
        public bool LoadingException { get; private set; }
        [BindProperty(SupportsGet = true)]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public bool WasExceptionParse { get; }

        /*Display*/
        public IGoalList Goals { get; private set; }
        public decimal Savings { get; private set; }
        private int UserId { get; set; }

        public GoalsModel(IGoalDatabase goalDatabase, EntryDatabase entryDatabase, ILogger<GoalsModel> logger,
            HttpClient httpClient, IConfiguration configuration, CurrencyConverter currencyConverter)
        {
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            _logger = logger;
            HttpClient = httpClient;
            _internetParser = new Lazy<InternetParser>(() => new InternetParser(HttpClient));
            Configuration = configuration;
            CurrencyConverter = currencyConverter;
        }

        public async Task OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                var goalsList = await GoalDatabase.ReadListAsync(UserId);
                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                try
                {
                    await SetCurrency();
                    Goals = await CurrencyConverter.ConvertGoalList(goalsList, UserId);
                    income = await CurrencyConverter.ConvertEntryList(income, UserId);
                    expenses = await CurrencyConverter.ConvertEntryList(expenses, UserId);
                    Savings = income.GetSum() - expenses.GetSum();
                    Savings = Savings < 0 ? 0 : Savings;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex.ToString());
                    CurrencyException = true;
                    Goals = goalsList;
                    Savings = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                LoadingException = true;
                Goals = GoalList.RandomList(Configuration);
                Savings = 0;
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

        public async Task<IActionResult> OnPostNewGoal()
        {
            if (!ModelState.IsValid)
            {
                await OnGet();
                return Page();
            }

            await SetCurrency();
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var temp = Goal.CreateLocalGoal(Title, Amount, Currency.Code);
            try
            {
                await GoalDatabase.CreateAsync(temp, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/goals", new { WasException, CurrencyException });
        }

        public async Task<IActionResult> OnPostParseGoal()
        {
            if (string.IsNullOrEmpty(Title))
            {
                await OnGet();
                return Page();
            }

            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                var temp = await _internetParser.Value.ReadPriceFromCamel(Title);
                await GoalDatabase.CreateAsync(temp, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return RedirectToPage("/goals", new { wasExceptionParse = true });

            }

            return RedirectToPage("/goals", new { WasException, CurrencyException });
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            try
            {
                await GoalDatabase.DeleteAsync(id, UserId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
            }

            return RedirectToPage("/goals", new { WasException, CurrencyException });
        }

        public async Task<IActionResult> OnPostPurchased(int id, string title, string amount)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            await SetCurrency();
            try
            {
                decimal.TryParse(amount, out var parsedAmount);
                var entry = Entry.CreateLocalEntry(title, parsedAmount, DateTime.Today, recurring: false,
                    importance: 1, Currency.Code);
                await GoalDatabase.MoveGoalToExpensesAsync(id, UserId, entry);
                return RedirectToPage("/expenses");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                return RedirectToPage("/goals");
            }
        }
    }
}
