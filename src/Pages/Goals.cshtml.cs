using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
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

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        [StringLength(30)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public decimal Amount { get; set; }

        public void OnGet()
        {
            var dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
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
            var temp = new Goal(Title, Amount);
            GoalDbUpdater.Add(temp, 0);
            return RedirectToPage("/goals");
        }

        public IActionResult OnPostDelete(int id)
        {
            DeleteGoalFromDb(id);
            return RedirectToPage("/goals");
        }

        public IActionResult OnPostPurchased(int id, string title, string amount)
        {
            decimal.TryParse(amount, out var parsedAmount);
            var entry = new Entry(title, parsedAmount, DateTime.Today, recurring:false, importance:1);
            EntryDbUpdater.Add(entry, 0, EntryType.Expense);
            DeleteGoalFromDb(id);
            return RedirectToPage("/expenses");
        }

        private void DeleteGoalFromDb(int id)
        {
            GoalDbUpdater.Remove(id, 0);
        }

    }
}
