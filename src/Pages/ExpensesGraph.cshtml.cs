using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesGraphModel : PageModel
    {
        public IEntryList Expenses { get; set; }
        private int UserId { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }
        public void OnGet()
        {
            var today = DateTime.Now;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = DateTime.Today;
            SetData();
        }

        public IActionResult OnGetFilter(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            SetData();
            return Page();
        }

        private void SetData()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList.GetFrom(StartDate).GetTo(EndDate);
        }
    }
}
