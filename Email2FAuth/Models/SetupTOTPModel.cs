namespace Email2FAuth.Models
{
    public record SetupTOTPModel
    {
        public string Username { get; set; }
        public string Secret { get; set; }
        public string Code { get; set; }
    }
}
