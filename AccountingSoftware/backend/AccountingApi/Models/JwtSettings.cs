namespace AccountingApi.Models
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = "AccountingApi";
        public string Audience { get; set; } = "AccountingClient";
        public int ExpirationMinutes { get; set; } = 60;
    }
}
