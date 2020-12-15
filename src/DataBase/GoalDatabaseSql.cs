using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class GoalDatabaseSql
    {
        private PiggyDbContext Database { get; }
        public GoalDatabaseSql(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task<int> CreateAsync(IGoal goal, int userid)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var dbGoal = new GoalModel(goal, userid);

            var sqlCommand = new SqlCommand("INSERT INTO Goals(UserId, Price, Title) VALUES (@UserId, @Price, @Title);SELECT CAST(scope_identity() AS int);", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
            sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
            sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
            dbGoal.Id = (int)sqlCommand.ExecuteScalar();

            sqlConnection.Close();
            return dbGoal.Id;
        }

        public async Task<bool> CreateListAsync(IGoalList goalList, int userid)
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

        public async Task<bool> UpdateAsync(IGoal oldGoal, IGoal newGoal)
        {
            return await UpdateAsync(oldGoal.Id, oldGoal.UserId, newGoal);
        }

        public async Task<bool> UpdateAsync(int id, int userId, IGoal newGoal)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand("UPDATE Goals SET Price = @Price, Title = @Title WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Price", newGoal.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", newGoal.Title);
            await sqlCommand.ExecuteNonQueryAsync();

            sqlConnection.Close();
            return true;
        }

        public async Task<bool> DeleteAsync(IGoal goal)
        {
            return await DeleteAsync(goal.Id, goal.UserId);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            //return await DeleteAsync(x => x.Id == id && x.UserId == userId);
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();
            return true;
        }

        public async Task<bool> DeleteAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(filter);
            Database.Remove(dbGoal ?? throw new InvalidOperationException());
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteListAsync(IEnumerable<IGoal> goalList, int userId)
        {
            var idList = goalList.Select(goal => goal.Id).ToList();
            return await DeleteListAsync(idList, userId);
        }

        public async Task<bool> DeleteListAsync(IEnumerable<int> idArray, int userId)
        {
            return await DeleteListAsync(PredicateBuilder.BuildGoalFilter(idArray, userId));
        }

        public async Task<bool> DeleteListAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var goalsToRemove = await Database.Goals.Where(filter).ToListAsync();
            Database.RemoveRange(goalsToRemove);
            await Database.SaveChangesAsync();
            return true;
        }

        public async Task<int> MoveGoalToExpensesAsync(IGoal goal, IEntry expense)
        {
            return await MoveGoalToExpensesAsync(goal.Id, goal.UserId, expense);
        }

        public async Task<int> MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense)
        {
            var dbGoal = await Database.Goals.FirstOrDefaultAsync(x => x.Id == goalId && x.UserId == userId);
            Database.Goals.Remove(dbGoal ?? throw new InvalidOperationException());
            var dbExpense = new ExpenseModel(expense, userId);
            await Database.Expenses.AddAsync(dbExpense);
            await Database.SaveChangesAsync();
            return dbExpense.Id;
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

        public async Task<IGoalList> ReadListAsync(int userId)
        {
            var list = new GoalList();
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);
            var query = "SELECT * FROM Goals WHERE UserId =" + userId;
            var MyDataSet = new DataSet();
            var MyDataAdapter = new System.Data.SqlClient.SqlDataAdapter(query, sqlConnection);

            MyDataAdapter.Fill(MyDataSet);

            var table = MyDataSet.Tables[0];

            var items = (from DataRow row in table.Rows
                select new Goal
                {
                    Id = row.Field<int>("Id"),
                    Amount = row.Field<decimal>("Price"),
                    Title = row.Field<string>("Title"),
                    UserId = row.Field<int>("UserId")
                } as IGoal).ToList();

            list.AddRange(items);
            return list;
        }

        public async Task<IGoalList> ReadListAsync(Expression<Func<IGoalModel, bool>> filter)
        {
            var list = await Database.Goals.Where(filter).Select(dbGoal => new Goal(dbGoal)).Cast<IGoal>().ToListAsync();
            var goalList = new GoalList();
            goalList.AddRange(list);
            return goalList;
        }
    }
}