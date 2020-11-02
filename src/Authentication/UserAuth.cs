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


        public static bool Registration(string email, string pass)
        {
            using var db = new DatabaseContext();
            var userInfo = db.Users.FirstOrDefault(a => a.Email == email); //Find if email is in db
            if (userInfo != null)
            {
                return false;
            }
            var salt = HashingProcessor.CreateSalt(SaltSize);
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            var user = new UserModel { Email = email, Password = passwordHash, Salt = salt };
            db.Add(user);
            db.SaveChanges();

            //Handler.UserId = user.Id;
            return true;
        }

        public static bool Login(string email, string pass)
        {
            using var db = new DatabaseContext();
            var userInfo = db.Users.FirstOrDefault(a => a.Email == email); //Find user and pass in db and check if matches

            if (userInfo == null)
            {
                return false;
            }

            if (!HashingProcessor.AreEqual(pass, userInfo.Password, userInfo.Salt))
            {
                return false;
            }
            //Handler.UserId = userInfo.Id;
            return true;
        }

        public static bool ChangePassword(string email, string pass)
        {
            using var db = new DatabaseContext();
            var userInfo = db.Users.FirstOrDefault(a => a.Email == email);

            if (userInfo == null)
            {
                return false;
            }
                
            var salt = HashingProcessor.CreateSalt(SaltSize);
            var passwordHash = HashingProcessor.GenerateHash(pass, salt);

            userInfo.Password = passwordHash;
            userInfo.Salt = salt;
            db.SaveChanges();

            return true;
        }

        public static int SendCode(string email)
        {
            using var db = new DatabaseContext();
            var userInfo = db.Users.FirstOrDefault(a => a.Email == email);

            if (userInfo == null)
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
