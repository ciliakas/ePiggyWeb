using System.Linq;
using ePiggyWeb.DataBase.Models;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataBase
{
    public static class UserDatabase
    {
        public static int Register(string email, string pass)
        {
            using var db = new DatabaseContext();
            var userExists = db.Users.Any(a => a.Email == email); //Find if email is in db
            if (userExists)
            {
                //This email is already in use
                return -1;
            }
            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            var user = new UserModel { Email = email, Password = passwordHash, Salt = salt };
            db.Add(user);
            db.SaveChanges();

            return user.Id;
        }

        public static int Authenticate(string email, string pass)
        {
            var user = GetUser(email);

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

        public static bool ChangePassword(string email, string pass)
        {
            using var db = new DatabaseContext();
            var user = db.Users.FirstOrDefault(a => a.Email == email);

            if (user == null)
            {
                return false;
            }
                
            var salt = HashingProcessor.CreateSalt();
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            user.Password = passwordHash;
            user.Salt = salt;
            db.SaveChanges();

            return true;
        }

        public static UserModel GetUser(int userId)
        {
            using var db = new DatabaseContext();
            return db.Users.FirstOrDefault(x => x.Id == userId);
        }

        public static UserModel GetUser(string email)
        {
            using var db = new DatabaseContext();
            return db.Users.FirstOrDefault(x => x.Email == email);
        }
    }
}
