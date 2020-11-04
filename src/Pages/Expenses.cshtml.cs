using System;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ExpensesModel : PageModel
    {
        public IEntryList Expenses { get; set; }

        [BindProperty]
        public string Title { get; set; }
        [BindProperty]
        public string Amount { get; set; }
        [BindProperty]
        public string Date { get; set; }
        [BindProperty]
        public string Importance { get; set; }
        [BindProperty]
        public string IsMonthly { get; set; }

        public string Error { get; set; }

        public void OnGet()
        {
            //kaþkas neveikia paspaudus post, meta kad tuðèias List???
            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
            //var expensesManager = new EntryManager(EntryType.Expense);
            //Expenses = expensesManager.EntryList;
        }

        public void OnPost()
        {
            if (!decimal.TryParse(Amount, out var parsedAmount))
            {
                Error = "Amount is not a number!";
                return;
            }

            var parsedDate = Convert.ToDateTime(Date);
            var parsedIsMonthly = Convert.ToBoolean(IsMonthly);
            var parsedImportance = int.Parse(Importance);
            var Entry = new Entry(Title, parsedAmount,parsedDate,parsedIsMonthly,parsedImportance);
            //kodas pridët á listus (bûtø gerai jei Arnas padarytø)
        }
    }
}
