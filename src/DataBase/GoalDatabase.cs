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
    public class GoalDatabase : IGoalDatabase
    {
        private PiggyDbContext Database { get; }
        public GoalDatabase(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task CreateAsync(IGoal goal, int userid)
        {
            var dbGoal = new GoalModel(goal, userid);
            await Database.AddAsync(dbGoal);
            await Database.SaveChangesAsync();
        }

        public async Task CreateListAsync(IGoalList goalList, int userid)
        {
            var dictionary = new Dictionary<IGoal, IGoalModel>();
            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);
                dictionary.Add(goal, dbGoal);
            }
            await Database.AddRangeAsync(dictionary.Values);
            await Database.SaveChangesAsync();
        }

        public async Task UpdateAsync(IGoal oldGoal, IGoal newGoal)
        {
            await UpdateAsync(oldGoal.Id, oldGoal.UserId, newGoal);
        }

        public async Task UpdateAsync(int id, int userId, IGoal newGoal)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
            if (dbGoal is null)
            {
               throw new Exception("Couldn't find goal id: " + id + " in database");
            }
            dbGoal.Edit(newGoal);
            await Database.SaveChangesAsync();
        }

        public async Task DeleteAsync(IGoal goal)
        {
            await DeleteAsync(goal.Id, goal.UserId);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            await DeleteAsync(x => x.Id == id && x.UserId == userId);
        }

        private async Task DeleteAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(filter);
            Database.Remove(dbGoal ?? throw new InvalidOperationException());
            await Database.SaveChangesAsync();
        }

        public async Task DeleteListAsync(IEnumerable<IGoal> goalList, int userId)
        {
            var idList = goalList.Select(goal => goal.Id).ToList();
            await DeleteListAsync(idList, userId);
        }

        private async Task DeleteListAsync(IEnumerable<int> idArray, int userId)
        {
            await DeleteListAsync(PredicateBuilder.BuildGoalFilter(idArray, userId));
        }

        private async Task DeleteListAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var goalsToRemove = await Database.Goals.Where(filter).ToListAsync();
            Database.RemoveRange(goalsToRemove);
            await Database.SaveChangesAsync();
        }

        public async Task MoveGoalToExpensesAsync(IGoal goal, IEntry expense)
        {
            await MoveGoalToExpensesAsync(goal.Id, goal.UserId, expense);
        }

        public async Task MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(x => x.Id == goalId && x.UserId == userId);
            Database.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
            var dbExpense = new ExpenseModel(expense, userId);
            await Database.Expenses.AddAsync(dbExpense);
            await Database.SaveChangesAsync();
        }

        public async Task<IGoal> ReadAsync(int id, int userId)
        {
            return await ReadAsync(x => x.Id == id && x.UserId == userId);
        }
        private async Task<IGoal> ReadAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(filter);
            return new Goal(dbGoal);
        }

        public async Task<IGoalList> ReadListAsync(int userId)
        {
            return await ReadListAsync(x => x.UserId == userId);
        }

        private async Task<IGoalList> ReadListAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var list = await Database.Goals.Where(filter).Select(dbGoal => new Goal(dbGoal)).Cast<IGoal>().ToListAsync();
            var goalList = new GoalList();
            goalList.AddRange(list);
            return goalList;
        }
    }
}