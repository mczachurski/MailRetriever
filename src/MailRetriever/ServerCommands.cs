namespace MailRetriever
{
    public static class ServerCommands
    {
        public const string Ok = "250 OK";
        public const string StartData = "354 Start mail input; end with";
        public const string AuthenticationSuccessful = "235 Authentication successful";
        public const string AuthenticationPlainLogin = "250 AUTH=PLAIN LOGIN";
        public const string Bye = "221 See you later";
        public const string Welcome = "220 MailRetriever email server is ready";
    }
}
