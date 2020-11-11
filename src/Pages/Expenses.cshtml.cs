using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class ExpensesModel : PageModel
    {
        public IEntryList Expenses { get; set; }

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


        public void OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            var dataManager = new DataManager(UserId);
            Expenses = dataManager.Expenses.EntryList;
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
            EntryDatabase.Create(entry, UserId, EntryType.Expense);
            return RedirectToPage("/Expenses");
        }

        public IActionResult OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            EntryDatabase.Delete(id, UserId, EntryType.Expense);
            return RedirectToPage("/Expenses");
        }
    }
}
