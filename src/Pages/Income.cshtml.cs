using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class IncomesModel : PageModel
    {
        public IEntryList Income { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [BindProperty]
        public string Title { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        [BindProperty]
        public string Amount { get; set; }
        [BindProperty]
        public string Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Importance is required")]
        public string Importance { get; set; }
        [BindProperty]
        public string IsMonthly { get; set; }

        public string Error { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
        }

        public void OnPostNewEntry()
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
            if (!ModelState.IsValid) return;

            if (!decimal.TryParse(Amount, out var parsedAmount))
            {
                Error = "Amount is not a number!";
                return;
            }

            var parsedDate = Convert.ToDateTime(Date);
            var parsedIsMonthly = Convert.ToBoolean(IsMonthly);
            var parsedImportance = int.Parse(Importance);
            var entry = new Entry(Title, parsedAmount, parsedDate, parsedIsMonthly, parsedImportance);

            Income.Add(entry);


        }

        public void OnPostDelete(int id)
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
            Debug.WriteLine("\n\n\n\n" + id);
            //code to remove by id
        }
    }
}
