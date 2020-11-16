using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.DataManagement.Saving;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class SavingSuggestionsModel : PageModel
    {
        public IGoal Goal { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }
        public IEntryList Expenses { get; set; }
        public int MonthsToSave { get; set; }

        public IList<ISavingSuggestion> EntrySuggestions { get; set; }
        public List<SavingSuggestionByMonth> MonthlySuggestions { get; set; }
        public void OnGet(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList;
            Goal = dataManager.Goals.GoalList.FirstOrDefault(x => x.Id == id);
            EntrySuggestions = new List<ISavingSuggestion>();
            MonthlySuggestions = new List<SavingSuggestionByMonth>();
            Savings = dataManager.Income.EntryList.GetSum() - dataManager.Expenses.EntryList.GetSum();
            if (Savings < 0)
            {
                Savings = 0;
            }
            MonthsToSave = AlternativeSavingCalculator.GetSuggestedExpensesOffers(Expenses,
                Goal,
                EntrySuggestions,
                MonthlySuggestions,
                Savings);
        }
    }
}
