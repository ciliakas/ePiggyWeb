using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace ePiggyWeb.Utilities
{
    public class EmailSender
    {
        public const string Email = "Email";
        public string OurEmail { get; }
        public string EmailPassword { get; }
        public string RecoveryMessageSubject { get; }
        public string RecoveryMessageBody { get; }
        public string GreetingMessageSubject { get; }
        public string GreetingMessageBody { get; }
        public string FarewellMessageSubject { get; }
        public string FarewellMessageBody { get; }
        public int MaxRandValue { get; }
        public string SmtpEmail { get; }
        public int Port { get; }

        public async Task<int> SendRecoveryCodeAsync(string email)
        {
            var rand = new Random();
            var randomCode = rand.Next(MaxRandValue);
            var message = new MailMessage(OurEmail, email, RecoveryMessageSubject, RecoveryMessageBody + randomCode);
            await SendAsync(message);
            return randomCode;
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
