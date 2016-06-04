namespace MailRetriever
{
    public class EmailHeader
    {
        public string Subject { get; set; }
        public string Date { get; set; }
        public string MessageId { get; set; }
        public string MimeVersion { get; set; }
        public string ContentType { get; set; }
        public string PlainHeader { get; set; }
    }
}
