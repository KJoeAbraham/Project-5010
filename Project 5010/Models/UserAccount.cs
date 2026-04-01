// UserAccount.cs
// Stores login credentials for a user. The password is never saved in plain text —
// it's hashed with PBKDF2 so even if someone reads the file, they can't see the password.

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