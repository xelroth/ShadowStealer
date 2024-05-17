namespace Backuper.payload.Components.Browsers
{
    internal struct PasswordopFormat
    {
        internal readonly string Username;
        internal readonly string Password;
        internal readonly string Url;

        internal PasswordopFormat(string username, string password, string url)
        {
            Username = username;
            Password = password;
            Url = url;
        }
    }

    internal struct CookieopFormat
    {
        internal string Host;
        internal string Name;
        internal string Path;
        internal string Cookie; 
        internal ulong Expiry;

        internal CookieopFormat(string host, string name, string path, string cookie, ulong expiry)
        {
            Host = host;
            Name = name;
            Path = path;
            Cookie = cookie;
            Expiry = expiry;
        }
    }

    internal struct CardopFormat
    {
        internal readonly string CardNumber;
        internal readonly int ExpiryYear;
        internal readonly int ExpiryMonth;
        internal readonly string NameOnCard;

        internal CardopFormat(string cardNumber, int expiryYear, int expiryMonth, string nameOnCard)
        {
            CardNumber = cardNumber;
            ExpiryYear = expiryYear;
            ExpiryMonth = expiryMonth;
            NameOnCard = nameOnCard;
        }
    }
}
