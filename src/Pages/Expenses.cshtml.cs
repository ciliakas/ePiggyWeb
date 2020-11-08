using System;
using System.ComponentModel.DataAnnotations;
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


        public void OnGet()
        {
            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
        }

        public void OnPostNewEntry()
        {
            DataManager dataManager;
            if (!ModelState.IsValid)
            {
                dataManager = new DataManager();
                Expenses = dataManager.Expenses.EntryList;
                return;
            }

            var entry = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance);
            EntryDbUpdater.Add(entry, 0, EntryType.Expense);
            dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
        }

        public void OnPostDelete(int id)
        {
            EntryDbUpdater.Remove(id, EntryType.Expense);
            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
            Response.Redirect("/Expenses");
        }
    }
}
