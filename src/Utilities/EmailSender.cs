using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ePiggyWeb.DataBase;

namespace ePiggyWeb.Utilities
{
    public class EmailSender
    {
        public string OurEmail { get; set; }
        public string EmailPassword { get; set; }
        public string RecoveryMessageSubject { get; set; }
        public string RecoveryMessageBody { get; set; }
        public string GreetingMessageSubject { get; set; }
        public string GreetingMessageBody { get; set; }
        public string FarewellMessageSubject { get; set; }
        public string FarewellMessageBody { get; set; }
        public int MaxRandValue { get; set; }
        public string SmtpEmail { get; set; }
        public int Port { get; set; }

        public async Task<int> SendRecoveryCodeAsync(string email)
        {
            var rand = new Random();
            var randomCode = rand.Next(MaxRandValue);
            var message = new MailMessage(OurEmail, email, RecoveryMessageSubject, RecoveryMessageBody + randomCode);
            await SendAsync(message);
            return randomCode;
        }

        public async Task SendFarewellEmailAsync(object sender, int id, UserDatabase userDatabase)
        {
            var user = await userDatabase.GetUserAsync(id);
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
