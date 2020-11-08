using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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

        public void OnGet()
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            Debug.WriteLine("\n\n\n" + UserId);
            var dataManager = new DataManager(UserId);
            Income = dataManager.Income.EntryList;
        }


        public IActionResult OnPostNewEntry()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            Debug.WriteLine("\n\n\n" + UserId);
            var entry = new Entry(Title, Amount, Date, Recurring, Importance);
            EntryDbUpdater.Add(entry, UserId, EntryType.Income);
            return RedirectToPage("/Income");
        }

        public IActionResult OnPostDelete(int id)
        {
            UserId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            Debug.WriteLine("\n\n\n" + UserId);
            EntryDbUpdater.Remove(id, UserId, EntryType.Expense);
            return RedirectToPage("/Income");
        }
    }
}
