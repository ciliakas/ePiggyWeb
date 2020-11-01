using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class IncomesModel : PageModel
    {
        public EntryList Income { get; set; }
        public void OnGet()
        {
            var incomeManager = new EntryManager(EntryType.Income);
            Income = incomeManager.EntryList;
        }
    }
}