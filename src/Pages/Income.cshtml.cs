using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    [Authorize]
    public class IncomesModel : PageModel
    {
        public IEntryList Income { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
        }
    }
}
