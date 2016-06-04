using MailKit.Net.Smtp;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MailRetriever.Tests
{
    [Trait("MailRetriever", "Server must process received email")]
    public class ServerMustProccessEmail
    {
        private static EmailMessage _emailMessage;
        private static IList<EmailMessage> _emailMessages;

        static ServerMustProccessEmail()
        {
            using (var server = new SmtpServer(12000))
            {
                AsyncHelpers.RunSync(() => SendEmail(
                    new[] { "john.doe@mailretriever.test", "emily.rose@mailretriever.test" },
                    "Simple subject",
                    "Email content")
                );

                _emailMessage = server.WaitForEmailFromAsync("john.doe@mailretriever.test", 10).Result;
                _emailMessages = server.EmailMessages.ToList();
            }
        }

        [Fact(DisplayName = "Server has email on list of emails")]
        public void ServerHasEmailOnListOfEmails()
        {
            Assert.Equal(1, _emailMessages.Count);
        }

        [Fact(DisplayName = "Email has correct receipient addresses")]
        public void EmailHasCorrectReceipientAddresses()
        {
            Assert.Equal("john.doe@mailretriever.test", _emailMessage.To[0]);
            Assert.Equal("emily.rose@mailretriever.test", _emailMessage.To[1]);
        }

        [Fact(DisplayName = "Email has correct sender address")]
        public void EmailHasCorrectSenderAddress()
        {
            Assert.Equal("mail.retriever@mailretriever.test", _emailMessage.From);
        }

        [Fact(DisplayName = "Email has correct body")]
        public void EmailHasCorrectBody()
        {
            Assert.Contains("Email content", _emailMessage.Body);
        }

        [Fact(DisplayName = "Email has correct subject")]
        public void EmailHasCorrectSubject()
        {
            Assert.Contains("Simple subject", _emailMessage.Header.Subject);
        }

        public static async Task<bool> SendEmail(string[] recipientAddress, string subject, string body)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("MailRetriever", "mail.retriever@mailretriever.test"));

            foreach (var address in recipientAddress)
            {
                mimeMessage.To.Add(new MailboxAddress(address, address));
            }

            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = body
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("127.0.0.1", 12000, false);
                await client.AuthenticateAsync("mail.retriever@mailretriever.test", "password");
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return true;
        }
    }
}
