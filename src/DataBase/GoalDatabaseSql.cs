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
    public class GoalDatabaseSql : IGoalDatabase
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
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var sqlCommand = new SqlCommand("INSERT INTO Goals(UserId, Price, Title) VALUES (@UserId, @Price, @Title);SELECT CAST(scope_identity() AS int);", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;

            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);

                sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
                sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
                sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
                dbGoal.Id = (int)sqlCommand.ExecuteScalar();
            }
            sqlConnection.Close();
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
                sqlConnection.Open();
            }

            var sqlCommand = new SqlCommand("UPDATE Goals SET Price = @Price, Title = @Title WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Price", newGoal.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", newGoal.Title);
            sqlCommand.ExecuteNonQuery();

            sqlConnection.Close();
            return true;
        }

        public async Task<bool> DeleteAsync(IGoal goal)
        {
            return await DeleteAsync(goal.Id, goal.UserId);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {

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

        public async Task<bool> DeleteListAsync(IEnumerable<IGoal> goalList, int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var idList = goalList.Select(goal => goal.Id).ToList();

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@UserId", userId);

            foreach (var id in idList)
            {
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlCommand.ExecuteNonQuery();
            }
            sqlConnection.Close();
            return true;
        }

        public async Task<int> MoveGoalToExpensesAsync(IGoal goal, IEntry expense)
        {
            return await MoveGoalToExpensesAsync(goal.Id, goal.UserId, expense);
        }

        public async Task<int> MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Id", goalId);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.ExecuteNonQuery();

            var dbExpense = new ExpenseModel(expense, userId);
            sqlCommand = new SqlCommand("INSERT INTO Expenses(UserId, Amount, Title) VALUES (@UserId, @Amount, @Title);SELECT CAST(scope_identity() AS int);", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@UserId", expense.UserId);
            sqlCommand.Parameters.AddWithValue("@Amount", expense.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", expense.Title);
            dbExpense.Id = (int)sqlCommand.ExecuteScalar();

            sqlConnection.Close();
            return dbExpense.Id;
        }

        public async Task<IGoal> ReadAsync(int id, int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }
            var sqlCommand = new SqlCommand("SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection);
            sqlCommand.CommandType = CommandType.Text;
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            var reader = sqlCommand.ExecuteReader();

            var dbGoal = new Goal();

            if (reader.HasRows)
            {
                reader.Read();

                dbGoal.Id = reader.GetInt32(0);
                dbGoal.UserId = reader.GetInt32(1);
                dbGoal.Title = reader.GetString(2);
                dbGoal.Amount = reader.GetDecimal(3);
            }
            sqlConnection.Close();
            return dbGoal;
        }

        public async Task<IGoalList> ReadListAsync(int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                sqlConnection.Open();
            }

            var list = new GoalList();
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

            sqlConnection.Close();
            list.AddRange(items);
            return list;
        }
    }
}