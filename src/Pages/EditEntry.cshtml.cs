using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using ePiggyWeb.CurrencyAPI;
using ePiggyWeb.DataBase;
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

        [Required(ErrorMessage = "Title Required.")]
        [BindProperty]
        [StringLength(25, ErrorMessage = "Too long title!")]
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
            OnPostCancel();
        }

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

            await EntryDatabase.UpdateAsync(Entry.Id, Entry.UserId ,Entry, EntryType.Expense);
            return RedirectToPage("/Expenses");
        }

        public IActionResult OnPostCancel()
        {
            return RedirectToPage(EntryTypeInt == 1 ? "/Income" : "/Expenses");
        }
    }
}
