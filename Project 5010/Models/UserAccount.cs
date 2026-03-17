using System;

namespace Project_5010.Models
{
    public class UserAccount
    {
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Salt { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}