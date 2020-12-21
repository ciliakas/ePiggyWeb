using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
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
                await sqlConnection.OpenAsync();
            }

            var dbGoal = new GoalModel(goal, userid);

            var sqlCommand = new SqlCommand(
            "INSERT INTO Goals(UserId, Price, Title) VALUES (@UserId, @Price, @Title);SELECT CAST(scope_identity() AS int);",
                    sqlConnection)
            { CommandType = CommandType.Text };
            sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
            sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
            sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
            dbGoal.Id = (int)await sqlCommand.ExecuteScalarAsync();

            await sqlConnection.CloseAsync();
            return dbGoal.Id;
        }

        public async Task<bool> CreateListAsync(IGoalList goalList, int userid)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand(
                "INSERT INTO Goals(UserId, Price, Title) VALUES (@UserId, @Price, @Title);SELECT CAST(scope_identity() AS int);",
                sqlConnection)
            { CommandType = CommandType.Text };

            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);

                sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
                sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
                sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
                dbGoal.Id = (int)await sqlCommand.ExecuteScalarAsync();
            }

            await sqlConnection.CloseAsync();
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

            var sqlCommand = new SqlCommand(
                "UPDATE Goals SET Price = @Price, Title = @Title WHERE Id = @Id AND UserId = @UserId",
                    sqlConnection)
            { CommandType = CommandType.Text };
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Price", newGoal.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", newGoal.Title);
            await sqlCommand.ExecuteNonQueryAsync();

            await sqlConnection.CloseAsync();
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
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection)
            {
                CommandType = CommandType.Text
            };
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            await sqlCommand.ExecuteNonQueryAsync();

            await sqlConnection.CloseAsync();
            return true;
        }

        public async Task<bool> DeleteListAsync(IEnumerable<IGoal> goalList, int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var idList = goalList.Select(goal => goal.Id).ToList();

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection)
            {
                CommandType = CommandType.Text
            };
            sqlCommand.Parameters.AddWithValue("@UserId", userId);

            foreach (var id in idList)
            {
                sqlCommand.Parameters.AddWithValue("@Id", id);
                await sqlCommand.ExecuteNonQueryAsync();
            }

            await sqlConnection.CloseAsync();
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
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand("DELETE FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection)
            {
                CommandType = CommandType.Text
            };
            sqlCommand.Parameters.AddWithValue("@Id", goalId);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            await sqlCommand.ExecuteNonQueryAsync();

            var dbExpense = new ExpenseModel(expense, userId);
            sqlCommand =
                new SqlCommand(
                    "INSERT INTO Expenses(UserId, Amount, Title) VALUES (@UserId, @Amount, @Title);SELECT CAST(scope_identity() AS int);",
                    sqlConnection)
                { CommandType = CommandType.Text };
            sqlCommand.Parameters.AddWithValue("@UserId", expense.UserId);
            sqlCommand.Parameters.AddWithValue("@Amount", expense.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", expense.Title);
            dbExpense.Id = (int)await sqlCommand.ExecuteScalarAsync();

            await sqlConnection.CloseAsync();
            return dbExpense.Id;
        }

        public async Task<IGoal> ReadAsync(int id, int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand("SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId", sqlConnection)
            {
                CommandType = CommandType.Text
            };
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            var reader = await sqlCommand.ExecuteReaderAsync();

            var dbGoal = new Goal();

            if (reader.HasRows)
            {
                await reader.ReadAsync();

                dbGoal.Id = reader.GetInt32(0);
                dbGoal.UserId = reader.GetInt32(1);
                dbGoal.Title = reader.GetString(3);
                dbGoal.Amount = reader.GetDecimal(2);
            }

            await sqlConnection.CloseAsync();
            return dbGoal;
        }

        public async Task<IGoalList> ReadListAsync(int userId)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var list = new GoalList();
            var query = "SELECT * FROM Goals WHERE UserId =" + userId;
            var myDataSet = new DataSet();
            var myDataAdapter = new SqlDataAdapter(query, sqlConnection);
            myDataAdapter.Fill(myDataSet);

            var table = myDataSet.Tables[0];

            var items = (from DataRow row in table.Rows
                         select new Goal
                         {
                             Id = row.Field<int>("Id"),
                             Amount = row.Field<decimal>("Price"),
                             Title = row.Field<string>("Title"),
                             UserId = row.Field<int>("UserId")
                         } as IGoal).ToList();

            await sqlConnection.CloseAsync();
            list.AddRange(items);
            return list;
        }
    }
}