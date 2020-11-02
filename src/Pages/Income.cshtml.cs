using ePiggyWeb.DataManagement;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class IncomesModel : PageModel
    {
        public EntryList Income { get; set; }
        public void OnGet()
        {
            var incomeManager = new EntryManager(new EntryList(EntryType.Income));
            Income = incomeManager.EntryList;
        }
    }
}
