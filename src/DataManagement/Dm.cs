using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class Dm
    {
        public IEntryManager Income { get; }
        public IEntryManager Expenses { get; }
        public IGoalManager Goals { get; }

        private int _userId;
        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                Read();
            }
        }

        public Dm(IEntryManager income, IEntryManager expenses, IGoalManager goals, int userId = 0)
        {
            UserId = userId;
            Income = income;
            Expenses = expenses;
            Goals = goals;
            RecurringUpdater.UpdateRecurring(Income);
            RecurringUpdater.UpdateRecurring(Expenses);
        }

        public void Read()
        {

        }
    }
}
