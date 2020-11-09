using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePiggyWeb.DataManagement.Goals
{
    public interface IGoalManager
    {
        public IGoalList GoalList { get; }
        public int UserId { get; }
        public bool Add(IGoal goal);
        public bool Edit(IGoal oldGoal, IGoal updatedGoal);
        public bool Edit(int id, IGoal updatedGoal);
        public bool Remove(IGoal goal);
        public bool Remove(int id);
    }
}
