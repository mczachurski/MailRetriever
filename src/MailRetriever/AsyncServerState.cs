using System.Text;

namespace MailRetriever
{
    public class AsyncServerState
    {
        public AsyncServerState()
        {
            EmailMessage = new EmailMessage();
            EmailContet = new StringBuilder();
        }

        public EmailMessage EmailMessage { get; set; }
        public bool IsProcessingData { get; set; }
        public StringBuilder EmailContet { get; set; }
    }
}
