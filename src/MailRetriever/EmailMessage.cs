using System.Collections.Generic;

namespace MailRetriever
{
    public class EmailMessage
    {
        public string From { get; set; }
        public IList<string> To { get; set; }
        public string Body { get; set; }
        public EmailHeader Header { get; set; }

        public EmailMessage()
        {
            To = new List<string>();
        }
    }
}
