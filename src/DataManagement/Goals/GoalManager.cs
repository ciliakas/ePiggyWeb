//using System;
//using System.Collections.Generic;
//using System.Linq;
//using ePiggyWeb.DataBase;
//using ePiggyWeb.DataManagement.Entries;
//using ePiggyWeb.Utilities;
//using IListExtension;

//namespace ePiggyWeb.DataManagement.Goals
//{
//    public class GoalManager : IGoalManager
//    {
//        public IGoalList GoalList { get; }

//        public int UserId { get; }

//        public GoalManager(IGoalList goalList, int userId = 0)
//        {
//            GoalList = goalList;
//            UserId = userId;
//            ReadFromDb();
//        }

//        public bool Add(IGoal goal)
//        {
//            var id = GoalDatabaseOld.Create(goal, UserId);

//            if (id <= 0)
//            {
//                throw new Exception("Invalid id of goal");
//            }

//            goal.Id = id;
//            GoalList.Add(goal);
//            return true;
//        }

//        public bool AddRange(IGoalList goalList)
//        {
//            if (!GoalDatabaseOld.CreateList(goalList, UserId))
//            {
//                return false;
//            }

//            GoalList.AddRange(goalList);
//            return true;
//        }

//        public bool Edit(IGoal oldGoal, IGoal updatedGoal)
//        {
//            return Edit(oldGoal.Id, updatedGoal);
//        }

//        public bool Edit(int id, IGoal updatedGoal)
//        {
//            if (!GoalDatabaseOld.Update(id, UserId, updatedGoal))
//            {
//                return false;
//            }

//            var localGoal = GoalList.FirstOrDefault(x => x.Id == id);

//            if (localGoal is null)
//            {
//                throw new Exception("Edited goal id: " + id + " in database but couldn't find it locally");
//            }
//            localGoal.Edit(updatedGoal);
//            return true;
//        }

//        public bool Remove(IGoal goal)
//        {
//            return Remove(goal.Id);
//        }

//        public bool Remove(int id)
//        {
//            if (!GoalDatabaseOld.Delete(id, UserId))
//            {
//                return false;
//            }

//            var localGoal = GoalList.FirstOrDefault(x => x.Id == id);

//            if (localGoal is null)
//            {
//                throw new Exception("Removed goal id: " + id + " from database but couldn't find it locally");
//            }

//            GoalList.Remove(localGoal);
//            return true;
//        }

//        public bool RemoveAll(IGoalList entryList)
//        {
//            var idList = entryList.Select(va => va.Id).ToArray();

//            if (!GoalDatabaseOld.DeleteList(idList, UserId))
//            {
//                return false;
//            }

//            GoalList.RemoveAll(entryList.Contains);
//            return true;
//        }

//        public bool RemoveAll(IEnumerable<int> idList)
//        {
//            var idArray = idList as int[] ?? idList.ToArray();

//            if (!GoalDatabaseOld.DeleteList(idArray, UserId))
//            {
//                return false;
//            }

//            var temp = new GoalList();
//            temp.AddRange(
//                from goal in GoalList 
//                from id in idArray 
//                where goal.Id == id 
//                select goal);

//            foreach (var entry in temp)
//            {
//                GoalList.Remove(entry);
//            }

//            return true;
//        }


//        public void ReadFromDb()
//        {
//            GoalList.AddRange(GoalDatabaseOld.ReadList(UserId));
//        }

//        public bool MoveGoalToExpenses(IGoal goal, IEntry expense, IEntryManager entryManager)
//        {
//            return Remove(goal.Id) && entryManager.Add(expense);
//        }

//        public bool MoveGoalToExpenses(int goalId, IEntry expense, IEntryManager entryManager)
//        {
//            return Remove(goalId) && entryManager.Add(expense);
//        }
//    }
//}
