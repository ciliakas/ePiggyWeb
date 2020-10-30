using ePiggyWeb.DataBase.Models;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class DatabaseContext : DbContext
    {
        private const string ConnectionString =
            "Server=51.75.187.147;Database=SmartSaver;User Id=usern;Password=123456789;";
        public DbSet<User> Users { get; set; }
        public DbSet<Expenses> Expenses { get; set; }
        public DbSet<Incomes> Incomes { get; set; }
        public DbSet<Goals> Goals { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
