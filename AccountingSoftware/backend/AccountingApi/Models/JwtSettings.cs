namespace AccountingApi.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = "YourSecretKeyHere123456789";
        public string Issuer { get; set; } = "AccountingApi";
        public string Audience { get; set; } = "AccountingClient";
        public int ExpirationMinutes { get; set; } = 60;
    }
}
