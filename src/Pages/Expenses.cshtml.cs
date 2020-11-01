using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ExpensesModel : PageModel
    {
        public EntryList Expenses { get; set; }

        public void OnGet()
        {
            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
        }
    }
}