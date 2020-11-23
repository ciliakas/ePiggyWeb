using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.DataManagement.Saving;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public int MonthsToSave { get; set; }
        [BindProperty]
        public int Id { get; set; }

        public IList<ISavingSuggestion> EntrySuggestions { get; set; }
        public List<SavingSuggestionByMonth> MonthlySuggestions { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }

        public string ErrorMessage = "";

        private GoalDatabase GoalDatabase { get; }
        private EntryDatabase EntryDatabase { get; }

        public SavingSuggestionsModel(ILogger<SavingSuggestionsModel> logger, GoalDatabase goalDatabase, EntryDatabase entryDatabase)
        {
            _logger = logger;
            GoalDatabase = goalDatabase;
            EntryDatabase = entryDatabase;
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
                var expenses = await EntryDatabase.ReadListAsync(UserId, EntryType.Expense);
                Goal = await GoalDatabase.ReadAsync(Id, UserId);
                EntrySuggestions = new List<ISavingSuggestion>();
                MonthlySuggestions = new List<SavingSuggestionByMonth>();
                var income = await EntryDatabase.ReadListAsync(UserId, EntryType.Income);
                Savings = income.GetSum() - expenses.GetSum();
                if (Savings < 0)
                {
                    Savings = 0;
                }

                MonthsToSave = AlternativeSavingCalculator.GetSuggestedExpensesOffers(expenses,
                    Goal,
                    EntrySuggestions,
                    MonthlySuggestions,
                    Savings);
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
