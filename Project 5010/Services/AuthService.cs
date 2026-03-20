using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace Project_5010.Services
{
    public class AuthService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        private readonly string _dataFolder;
        private readonly string _usersFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthService()
        {
            _dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Momentum");

            _usersFilePath = Path.Combine(_dataFolder, "users.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            EnsureStorageExists();
        }

        public bool Register(string username, string password)
        {
            return Register(username, password, out _);
        }

        public bool Register(string username, string password, out string message)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Username and password cannot be empty.";
                return false;
            }

            username = username.Trim();

            List<UserAccount> users = LoadUsers();

            bool exists = users.Any(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                message = "That username already exists.";
                return false;
            }

            users.Add(new UserAccount
            {
                Username = username,
                PasswordHash = HashPassword(password)
            });

            SaveUsers(users);
            message = "Registration successful.";
            return true;
        }

        public bool Login(string username, string password)
        {
            return Login(username, password, out _);
        }

        public bool Login(string username, string password, out string message)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Username and password cannot be empty.";
                return false;
            }

            username = username.Trim();

            List<UserAccount> users = LoadUsers();

            UserAccount? user = users.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                message = "User not found.";
                return false;
            }

            bool valid = VerifyPassword(password, user.PasswordHash);

            if (!valid)
            {
                message = "Incorrect password.";
                return false;
            }

            message = "Login successful.";
            return true;
        }

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string password, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(storedPassword))
            {
                return false;
            }

            string[] parts = storedPassword.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }

        private void EnsureStorageExists()
        {
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }

            if (!File.Exists(_usersFilePath))
            {
                SaveUsers(new List<UserAccount>());
            }
        }

        private List<UserAccount> LoadUsers()
        {
            try
            {
                EnsureStorageExists();

                string json = File.ReadAllText(_usersFilePath);
                List<UserAccount>? users = JsonSerializer.Deserialize<List<UserAccount>>(json, _jsonOptions);

                return users ?? new List<UserAccount>();
            }
            catch
            {
                return new List<UserAccount>();
            }
        }

        private void SaveUsers(List<UserAccount> users)
        {
            Directory.CreateDirectory(_dataFolder);

            string json = JsonSerializer.Serialize(users, _jsonOptions);
            File.WriteAllText(_usersFilePath, json);
        }

        private class UserAccount
        {
            public string Username { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
        }
    }
}