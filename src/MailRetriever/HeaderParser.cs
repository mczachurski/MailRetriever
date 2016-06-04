namespace MailRetriever
{
    public static class HeaderParser
    {
        public static EmailHeader Parse(string header)
        {
            var emailHeader = new EmailHeader();
            emailHeader.PlainHeader = header;
            emailHeader.Subject = GetTokenFromHeader(HeaderTokens.Subject, header);
            emailHeader.ContentType = GetTokenFromHeader(HeaderTokens.ContentType, header);
            emailHeader.Date = GetTokenFromHeader(HeaderTokens.Date, header);
            emailHeader.MessageId = GetTokenFromHeader(HeaderTokens.MessageId, header);
            emailHeader.MimeVersion = GetTokenFromHeader(HeaderTokens.MimeVersion, header);

            return emailHeader;
        }

        private static string GetTokenFromHeader(string token, string header)
        {
            string[] headerLines = header.Split('\n');
            foreach (var line in headerLines)
            {
                if (line.StartsWith(token))
                {
                    return line.Replace($"{token}:", string.Empty).Trim();
                }
            }

            return string.Empty;
        }
    }
}
