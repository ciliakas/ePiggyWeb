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
            GoalList.Add(goal);
            return true;
        }

        public bool Edit(IGoal oldGoal, IGoal updatedGoal)
        {
            return Edit(oldGoal.Id, updatedGoal);
        }

        public bool Edit(int id, IGoal updatedGoal)
        {
            if (!GoalDatabase.Update(id, UserId, updatedGoal))
            {
                return false;
            }
            var localGoal = GoalList.FirstOrDefault(x => x.Id == id);
            if (localGoal is null)
            {
                ExceptionHandler.Log("Edited goal id: " + id + " in database but couldn't find it locally");
                return false;
            }
            localGoal.Edit(updatedGoal);
            return true;
        }

        public bool Remove(IGoal goal)
        {
            return Remove(goal.Id);
        }

        public bool Remove(int id)
        {
            if (!GoalDatabase.Delete(id, UserId))
            {
                return false;
            }
            var localGoal = GoalList.FirstOrDefault(x => x.Id == id);
            if (localGoal is null)
            {
                ExceptionHandler.Log("Removed goal id: " + id + " from database but couldn't find it locally");
                return false;
            }

            GoalList.Remove(localGoal);
            return true;
        }

        public bool ReadFromDb()
        {
            GoalList.AddRange(GoalDatabase.ReadList(UserId));
            return true;
        }
    }
}
