using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class IncomesModel : PageModel
    {
        public EntryList Income { get; set; }
        public void OnGet()
        {
            var dataManager = new DataManager();
            Income = dataManager.Income.EntryList;
        }
    }
}
