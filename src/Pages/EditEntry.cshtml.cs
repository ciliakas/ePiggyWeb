using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EditEntryModel : PageModel
    {
        public IEntry Entry { get; set; }
        public int EntryTypeInt { get; set; }


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
        public void OnGet(int id, int entryType)
        {
            var dataManager = new DataManager();
            EntryTypeInt = entryType;

            Entry = entryType == 1
                ? dataManager.Income.EntryList.FirstOrDefault(x => x.Id == id)
                : dataManager.Expenses.EntryList.FirstOrDefault(x => x.Id == id);
            
        }

        public void OnPost()
        {
            if (!ModelState.IsValid) return;

            if (!decimal.TryParse(Amount, out var parsedAmount))
            {
                Error = "Amount is not a number!";
                return;
            }

            var parsedDate = Convert.ToDateTime(Date);
            var parsedIsMonthly = Convert.ToBoolean(IsMonthly);
            var parsedImportance = int.Parse(Importance);
            Entry.Title = Title;
            Entry.Amount = parsedAmount;
            Entry.Date = parsedDate;
            Entry.Recurring = parsedIsMonthly;
            Entry.Importance = parsedImportance;


            if (EntryTypeInt == 1)
            {
                EntryDbUpdater.Edit(Entry.Id, Entry, EntryType.Income);
                Redirect("/Income");
            }
            else
            {
                EntryDbUpdater.Edit(Entry.Id, Entry, EntryType.Expense);
                Redirect("/Expense");
            }

            
        }

        public void OnPostCancel()
        {
            if (EntryTypeInt == 1)
            {
                Redirect("/Income");
            }
            Redirect("/Expenses");
        }


    }
}
