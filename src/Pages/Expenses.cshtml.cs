using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ExpensesModel : PageModel
    {
        public DataEntries Expenses { get; set; }

        public void OnGet()
        {

            Expenses = new DataEntries();
            using var context = new DatabaseContext();
            var expense = context.Expenses; // define query
            foreach (var item in expense.Where(x => x.UserId == 0)) // query executed and data obtained from database
            {
                var newExpense = new DataEntry(item.Id, item.UserId, item.Amount, item.Title, item.Date, item.IsMonthly,
                    item.Importance);
                Expenses.Add(newExpense);
            }
        }
    }
}
