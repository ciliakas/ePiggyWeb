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
        private GoalDatabase GoalDatabase { get; }
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
        public Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }

        /*Exception handling vars*/
        [BindProperty(SupportsGet = true)]
        public bool WasException { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool CurrencyException { get; set; }
        public bool LoadingException { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool WasExceptionParse { get; set; }

        /*Display*/
        public IGoalList Goals { get; private set; }
        public decimal Savings { get; private set; }
        private int UserId { get; set; }

        public GoalsModel(GoalDatabase goalDatabase, EntryDatabase entryDatabase, ILogger<GoalsModel> logger,
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
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var goalsList = await GoalDatabase.ReadListAsync(UserId);
                try
                {
                    Goals = await CurrencyConverter.ConvertGoalList(goalsList, UserId);
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    Goals = goalsList;
                }

                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);

                Savings = income.GetSum() - expenses.GetSum();
                if (Savings < 0)
                {
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
            finally
            {
                await SetCurrency();
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
            try
            {
                if (!ModelState.IsValid)
                {
                    await OnGet();
                    return Page();
                }
                await SetCurrency();

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var temp = Goal.CreateLocalGoal(Title, Amount, "EUR");
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
            try
            {
                if (string.IsNullOrEmpty(Title))
                {
                    await OnGet();
                    return Page();
                }
                await SetCurrency();

                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
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
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
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
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                decimal.TryParse(amount, out var parsedAmount);
                var entry = Entry.CreateLocalEntry(title, parsedAmount, DateTime.Today, recurring: false,
                    importance: 1, "EUR");
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
