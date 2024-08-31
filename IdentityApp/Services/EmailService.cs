using System.Threading.Tasks;
using IdentityApp.DTOs.Account;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;

namespace IdentityApp.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<bool> SendEmailAsync(EmailSendDto emailSendDTO)
        {
            var apiKey = _config["MailJet:ApiKey"];
            var emailSecretKey = _config["MailJet:SecretKey"];
            MailjetClient client = new MailjetClient(apiKey, emailSecretKey);

            var email = new TransactionalEmailBuilder().
                WithFrom(new SendContact(_config["Email:From"], _config["Email:ApplicationName"])).
                WithSubject(emailSendDTO.Subject).
                WithHtmlPart(emailSendDTO.Body).
                WithTo(new SendContact(emailSendDTO.To)).
                Build();

            var response = await client.SendTransactionalEmailAsync(email);
            if(response.Messages != null)
            {
                if (response.Messages[0].Status == "success")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
