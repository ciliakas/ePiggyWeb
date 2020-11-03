using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataManager
    {
        public IEntryManager Income { get; } = new EntryManager(new EntryList(EntryType.Income));

        public IEntryManager Expenses { get; } = new EntryManager(new EntryList(EntryType.Expense));

        public IGoalManager Goals { get; } = new GoalManager(new GoalList());

    }
}
