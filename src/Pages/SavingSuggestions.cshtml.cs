using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
        [BindProperty]
        public DateTime EndDate { get; set; }
        public CalculationResults MinimalSuggestions { get; set; }
        public CalculationResults RegularSuggestions { get; set; }
        public CalculationResults MaximalSuggestions { get; set; }
        public IConfiguration Configuration { get; }

        private readonly ThreadingCalculator _threadingCalculator = new ThreadingCalculator();

        public string ErrorMessage = "";

        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }

        public SavingSuggestionsModel(ILogger<SavingSuggestionsModel> logger, GoalDatabase goalDatabase, EntryDatabase entryDatabase, IConfiguration configuration)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
            Configuration = configuration;
        }
        public async Task OnGet(int id)
        {
            Id = id;
            var today = DateTime.Now;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = DateTime.Today;
            await SetData();
        }

        public async Task<IActionResult> OnGetFilter(DateTime startDate, DateTime endDate, int id)
        {
            if (startDate > endDate)
            {
                ErrorMessage = "Start date is bigger than end date!";
                var today = DateTime.Now;
                StartDate = new DateTime(today.Year, today.Month, 1);
                EndDate = DateTime.Today;
            }
            else
            {
                StartDate = startDate;
                EndDate = endDate;
            }

            Id = id;
            await SetData();
            return Page();
        }

        public async Task SetData()
        {
            try
            {
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                Expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Goal = await GoalDatabase.ReadAsync(Id, UserId);
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                Savings = income.GetSum() - Expenses.GetSum();
                if (Savings < 0)
                {
                    Savings = 0;
                }

                Expenses = Expenses.GetFrom(StartDate).GetTo(EndDate);

                var suggestionDictionary = _threadingCalculator.GetAllSuggestedExpenses(Expenses, Goal, Savings, Configuration);
                MinimalSuggestions = suggestionDictionary[SavingType.Minimal];
                RegularSuggestions = suggestionDictionary[SavingType.Regular];
                MaximalSuggestions = suggestionDictionary[SavingType.Maximal];
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
                WasException = true;
                Goal = DataManagement.Goals.Goal.CreateLocalGoal("Example Goal", 100);
                Savings = 0;
            }

        }
    }
}
