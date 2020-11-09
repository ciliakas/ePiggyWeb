using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using ePiggyWeb.DataBase;

namespace ePiggyWeb.Utilities
{
    public class EmailSender
    {
        private static string EmailAddress { get; } = "smartsaverrecovery@gmail.com";
        private static string EmailPassword { get; } = "Smartsaver123456";
        private static string RecoveryMessageSubject { get; } = "Password Recovery";
        private static string RecoveryMessageBody { get; } = "Your password recovery code is: ";
        private static int MaxRandValue { get; } = 999999;
        private static string SmtpEmail { get; } = "smtp.gmail.com";
        private static int PortConst { get; } = 587;

        public static int SendRecoveryCode(string email)
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
