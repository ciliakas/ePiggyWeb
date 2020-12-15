#nullable enable
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class UserDatabase
    {
        public event EventHandler<UserModel>? LoggedIn, Registered, Deleted;

        public delegate void RegisterEvent(object? sender, UserModel user);
        //public event EventHandler<UserModel> Registered;

        private PiggyDbContext Database { get; }
        public UserDatabase(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task<int> RegisterAsync(string email, string pass)
        {
            var userExists = await Database.Users.AnyAsync(a => a.Email == email); //Find if email is in db
            if (userExists)
            {
                //This email is already in use
                return -1;
            }
            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            var user = new UserModel { Email = email, Password = passwordHash, Salt = salt };
            Database.Add(user);
            await Database.SaveChangesAsync();

            Registered?.Invoke(this, user);

            return user.Id;
        }

        public async Task<int> AuthenticateAsync(string email, string pass)
        {
            var user = await Database.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
            {
                //User couldn't be found
                return -1;
            }

            if (!HashingProcessor.AreEqual(pass, user.Password, user.Salt))
            {
                //Password is wrong
                return -2;
            }

            LoggedIn?.Invoke(this, user);
            
            return user.Id;
        }

        public async Task<bool> ChangePasswordAsync(string email, string pass)
        {
            var user = await Database.Users.FirstOrDefaultAsync(a => a.Email == email);

            if (user is null)
            {
                return false;
            }

            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            user.Password = passwordHash;
            user.Salt = salt;
            await Database.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            return await DeleteUserAsync(x => x.Id == userId);
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            return await DeleteUserAsync(x => x.Email == email);
        }

        public async Task<bool> DeleteUserAsync(Expression<Func<UserModel, bool>> filter)
        {
            var user = await Database.Users.FirstOrDefaultAsync(filter);

            if (user is null)
            {
                return false;
            }

            var id = user.Id;
            var goalsToRemove = await Database.Goals.Where(x => x.Id == id).ToListAsync();
            Database.RemoveRange(goalsToRemove);
            var incomesToRemove = await Database.Incomes.Where(x => x.Id == id).ToListAsync();
            Database.RemoveRange(incomesToRemove);
            var expensesToRemove = await Database.Expenses.Where(x => x.Id == id).ToListAsync();
            Database.RemoveRange(expensesToRemove);
            Database.Users.Remove(user);
            await Database.SaveChangesAsync();

            Deleted?.Invoke(this, user);
            return true;
        }

        public async Task<UserModel> GetUserAsync(int userId)
        {
            return await GetUserAsync(x => x.Id == userId);
        }

        public async Task<UserModel> GetUserAsync(string email)
        {
            return await GetUserAsync(x => x.Email == email);
        }

        public async Task<UserModel> GetUserAsync(Expression<Func<UserModel, bool>> filter)
        {
            return await Database.Users.FirstOrDefaultAsync(filter);
        }

        public async Task ChangeCurrency(int userId, string currencyCode)
        {
            var user = await Database.Users.FirstOrDefaultAsync(x => x.Id == userId);

            //user.Currency = currencyCode;

            await Database.SaveChangesAsync();
        }

        public async Task ChangeCurrency(int userId, string currencyCode, decimal rate)
        {
            var user = await Database.Users.FirstOrDefaultAsync(x => x.Id == userId);

            var goals = await Database.Goals.Where(x => x.UserId == userId).ToListAsync();
            var incomes = await Database.Incomes.Where(x => x.UserId == userId).ToListAsync();
            var expenses = await Database.Expenses.Where(x => x.UserId == userId).ToListAsync();

            if (goals != null)
            {
                foreach (var goal in goals)
                {
                    goal.Price = decimal.Round(goal.Price * rate, 2);
                }
            }
            if (incomes != null)
            {
                foreach (var income in incomes)
                {
                    income.Amount = decimal.Round(income.Amount * rate, 2);
                }
            }
            if (expenses != null)
            {
                foreach (var expense in expenses)
                {
                    expense.Amount = decimal.Round(expense.Amount * rate, 2);
                }
            }

            //user.Currency = currencyCode;

            await Database.SaveChangesAsync();
        }
    }
}
