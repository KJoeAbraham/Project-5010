// PRFileService.cs
// Handles saving and loading personal records (PRs) to/from prs.json.
// PRs track the user's best lifts for each exercise.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class PRFileService
    {
        private readonly string filePath;

        public PRFileService(string username = "default")
        {
            string sanitized = SanitizeName(username);
            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Momentum", "Profiles", sanitized);
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "prs.json");
        }

        public List<PersonalRecord> Load()
        {
            if (!File.Exists(filePath)) return new List<PersonalRecord>();
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<PersonalRecord>>(json) ?? new List<PersonalRecord>();
        }

        public void Save(List<PersonalRecord> prs)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(prs, opts));
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "default";
            string s = name.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c.ToString(), string.Empty);
            return string.IsNullOrWhiteSpace(s) ? "default" : s;
        }
    }
}
