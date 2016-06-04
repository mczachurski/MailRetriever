# MailRetriever

![Logo](images/logo.png) It is a simple SMTP server. It can be run in c# code. It was created for unit tests purposes (espacially BDD tests). You can run SMTP server by one line of code. After that server can inform you when specific email was retrieved. Server has also a list of all received messages.

```csharp

      using (var server = new SmtpServer(12000))
      {
          // Sending an email...
          SendEmail("receiver@email.test", "subject", "content"); 
          
          // Waiting for email (timeout is 10 seconds)...
          emailMessage = server.WaitForEmailFromAsync("receiver@email.test", 10).Result;
      }

```

Example of usage in unit test:

```csharp

  [Fact]
  public void ServerRetrieveEmail()
  {
      EmailMessage emailMessage;
      List<EmailMessage> emailMessages;
      using (var server = new SmtpServer(12000))
      {
          AsyncHelpers.RunSync(() => SendEmail(
              new[] { "john.doe@mailretriever.test", "emily.rose@mailretriever.test" },
              "Simple subject",
              "Email content")
          );

          emailMessage = server.WaitForEmailFromAsync("john.doe@mailretriever.test", 10).Result;
          emailMessages = server.EmailMessages.ToList();
      }

      Assert.Equal(1, emailMessages.Count);
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

```

For sending email you can use MailKit which is also ASP.NET Core 1 compatible.

AppVeyour status: [![Build status](https://ci.appveyor.com/api/projects/status/x21xuu0dhahguo04?svg=true)](https://ci.appveyor.com/project/marcinczachurski/mailretriever)
