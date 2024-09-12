namespace Email2FAuth.Models
{
    public record UserRegistrationModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
