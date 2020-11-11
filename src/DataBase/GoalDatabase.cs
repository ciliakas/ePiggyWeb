using System;
using System.Collections.Generic;
using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class GoalDatabase
    {
        public static int Create(IGoal goal, int userid)
        {
            using var db = new DatabaseContext();
            var dbGoal = new GoalModel (goal, userid);
            db.Add(dbGoal);
            db.SaveChanges();
            return dbGoal.Id;
        }

        public static bool CreateList(IGoalList goalList, int userid)
        {
            using var db = new DatabaseContext();
            var dictionary = new Dictionary<IGoal, IGoalModel>();
            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);
                dictionary.Add(goal, dbGoal);
            }
            // Setting all of the ID's to local Entries, just so this method remains usable both with local and only database usage
            db.AddRange(dictionary.Values);
            db.SaveChanges();
            goalList.Clear();
            foreach (var (key, value) in dictionary)
            {
                key.Id = value.Id;
                goalList.Add(key);
            }
            return true;
        }

        public static bool Update(IGoal oldGoal, IGoal newGoal)
        {
            return Update(oldGoal.Id, oldGoal.UserId, newGoal);
        }

        public static bool Update(int id, int userId, IGoal newGoal)
        {
            using var db = new DatabaseContext();
            var dbGoal = db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            if (dbGoal == null)
            {
                ExceptionHandler.Log("Couldn't find goal id: " + id + " in database");
                return false;
            }
            dbGoal.Edit(newGoal);
            db.SaveChanges();
            return true;
        }

        public static bool Delete(IGoal goal)
        {
            return Delete(goal.Id, goal.UserId);
        }

        public static bool Delete(int id, int userId)
        {
            return Delete(x => x.Id == id && x.UserId == userId);
        }

        public static bool Delete(Func<GoalModel, bool> filter)
        {
            using var db = new DatabaseContext();
            try
            {
                var dbGoal = db.Goals.FirstOrDefault(filter);
                db.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                db.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                return false;
            }
            return true;
        }

        public static bool DeleteList(IEnumerable<IGoal> goalList)
        {
            var enumerable = goalList as IGoal[] ?? goalList.ToArray();
            if (!enumerable.Any())
            {
                return false;
            }
            var userId = enumerable.First().UserId;
            var idList = enumerable.Select(goal => goal.Id).ToList();
            return DeleteList(idList, userId);
        }

        public static bool DeleteList(IEnumerable<int> idArray, int userId)
        {
            using var db = new DatabaseContext();
            var goalsToRemove = idArray.Select(id => db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId)).ToList();
            db.Goals.RemoveRange(goalsToRemove);
            db.SaveChanges();
            return true;
        }

        public static bool DeleteList(Func<GoalModel, bool> filter)
        {
            using var db = new DatabaseContext();
            var goalsToRemove =  db.Goals.Where(filter).ToList();
            db.Goals.RemoveRange(goalsToRemove);
            db.SaveChanges();
            return true;
        }

        public static int MoveGoalToExpenses(IGoal goal, IEntry expense)
        {
            return MoveGoalToExpenses(goal.Id, goal.UserId, expense);
        }

        public static int MoveGoalToExpenses(int goalId, int userId, IEntry expense)
        {
            using var db = new DatabaseContext();
            try
            {
                var dbGoal = db.Goals.FirstOrDefault(x => x.Id == goalId && x.UserId == userId);
                db.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                var dbExpense = new ExpenseModel(expense, userId);
                db.Expenses.Add(dbExpense);
                db.SaveChanges();
                return dbExpense.Id;
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find goal id: " + goalId + " in database");
                return -1;
            }
        }

        public static IGoal Read(int id, int userId)
        {
            using var db = new DatabaseContext();
            var dbGoal = db.Goals.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            return new Goal(dbGoal);
        }

        public static IEnumerable<IGoal> ReadList(int userId)
        {
            return ReadList(x => x.UserId == userId);
        }

        public static IEnumerable<IGoal> ReadList(Func<GoalModel, bool> filter)
        {
            using var db = new DatabaseContext();
            return db.Goals.Where(filter).Select(dbGoal => new Goal(dbGoal)).Cast<IGoal>().ToList();
        }
    }
}
