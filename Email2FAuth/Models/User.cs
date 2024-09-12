namespace Email2FAuth.Models
{
    public record User
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string? AuthSecret { get; set; }

        public string Email { get; set; }

        public bool IsAuthEnabled { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? LastLogin { get; set; }
    }
}
