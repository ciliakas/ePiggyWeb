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
        private const string DefaultCurrency = "EUR";
        public event EventHandler<UserModel>? LoggedIn, Registered, Deleted;

        private PiggyDbContext Database { get; }
        public UserDatabase(PiggyDbContext database)
        {
            Database = database;
        }

        public async Task<int> RegisterAsync(string email, string pass)
        {
            var userExists = await Database.Users.AnyAsync(a => a.Email == email);
            if (userExists)
            {
                //This email is already in use
                return -1;
            }
            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            var user = new UserModel { Email = email, Password = passwordHash, Salt = salt , Currency = DefaultCurrency};
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

        public async Task ChangePasswordAsync(string email, string pass)
        {
            var user = await Database.Users.FirstOrDefaultAsync(a => a.Email == email);

            if (user is null)
            {
                throw new ArgumentException();
            }

            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            user.Password = passwordHash;
            user.Salt = salt;
            await Database.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string email)
        {
            await DeleteUserAsync(x => x.Email == email);
        }

        private async Task DeleteUserAsync(Expression<Func<UserModel, bool>> filter)
        {
            var user = await Database.Users.FirstOrDefaultAsync(filter);

            if (user is null)
            {
                throw new ArgumentException();
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
        }

        public async Task<UserModel> GetUserAsync(int userId)
        {
            return await GetUserAsync(x => x.Id == userId);
        }

        private async Task<UserModel> GetUserAsync(Expression<Func<UserModel, bool>> filter)
        {
            return await Database.Users.FirstOrDefaultAsync(filter);
        }

        public async Task ChangeCurrency(int userId, string currencyCode)
        {
            var user = await Database.Users.FirstOrDefaultAsync(x => x.Id == userId);
            user.Currency = currencyCode;
            await Database.SaveChangesAsync();
        }
    }
}
