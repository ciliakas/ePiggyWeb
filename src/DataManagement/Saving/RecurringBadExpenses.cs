using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ePiggyWeb.DataManagement.Saving
{
    public class RecurringBadExpenses
    {
        public List<IEntry> GetRecurringBadExpensesList(IEntryList entryList)
        {
            //var recurringBadExpensesList = new List<Entry>();

            
            var enumCount = Enum.GetValues(typeof(Importance)).Length;
            var tempList1 = entryList.GetBy((Importance)enumCount);
            var tempList2 = entryList.GetBy((Importance)enumCount - 1);
            var badExpensesList = tempList1.Concat(tempList2); //Connects both lists
            
            
            /*
            var recurringBadExpensesList = from entry in entryList
                                           group entry by new
                                           {
                                               Importance.Unnecessary,
                                               entry.Recurring,
                                           }
            */

            var recurringBadExpensesList = badExpensesList.GroupBy(e => e.Recurring).ToList();

            /*
            var recurringBadExpensesList = badExpensesList
                .GroupBy(e => e.Recurring)
                .Select(exp => exp.ToList())
                .ToList();
            */

            /*
            var recurringBadExpensesList = from expense in badExpensesList
                                           group expense by expense.Recurring into recurringExpenses
                                           select recurringExpenses;
            */
            //var result = recurringBadExpensesList.ToList;
;
            return recurringBadExpensesList;
        } 
    }
}
