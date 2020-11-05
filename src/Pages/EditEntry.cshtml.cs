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
        [Required]
        [BindProperty]
        public Entry Entry { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public int EntryTypeInt { get; set; }
        
        public void OnGet(int id, int entryType)
        {
            var dataManager = new DataManager();
            EntryTypeInt = entryType;

            Entry = entryType == 1
                ? (Entry)dataManager.Income.EntryList.FirstOrDefault(x => x.Id == id)
                : (Entry)dataManager.Expenses.EntryList.FirstOrDefault(x => x.Id == id);

            if (Entry != null)
            {
                Title = Entry.Title;
                return;
            }
            Response.Redirect(EntryTypeInt == 1 ? "/Income" : "/Expenses");
        }

        /*Editing and redirecting according to EntryType*/
        public void OnPost()
        {
            if (!ModelState.IsValid) return;

            if (EntryTypeInt == 1)
            {
                EntryDbUpdater.Edit(Entry.Id, Entry, EntryType.Income);
                Response.Redirect("/Income");
            }
            else
            {
                EntryDbUpdater.Edit(Entry.Id, Entry, EntryType.Expense);
                Response.Redirect("/Expenses");
            }
        }

        /*If cancel pressed return to previous page*/
        public void OnPostCancel()
        {
            if (EntryTypeInt == 1)
            {
                Response.Redirect("/Income");
                return;
            }
            Response.Redirect("/Expenses");
        }


    }
}
