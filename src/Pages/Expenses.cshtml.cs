using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class ExpensesModel : PageModel
    {
        public IEntryList Expenses { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [BindProperty]
        public string Title { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        [BindProperty]
        public string Amount { get; set; }
        [BindProperty]
        public string Date { get; set; }
        [BindProperty]
        [Required(ErrorMessage = "Importance is required")]
        public string Importance { get; set; }
        [BindProperty]
        public string IsMonthly { get; set; }

        public string Error { get; set; }

        public void OnGet()
        {
            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;
            //var expensesManager = new EntryManager(EntryType.Expense);
            //Expenses = expensesManager.EntryList;
        }

        public void OnPostNewEntry()
        {
            var dataManager = new DataManager();
            if (!ModelState.IsValid)
            {
                Expenses = dataManager.Expenses.EntryList;
                return;
            }
            
            if (!decimal.TryParse(Amount, out var parsedAmount))
            {
                Error = "Amount is not a number!";
                Expenses = dataManager.Expenses.EntryList;
                return;
            }

            var parsedDate = Convert.ToDateTime(Date);
            var parsedIsMonthly = Convert.ToBoolean(IsMonthly);
            var parsedImportance = int.Parse(Importance);
            var Entry = new Entry(Title, parsedAmount, parsedDate, parsedIsMonthly, parsedImportance);
            // Expenses.EntryList.Add(Entry);

            Expenses = dataManager.Expenses.EntryList;


        }

        public void OnPostDelete()
        {

            var dataManager = new DataManager();
            Expenses = dataManager.Expenses.EntryList;

        }
    }
}
