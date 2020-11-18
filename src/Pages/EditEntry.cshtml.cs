using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
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
        [Required]
        [BindProperty]
        public Entry Entry { get; set; }

        [Required(ErrorMessage = "Required")]
        [BindProperty]
        public string Title { get; set; }

        [BindProperty]
        public int EntryTypeInt { get; set; }
        private EntryDatabase EntryDatabase { get; }
        public EditEntryModel(EntryDatabase entryDatabase)
        {
            EntryDatabase = entryDatabase;
        }

        public async Task OnGet(int id, int entryType)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.Name).Value);
            EntryTypeInt = entryType;

            Entry = entryType == 1
                ? (Entry) await EntryDatabase.ReadAsync(id, userId, EntryType.Income)
                : (Entry) await EntryDatabase.ReadAsync(id, userId, EntryType.Expense);

            if (Entry != null)
            {
                Title = Entry.Title;
                return;
            }

            OnPostCancel(); //I entry is empty going back
        }

        /*Editing and redirecting according to EntryType*/
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Entry.Title = Title;

            if (EntryTypeInt == 1)
            {
                await EntryDatabase.UpdateAsync(Entry.Id, Entry.UserId, Entry, EntryType.Income);
                return RedirectToPage("/Income");
            }
            else
            {
                await EntryDatabase.UpdateAsync(Entry.Id, Entry.UserId ,Entry, EntryType.Expense);
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
