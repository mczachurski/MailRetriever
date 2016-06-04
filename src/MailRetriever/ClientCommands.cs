namespace MailRetriever
{
    public static class ClientCommands
    {
        public const string Quit = "QUIT";
        public const string Ehlo = "EHLO";
        public const string Hello = "HELLO";
        public const string ReceipientTo = "RCPT TO:";
        public const string MailFrom = "MAIL FROM:";
        public const string PlainAuthentication = "AUTH PLAIN";
        public const string Data = "DATA";
        public const string EndData = "\r\n.";
    }
}
