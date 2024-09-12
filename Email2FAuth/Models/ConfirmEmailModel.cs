namespace Email2FAuth.Models
{
    public record ConfirmEmailModel
    {
        public string Username { get; set; }
        public string Code { get; set; }
    }
}
