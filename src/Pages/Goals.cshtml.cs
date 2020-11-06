using System;
using System.Linq;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class GoalsModel : PageModel
    {
        public IGoalList Goals { get; set; }
        public decimal Savings { get; set; }
        private int UserId { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
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

        public void OnPostPurchased(int id, string title, decimal amount)
        {
            var entry = new Entry(title, amount, DateTime.Today, recurring:false, importance:1);
            EntryDbUpdater.Add(entry, UserId, EntryType.Expense);
            DeleteGoalFromDb(id);
        }

        private void DeleteGoalFromDb(int id)
        {
            GoalDbUpdater.Remove(id, UserId);
        }

    }
}
