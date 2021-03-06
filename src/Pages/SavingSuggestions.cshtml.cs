using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        public bool WasException { get; private set; }
        public IGoal Goal { get; private set; }
        public decimal Savings { get; private set; }
        private int UserId { get; set; }
        private IEntryList Expenses { get; set; }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        private DateTime StartDate { get; set; }
        public DateTime Today { get; private set; }
        public CalculationResults MinimalSuggestions { get; private set; }
        public CalculationResults RegularSuggestions { get; private set; }
        public CalculationResults MaximalSuggestions { get; private set; }
        private IGoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }
        public bool CurrencyException { get; private set; }
        private Currency Currency { get; set; }
        public string CurrencySymbol { get; private set; }
        private CurrencyConverter CurrencyConverter { get; }

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; }
        [BindProperty(SupportsGet = true)]
        public int Year { get; set; }
        private SavingCalculator MyCalc { get; set; }
        public decimal MonthlyIncome => MyCalc.MonthlyIncome;
        public SavingSuggestionsModel(ILogger<SavingSuggestionsModel> logger, IGoalDatabase goalDatabase,
            EntryDatabase entryDatabase, CurrencyConverter currencyConverter)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            CurrencyConverter = currencyConverter;
        }

        public async Task OnGet(int id)
        {
            Id = id;
            Today = DateTime.Today;
            StartDate = new DateTime(Today.Year, Today.Month, 1);
            await SetData();
            await SetCurrency();
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
            await SetData();
            await SetCurrency();
            return Page();
        }

        private async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                var goal = await GoalDatabase.ReadAsync(Id, UserId);
                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
               
                Goal = (await CurrencyConverter.ConvertGoalList(new GoalList {goal}, UserId)).First();
                income = await CurrencyConverter.ConvertEntryList(income, UserId);
                Expenses = await CurrencyConverter.ConvertEntryList(expenses, UserId);
                Savings = income.GetSum() - expenses.GetSum();
                Savings = Savings < 0 ? 0 : Savings;

                var endDate = StartDate.AddMonths(1).AddDays(-1);
                Expenses = Expenses.GetFrom(StartDate).GetTo(endDate);

                MyCalc = new SavingCalculator(Expenses, income.GetFrom(StartDate).GetTo(endDate), Goal, Savings);

                MinimalSuggestions = MyCalc.GetSuggestedExpensesOffers(SavingType.Minimal);
                RegularSuggestions = MyCalc.GetSuggestedExpensesOffers(SavingType.Regular);
                MaximalSuggestions = MyCalc.GetSuggestedExpensesOffers(SavingType.Maximal);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Goal = DataManagement.Goals.Goal.CreateLocalGoal(title:"Example Goal", amount:100, currency:"EUR");
                Savings = 0;

                if (ex is HttpListenerException || ex is HttpRequestException)
                {
                    CurrencyException = true;
                }
            }
        }
    }
}
