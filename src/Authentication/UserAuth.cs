using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using ePiggyWeb.DataBase;
using ePiggyWeb.DataBase.Models;

namespace ePiggyWeb.Authentication
{
    public static class UserAuth
    {
        private const string EmailAddress = "smartsaverrecovery@gmail.com";
        private const string EmailPassword = "Smartsaver123456";
        private const string RecoveryMessageSubject = "Password Recovery";
        private const string RecoveryMessageBody = "Your password recovery code is: ";
        private const int MaxRandValue = 999999;
        private const string SmtpEmail = "smtp.gmail.com";
        private const int PortConst = 587;
        private const int SaltSize = 20;


        public static int Registration(string email, string pass)
        {
            using var db = new DatabaseContext();
            var userInfo = db.Users.FirstOrDefault(a => a.Email == email); //Find if email is in db
            if (userInfo != null)
            {
                //This email is already in use
                return -1;
            }
            var salt = HashingProcessor.CreateSalt(SaltSize);
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            var user = new UserModel { Email = email, Password = passwordHash, Salt = salt };
            db.Add(user);
            db.SaveChanges();

            return user.Id;
        }

        public static int Login(string email, string pass)
        {
            using var db = new DatabaseContext();
            var user = db.Users.FirstOrDefault(a => a.Email == email); //Find user and pass in db and check if matches

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

        public static int GetUserIdByEmail(string email)
        {
            if (email is null)
            {
                return -1;
            }
            using var db = new DatabaseContext();
            var user = db.Users.FirstOrDefault(a => a.Email == email);
            if (user == null)
            {
                return -1;
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
                
            var salt = HashingProcessor.CreateSalt(SaltSize);
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            user.Password = passwordHash;
            user.Salt = salt;
            db.SaveChanges();

            return true;
        }

        public static int SendCode(string email)
        {
            using var db = new DatabaseContext();
            var user = db.Users.FirstOrDefault(a => a.Email == email);

            if (user == null)
            {
                return 0;
            }

            var rand = new Random();
            var randomCode = rand.Next(MaxRandValue);

            var message = new MailMessage();
            message.To.Add(email);
            message.From = new MailAddress(EmailAddress);
            message.Body = RecoveryMessageBody + randomCode;
            message.Subject = RecoveryMessageSubject;

            var smtp = new SmtpClient(SmtpEmail)
            {
                EnableSsl = true,
                Port = PortConst,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(EmailAddress, EmailPassword)
            };
            smtp.Send(message);

            return randomCode;
        }
    }
}
