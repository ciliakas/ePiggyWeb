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

        public async Task CreateAsync(IGoal goal, int userid)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var dbGoal = new GoalModel(goal, userid);

            var sqlCommand = new SqlCommand(
            "INSERT INTO Goals(UserId, Price, Title, Currency) " +
                "VALUES (@UserId, @Price, @Title, @Currency);SELECT CAST(scope_identity() AS int);",
                    sqlConnection)
            { CommandType = CommandType.Text };
            sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
            sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
            sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
            sqlCommand.Parameters.AddWithValue("@Currency", dbGoal.Currency);
            dbGoal.Id = (int)await sqlCommand.ExecuteScalarAsync();

            await sqlConnection.CloseAsync();
        }

        public async Task CreateListAsync(IGoalList goalList, int userid)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand(
                    "INSERT INTO Goals(UserId, Price, Title, Currency) " +
                    "VALUES (@UserId, @Price, @Title, @Currency);SELECT CAST(scope_identity() AS int);",
                sqlConnection)
            { CommandType = CommandType.Text };

            foreach (var goal in goalList)
            {
                var dbGoal = new GoalModel(goal, userid);

                sqlCommand.Parameters.AddWithValue("@UserId", dbGoal.UserId);
                sqlCommand.Parameters.AddWithValue("@Price", dbGoal.Price);
                sqlCommand.Parameters.AddWithValue("@Title", dbGoal.Title);
                sqlCommand.Parameters.AddWithValue("@Currency", dbGoal.Currency);
                dbGoal.Id = (int)await sqlCommand.ExecuteScalarAsync();
            }

            await sqlConnection.CloseAsync();
        }

        public async Task UpdateAsync(IGoal oldGoal, IGoal newGoal)
        {
            await UpdateAsync(oldGoal.Id, oldGoal.UserId, newGoal);
        }

        public async Task UpdateAsync(int id, int userId, IGoal newGoal)
        {
            var sqlConnection = new SqlConnection(Database.Database.GetDbConnection().ConnectionString);

            if (sqlConnection.State == ConnectionState.Closed)
            {
                await sqlConnection.OpenAsync();
            }

            var sqlCommand = new SqlCommand(
                "UPDATE Goals SET Price = @Price, Title = @Title, Currency = @Currency WHERE Id = @Id AND UserId = @UserId",
                    sqlConnection)
            { CommandType = CommandType.Text };
            sqlCommand.Parameters.AddWithValue("@Id", id);
            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Price", newGoal.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", newGoal.Title);
            sqlCommand.Parameters.AddWithValue("@Currency", newGoal.Currency);
            await sqlCommand.ExecuteNonQueryAsync();

            await sqlConnection.CloseAsync();
        }

        public async Task DeleteAsync(IGoal goal)
        {
            await DeleteAsync(goal.Id, goal.UserId);
        }

        public async Task DeleteAsync(int id, int userId)
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
        }

        public async Task DeleteListAsync(IEnumerable<IGoal> goalList, int userId)
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
        }

        public async Task MoveGoalToExpensesAsync(IGoal goal, IEntry expense)
        {
            await MoveGoalToExpensesAsync(goal.Id, goal.UserId, expense);
        }

        public async Task MoveGoalToExpensesAsync(int goalId, int userId, IEntry expense)
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
            var query = @"INSERT INTO Expenses(UserId, Amount, Title, Date, IsMonthly, Importance, Currency) 
            VALUES (@UserId, @Amount, @Title, @Date, @IsMonthly, @Importance, @Currency); SELECT CAST(scope_identity() AS int);";

            sqlCommand = new SqlCommand(query, sqlConnection)
            {
                CommandType = CommandType.Text
            };

            sqlCommand.Parameters.AddWithValue("@UserId", userId);
            sqlCommand.Parameters.AddWithValue("@Amount", expense.Amount);
            sqlCommand.Parameters.AddWithValue("@Title", expense.Title);
            sqlCommand.Parameters.AddWithValue("@Date", expense.Date);
            sqlCommand.Parameters.AddWithValue("@IsMonthly", expense.Recurring);
            sqlCommand.Parameters.AddWithValue("@Importance", expense.Importance);
            sqlCommand.Parameters.AddWithValue("@Currency", expense.Currency);
            dbExpense.Id = (int)await sqlCommand.ExecuteScalarAsync();

            await sqlConnection.CloseAsync();
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
                dbGoal.Currency = reader.GetString(4);
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
                             UserId = row.Field<int>("UserId"),
                             Currency = row.Field<string>("Currency")
                         } as IGoal).ToList();

            await sqlConnection.CloseAsync();
            list.AddRange(items);
            return list;
        }
    }
}