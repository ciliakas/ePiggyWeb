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

            OnPostCancel(); //I entry is empty going back
        }

        /*Editing and redirecting according to EntryType*/
        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Entry.Title = Title;

            if (EntryTypeInt == 1)
            {
                EntryDatabase.Update(Entry.Id, Entry.UserId, Entry, EntryType.Income);
                return RedirectToPage("/Income");
            }
            else
            {
                EntryDatabase.Update(Entry.Id, Entry.UserId ,Entry, EntryType.Expense);
                return RedirectToPage("/Expenses");
            }
        }

        /*If cancel pressed return to previous page*/
        public IActionResult OnPostCancel()
        {
            return RedirectToPage(EntryTypeInt == 1 ? "/Income" : "/Expenses");
        }



    }
}
