using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
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
        [BindProperty]
        public Entry Entry { get; set; }

        [BindProperty]
        public int EntryTypeInt { get; set; }
        
        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [BindProperty]
        public string Title { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [BindProperty]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [BindProperty]
        public string Date { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Importance is required")]
        public string Importance { get; set; }

        [BindProperty]
        public string Recurring { get; set; }

        public string Error { get; set; }
        public void OnGet(int id, int entryType)
        {
            var dataManager = new DataManager();
            EntryTypeInt = entryType;

            Entry = entryType == 1
                ? (Entry)dataManager.Income.EntryList.FirstOrDefault(x => x.Id == id)
                : (Entry)dataManager.Expenses.EntryList.FirstOrDefault(x => x.Id == id);

            if (Entry == null)
            {
                Response.Redirect("/Index");
            }
            Id = Entry.Id;
            UserId = Entry.UserId;
            Title = Entry.Title;
            Amount = Entry.Amount;
            Importance = Entry.Importance.ToString();


        }

        public void OnPost()
        {

            Debug.WriteLine("\n\n\n\n" + Id + "\t" + UserId + "\t" + EntryTypeInt);
           if (!ModelState.IsValid) return;

            /*if (!decimal.TryParse(Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
            {
                Error = "Amount is not a number!";
                return;
            }*/

            var parsedDate = Convert.ToDateTime(Date);
            var parsedIsMonthly = Convert.ToBoolean(Recurring);
            var parsedImportance = int.Parse(Importance);
            var entry = new Entry(Id,UserId,Title,Amount,parsedDate,parsedIsMonthly,parsedImportance);


            if (EntryTypeInt == 1)
            {
                EntryDbUpdater.Edit(entry.Id, entry, EntryType.Income);
                Response.Redirect("/Income");
            }
            else
            {
                EntryDbUpdater.Edit(entry.Id, entry, EntryType.Expense);
                Response.Redirect("/Expenses");
            }

            
        }

        public void OnPostCancel()
        {
            Debug.WriteLine("\n\n\n\n"  + EntryTypeInt);
            if (EntryTypeInt == 1)
            {
                Response.Redirect("/Income");
            }
            Response.Redirect("/Expenses");
        }


    }
}
