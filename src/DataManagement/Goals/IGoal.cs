using System;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.DataManagement.Goals
{
    //Name could use some work
    public interface IGoal : IComparable<IGoal>, IComparable<decimal>, IEquatable<IGoal>, IEquatable<decimal>
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public void Edit(IGoal goal);
    }
}
