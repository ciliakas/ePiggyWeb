using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class GoalDb
    {
        private PiggyDbContext Database { get; }
        public GoalDb(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task<int> Create(IGoal goal, int userid)
        {
            var dbGoal = new GoalModel(goal, userid);
            await Database.AddAsync(dbGoal);
            await Database.SaveChangesAsync();
            return dbGoal.Id;
        }

        public async Task<bool> CreateList(IGoalList goalList, int userid)
        {
            var dictionary = new Dictionary<IGoal, IGoalModel>();
            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);
                dictionary.Add(goal, dbGoal);
            }
            // Setting all of the ID's to local Entries, just so this method remains usable both with local and only database usage
            await Database.AddRangeAsync(dictionary.Values);
            await Database.SaveChangesAsync();
            goalList.Clear();
            foreach (var (key, value) in dictionary)
            {
                key.Id = value.Id;
                goalList.Add(key);
            }
            return true;
        }

        public async Task<bool> Update(IGoal oldGoal, IGoal newGoal)
        {
            return await Update(oldGoal.Id, oldGoal.UserId, newGoal);
        }

        public async Task<bool> Update(int id, int userId, IGoal newGoal)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (dbGoal == null)
            {
                ExceptionHandler.Log("Couldn't find goal id: " + id + " in database");
                return false;
            }
            dbGoal.Edit(newGoal);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(IGoal goal)
        {
            return await Delete(goal.Id, goal.UserId);
        }

        public async Task<bool> Delete(int id, int userId)
        {
            return await Delete(x => x.Id == id && x.UserId == userId);
        }

        public async Task<bool> Delete(Expression<Func<GoalModel, bool>> filter)
        {
            try
            {
                var dbGoal = await Database.Goals.FirstOrDefaultAsync(filter);
                Database.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                await Database.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteList(IEnumerable<IGoal> goalList)
        {
            var enumerable = goalList as IGoal[] ?? goalList.ToArray();
            if (!enumerable.Any())
            {
                return false;
            }
            var userId = enumerable.First().UserId;
            var idList = enumerable.Select(goal => goal.Id).ToList();
            return await DeleteList(idList, userId);
        }

        public async Task<bool> DeleteList(IEnumerable<int> idArray, int userId)
        {
            var list = new List<GoalModel>();
            foreach (var id in idArray)
            {
                var temp = await Database.Goals.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
                list.Add(temp);
            }
            
            Database.Goals.RemoveRange(list);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteList(Expression<Func<GoalModel, bool>> filter)
        {
            var goalsToRemove = await Database.Goals.Where(filter).ToListAsync();
            Database.Goals.RemoveRange(goalsToRemove);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<int> MoveGoalToExpenses(IGoal goal, IEntry expense)
        {
            return await MoveGoalToExpenses(goal.Id, goal.UserId, expense);
        }

        public async Task<int> MoveGoalToExpenses(int goalId, int userId, IEntry expense)
        {
            try
            {
                var dbGoal = await Database.Goals.FirstOrDefaultAsync(x => x.Id == goalId && x.UserId == userId);
                Database.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
                var dbExpense = new ExpenseModel(expense, userId);
                await Database.Expenses.AddAsync(dbExpense);
                await Database.SaveChangesAsync();
                return dbExpense.Id;
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Log(ex.ToString());
                ExceptionHandler.Log("Couldn't find goal id: " + goalId + " in database");
                return -1;
            }
        }

        public async Task<IGoal> ReadAsync(int id, int userId)
        {
            return await ReadAsync(x => x.Id == id && x.UserId == userId);
        }

        public async Task<IGoal> ReadAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(filter);
            return new Goal(dbGoal);
        }

        public async Task<IEnumerable<IGoal>> ReadListAsync(int userId)
        {
            return await ReadListAsync(x => x.UserId == userId);
        }

        public async Task<IEnumerable<IGoal>> ReadListAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            return await Database.Goals.Where(filter).Select(dbGoal => new Goal(dbGoal)).Cast<IGoal>().ToListAsync();
        }
    }
}
