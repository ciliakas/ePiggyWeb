using ePiggyWeb.DataManagement;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        public void OnGet()
        {

            var dataManager = new DataManager();
            ViewData["EntryList"] = dataManager.Income.ToString();
        }
    }
}
