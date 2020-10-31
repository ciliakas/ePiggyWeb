using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ePiggyWeb.Pages
{
    public class EntryModel : PageModel
    {
        public void OnGet()
        {
            var Expenses = new DataEntries();
            using var context = new DatabaseContext();
            var expenses = context.Expenses; // define query
            foreach (var expense in expenses.Where(x => x.UserId == 0)) // query executed and data obtained from database
            {
                var newExpense = new DataEntry(expense.Id, expense.UserId, expense.Amount, expense.Title, expense.Date, expense.IsMonthly, expense.Importance);
                Expenses.Add(newExpense);
            }
            ViewData["EntryList"] = Expenses;
        }
    }
}
