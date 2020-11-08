using System.Linq;
using ePiggyWeb.DataBase;
using ePiggyWeb.Utilities;
using IListExtension;

namespace ePiggyWeb.DataManagement.Goals
{
    public class GoalManager : IGoalManager
    {
        public IGoalList GoalList { get; }

        public int UserId { get; }

        public GoalManager(IGoalList goalList, int userId = 0)
        {
            GoalList = goalList;
            UserId = userId;
            ReadFromDb();
        }

        public bool Add(IGoal goal)
        {
            var id = GoalDatabase.Create(goal, UserId);
            //Check if id is correct, return false if something is wrong

            goal.Id = id;
            goal.UserId = UserId;
            GoalList.Add(goal);
            return false;
        }

        public bool Edit(IGoal oldGoal, IGoal newGoal)
        {
            if (!GoalDatabase.Update(oldGoal, newGoal))
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

        public bool Remove(IGoal goal)
        {
            if (!GoalDatabase.Delete(goal))
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

        public bool ReadFromDb()
        {
            GoalList.AddRange(GoalDatabase.Read(UserId));
            return true;
        }
    }
}
