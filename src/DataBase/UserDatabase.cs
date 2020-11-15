using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ePiggyWeb.DataBase
{
    public class UserDatabase
    {
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

            return user.Id;
        }

        public async Task<int> AuthenticateAsync(string email, string pass)
        {
            var user = await GetUserAsync(email);

            if (user == null)
            {
                //User couldn't be found
                return -1;
            }

            if (!HashingProcessor.AreEqual(pass, user.Password, user.Salt))
            {
                //Password is wrong
                return -2;
            }
            return user.Id;
        }

        public async Task<bool> ChangePasswordAsync(string email, string pass)
        {
            var user = await Database.Users.FirstOrDefaultAsync(a => a.Email == email);

            if (user == null)
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
            var temp = await Database.Users.FirstOrDefaultAsync(filter);
            if (temp is null)
            {
                return false;
            }
            Database.Users.Remove(temp);
            await Database.SaveChangesAsync();
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
    }
}
