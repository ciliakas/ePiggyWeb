using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class IncomesModel : PageModel
    {
        public IEntryList Income { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public string Title { get; set; }
        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public decimal Amount { get; set; }
        [BindProperty]
        public DateTime Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Required")]
        public int Importance { get; set; }
        [BindProperty]
        public bool Recurring { get; set; }

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
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Income = dataManager.Income.EntryList.GetFrom(StartDate).GetTo(EndDate);
        }


        public IActionResult OnGetFilter(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Income = dataManager.Income.EntryList.GetFrom(startDate).GetTo(endDate);
            return Page();
        }


        public IActionResult OnPostNewEntry()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var entry = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance);
            EntryDbUpdater.Add(entry, UserId, EntryType.Income);
            return RedirectToPage("/income");
        }

        public IActionResult OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            EntryDbUpdater.Remove(id, UserId, EntryType.Income);
            return RedirectToPage("/income");
        }
    }
}
