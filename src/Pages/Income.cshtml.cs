using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages.Finances
{
    public class IncomesModel : PageModel
    {
        public DataEntries Income { get; set; }
        public void OnGet()
        {

            Income = new DataEntries();
            using var context = new DatabaseContext();
            var income = context.Incomes; // define query
            foreach (var item in income.Where(x => x.UserId == 0)) // query executed and data obtained from database
            {
                var newExpense = new DataEntry(item.Id, item.UserId, item.Amount, item.Title, item.Date, item.IsMonthly, item.Importance);
                Income.Add(newExpense);
            }
           // ViewData["EntryList"] = Income;
        }
    }
}
