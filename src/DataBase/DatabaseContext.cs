using ePiggyWeb.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class DatabaseContext : DbContext
    {
        private const string ConnectionString =
            "Server=51.75.187.147;Database=SmartSaver;User Id=usern;Password=123456789;";
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ExpenseModel> Expenses { get; set; }
        public DbSet<IncomeModel> Incomes { get; set; }
        public DbSet<GoalModel> Goals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
