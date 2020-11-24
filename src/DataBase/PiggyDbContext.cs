using ePiggyWeb.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class PiggyDbContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ExpenseModel> Expenses { get; set; }
        public DbSet<IncomeModel> Incomes { get; set; }
        public DbSet<GoalModel> Goals { get; set; }
        public PiggyDbContext(DbContextOptions<PiggyDbContext> options) : base(options) {}

    }
}
