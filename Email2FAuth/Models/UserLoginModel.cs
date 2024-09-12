namespace Email2FAuth.Models
{
    public record UserLoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
