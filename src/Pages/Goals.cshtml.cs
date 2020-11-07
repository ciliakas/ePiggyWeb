using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public string Title { get; set; }
        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public decimal Amount { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
        }


        public void OnPostNewGoal()
        {
            DataManager dataManager;
            if (!ModelState.IsValid)
            {
                dataManager = new DataManager();
                Goals = dataManager.Goals.GoalList;
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
                return;
            }

            var temp = new Goal(Title, Amount);
            GoalDbUpdater.Add(temp, 0);

            dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
        }
        public void OnPostDelete(int id)
        {
            DeleteGoalFromDb(id);
            var dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
            Response.Redirect("/Goals");
        }

        public void OnPostPurchased(int id, string title, string amount)
        {
            decimal.TryParse(amount, out var parsedAmount);
            var entry = new Entry(title, parsedAmount, DateTime.Today, recurring:false, importance:1);
            EntryDbUpdater.Add(entry, 0, EntryType.Expense);
            DeleteGoalFromDb(id);
            var dataManager = new DataManager();
            Goals = dataManager.Goals.GoalList;
            Response.Redirect("/Expenses");
        }

        private void DeleteGoalFromDb(int id)
        {
            GoalDbUpdater.Remove(id, 0);
        }

    }
}
