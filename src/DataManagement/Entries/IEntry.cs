using System;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntry : IGoal
    {
        /*
        My reasoning was that at the core, these two types are very connected, and since we want to have the feature of converting a goal into expense, 
        this kind of neatly connects the two.
        At the same time, since these are just interfaces and not concrete classes, 
        it wouldn't take too much time to implement this change if the need arose.
         */
        public DateTime Date { get; set; }
        public bool Recurring { get; set; }
        public int Importance { get; set; }
        public void Edit(IEntry newEntry);
    }
}
