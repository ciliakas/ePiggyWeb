using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
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
        public decimal Amount { get; set; }

        [BindProperty]
        public DateTime Date { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Importance is required")]
        public int Importance { get; set; }

        [BindProperty]
        public bool Recurring { get; set; }

        //public string Error { get; set; }
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
            Importance = Entry.Importance;
        }

        public void OnPost()
        {
            if (!ModelState.IsValid) return;

            var entry = new Entry(Id,UserId,Title,Amount, Date, Recurring, Importance);


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
