using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.DataManagement.Saving;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ePiggyWeb.Pages
{
    public class SavingSuggestionsModel : PageModel
    {
        private readonly ILogger<SavingSuggestionsModel> _logger;
        public bool WasException { get; set; }
        public IGoal Goal { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }
        public IEntryList Expenses { get; set; }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public DateTime StartDate { get; set; }
        public DateTime Today { get; set; }
        public CalculationResults MinimalSuggestions { get; set; }
        public CalculationResults RegularSuggestions { get; set; }
        public CalculationResults MaximalSuggestions { get; set; }
        public IConfiguration Configuration { get; }

        private readonly ThreadingCalculator _threadingCalculator = new ThreadingCalculator();

        public string ErrorMessage = "";

        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        public bool CurrencyException { get; set; }
        public Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }
        private CurrencyConverter CurrencyConverter { get; }

        [BindProperty(SupportsGet = true)] 
        public int Month { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }


        public SavingSuggestionsModel(ILogger<SavingSuggestionsModel> logger, IGoalDatabase goalDatabase,
            EntryDatabase entryDatabase, IConfiguration configuration, CurrencyConverter currencyConverter)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            Configuration = configuration;
            CurrencyConverter = currencyConverter;
        }
        public async Task OnGet(int id)
        {
            Id = id;
            Today = DateTime.Today;
            StartDate = new DateTime(Today.Year, Today.Month, 1);
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

        public async Task<IActionResult> OnGetFilter(int id)
        {
            StartDate = new DateTime(Year, Month, 1);

            Id = id;
            await SetCurrency();
            await SetData();
            return Page();
        }

        public async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                Goal = await GoalDatabase.ReadAsync(Id, UserId);
                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                try
                {
                    income = await CurrencyConverter.ConvertEntryList(income, UserId);
                    Expenses = await CurrencyConverter.ConvertEntryList(expenses, UserId);
                    Savings = income.GetSum() - expenses.GetSum();
                    Savings = Savings < 0 ? 0 : Savings;
                }
                catch (Exception ex)
                {
                    CurrencyException = true;
                    _logger.LogInformation(ex.ToString());
                    throw;
                }
                Savings = income.GetSum() - Expenses.GetSum();
                if (Savings < 0)
                {
                    Savings = 0;
                }

                var endDate = StartDate.AddMonths(1).AddDays(-1);
                Expenses = Expenses.GetFrom(StartDate).GetTo(endDate);

                var suggestionDictionary = _threadingCalculator.GetAllSuggestedExpenses(Expenses, Goal, Savings, Configuration);
                MinimalSuggestions = suggestionDictionary[SavingType.Minimal];
                RegularSuggestions = suggestionDictionary[SavingType.Regular];
                MaximalSuggestions = suggestionDictionary[SavingType.Maximal];
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Goal = DataManagement.Goals.Goal.CreateLocalGoal("Example Goal", 100, "EUR");
                Savings = 0;
            }

        }
    }
}
