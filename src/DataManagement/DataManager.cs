using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement
{
    public class DataManager
    {
        /*
        This is the main class for the back end. With this object, we can do everything that we'd need with the back end
        In the future update of this branch, the constructor for DataManager will require UserId (default will be set to 0),
        and after that we won't ever need to pass another parameter to this class
        Basically I've tried to make this as class as self contained as possible, without any dependencies on the class which might call it
         */
        public IEntryManager Income { get; } = new EntryManager(new EntryList(EntryType.Income));

        public IEntryManager Expenses { get; } = new EntryManager(new EntryList(EntryType.Expense));

        public IGoalManager Goals { get; } = new GoalManager(new GoalList());

    }
}
