using System;
using System.ComponentModel.DataAnnotations;
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

        public void OnGet()
        {
            LoadData();
        }

        private void LoadData()
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
        }

        public void OnPostNewEntry()
        {
            DataManager dataManager;
            if (!ModelState.IsValid)
            {
                dataManager = new DataManager();
                Income = dataManager.Income.EntryList;
                return;
            }

            var entry = Entry.CreateLocalEntry(Title, Amount, Date, Recurring, Importance);
            EntryDbUpdater.Add(entry, 0, EntryType.Income);
            dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
        }

        public void OnPostDelete(int id)
        {
            EntryDbUpdater.Remove(id, EntryType.Income);
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
            Response.Redirect("/Income");
        }
    }
}
