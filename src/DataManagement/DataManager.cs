using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataManager
    {
        private EntryList LocalIncome { get; } = new EntryList(EntryType.Income);

        private EntryList LocalExpenses { get; } = new EntryList(EntryType.Expense);

        public EntryManager Income { get; }

        public EntryManager Expenses { get; }

        public DataManager()
        {
            Income = new EntryManager(LocalIncome);
            Expenses = new EntryManager(LocalExpenses);
            Income.ReadFromDb(0);
            Expenses.ReadFromDb(0);
        }
    }
}
