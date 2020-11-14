using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;

namespace ePiggyWeb.Utilities
{
    public class EmailSender
    {
        private string OurEmail { get; } = "smartsaverrecovery@gmail.com";
        private string EmailPassword { get; } = "Smartsaver123456";
        private string RecoveryMessageSubject { get; } = "Password Recovery";
        private string RecoveryMessageBody { get; } = "Your password recovery code is: ";
        private string GreetingMessageSubject { get; } = "Welcome to ePiggy!";
        private string GreetingMessageBody { get; } = "Thank you for joining ePiggy! Your wallet thanks you as well!";
        private string FarewellMessageSubject { get; } = "Farewell from the ePiggy team";
        private string FarewellMessageBody { get; } = "We're sorry to see you go, please leave your feedback of our app.";
        private int MaxRandValue { get; }
        private string SmtpEmail { get; }
        private int Port { get; }

        public EmailSender(int maxRandValue = 999999, string smtpEmail = "smtp.gmail.com", int port = 587)
        {
            MaxRandValue = maxRandValue;
            SmtpEmail = smtpEmail;
            Port = port;
        }

        public async Task<int> SendRecoveryCodeAsync(string email)
        {
            var rand = new Random();
            var randomCode = rand.Next(MaxRandValue);
            var message = new MailMessage(OurEmail, email, RecoveryMessageSubject, RecoveryMessageBody + randomCode);
            await SendAsync(message);
            return randomCode;
        }

        public async Task SendFarewellEmailAsync(object sender, int id)
        {
            var user = UserDatabase.GetUser(id);
            if (user is null) return;
            await SendFarewellEmailAsync(user.Email);
        }

        public async Task SendFarewellEmailAsync(string email)
        {
            var message = new MailMessage(OurEmail, email, FarewellMessageSubject, FarewellMessageBody);
            await SendAsync(message);
        }

        public async Task SendGreetingEmailAsync(string email)
        {
            var message = new MailMessage(OurEmail, email, GreetingMessageSubject, GreetingMessageBody);
            await SendAsync(message);
        }

        private async Task SendAsync(MailMessage message)
        {
            var smtp = new SmtpClient(SmtpEmail, Port)
            {
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(OurEmail, EmailPassword)
            };
            await smtp.SendMailAsync(message);
        }
    }
}
