﻿namespace Email2FAuth.Models
{
    public record ConfirmTOTPModel
    {
        public string Username { get; set; }
        public string Code { get; set; }
    }
}
