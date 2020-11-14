using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.DataManagement.Saving;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class GoalsModel : PageModel
    {
        public IGoalList Goals { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }
        public IEntryList Expenses { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [StringLength(30)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public decimal Amount { get; set; }

        public IList<ISavingSuggestion> EntrySuggestions{ get; set; }
        public List<SavingSuggestionByMonth> MonthlySuggestions { get; set; }

        public void OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList;
            Goals = dataManager.Goals.GoalList;
            EntrySuggestions = new List<ISavingSuggestion>();
            MonthlySuggestions = new List<SavingSuggestionByMonth>();
            AlternativeSavingCalculator.GetSuggestedExpensesOffers(Expenses, Goals[0], EntrySuggestions,
                MonthlySuggestions, Savings);
            Savings = dataManager.Income.EntryList.GetSum() - dataManager.Expenses.EntryList.GetSum();
            if (Savings < 0)
            {
                Savings = 0;
            }
        }


        public IActionResult OnPostNewGoal()
        { 
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var temp = Goal.CreateLocalGoal(Title, Amount);
            GoalDatabase.Create(temp, UserId);
            return RedirectToPage("/goals");
        }

        public IActionResult OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            DeleteGoalFromDb(id);
            return RedirectToPage("/goals");
        }

        public IActionResult OnPostPurchased(int id, string title, string amount)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            decimal.TryParse(amount, out var parsedAmount);
            var entry = Entry.CreateLocalEntry(title, parsedAmount, DateTime.Today, recurring:false, importance:1);
            GoalDatabase.MoveGoalToExpenses(id, UserId, entry);
            return RedirectToPage("/expenses");
        }

        public IActionResult OnPostSuggestions()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList;
            Goals = dataManager.Goals.GoalList;
            Savings = dataManager.Income.EntryList.GetSum() - dataManager.Expenses.EntryList.GetSum();
            AlternativeSavingCalculator.GetSuggestedExpensesOffers(Expenses, Goals[0], EntrySuggestions,
                MonthlySuggestions, Savings);
            return Page();
        }

        private void DeleteGoalFromDb(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            GoalDatabase.Delete(id, UserId);
        }
    }
}
