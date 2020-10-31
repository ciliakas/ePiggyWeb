using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataManager
    {
        public EntryManager Income { get; } = new EntryManager(EntryType.Income);

        public EntryManager Expenses { get; } = new EntryManager(EntryType.Expense);

        public GoalManager Goals { get; } = new GoalManager();

    }
}
