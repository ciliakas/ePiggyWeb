﻿using System.Collections.Generic;

namespace ePiggyWeb.DataManagement.Goals
{
    public interface IGoalManager
    {
        public IGoalList GoalList { get; }
        public int UserId { get; }
        public bool Add(IGoal goal);
        public bool AddRange(IGoalList goalList);
        public bool Edit(IGoal oldGoal, IGoal updatedGoal);
        public bool Edit(int id, IGoal updatedGoal);
        public bool Remove(IGoal goal);
        public bool Remove(int id);
        public bool RemoveAll(IGoalList entryList);
        public bool RemoveAll(IEnumerable<int> idList);
    }
}
