using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Goals
{
    public class GoalManager
    {
        public GoalList GoalList { get; }

        private int UserId { get; } = 0;

        public GoalManager()
        {
            GoalList = new GoalList();
        }

        public bool Add(Goal goal)
        {
            var id = GoalDbUpdater.Add(goal, UserId);
            //Check if id is correct, return false if something is wrong

            goal.Id = id;
            goal.UserId = UserId;
            GoalList.Add(goal);
            return false;
        }

        public bool Edit(Goal oldGoal, Goal newGoal)
        {
            if (!GoalDbUpdater.Edit(oldGoal, newGoal))
            {
                return false;
            }
            var localGoal = GoalList.FirstOrDefault(x => x.Id == oldGoal.Id);
            if (localGoal is null)
            {
                ExceptionHandler.Log("Edited goal id: " + oldGoal.Id + " in database but couldn't find it locally");
                return false;
            }
            localGoal.Edit(newGoal);
            return true;
        }

        public bool Remove(Goal goal)
        {
            if (!GoalDbUpdater.Remove(goal))
            {
                return false;
            }
            var localGoal = GoalList.FirstOrDefault(x => x.Id == goal.Id);
            if (localGoal is null)
            {
                ExceptionHandler.Log("Removed goal id: " + goal.Id  + " from database but couldn't find it locally" );
                return false;
            }

            GoalList.Remove(localGoal);
            return true;
        }
    }
}
