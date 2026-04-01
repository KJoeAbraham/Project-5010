// FoodFileService.cs
// Handles saving and loading food entries to/from food.json.
// Each user has their own food log stored in their profile folder.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class FoodFileService
    {
        private readonly string filePath;

        public FoodFileService(string username = "default")
        {
            string sanitized = SanitizeName(username);
            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Momentum", "Profiles", sanitized);
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "food.json");
        }

        public List<FoodEntry> Load()
        {
            if (!File.Exists(filePath)) return new List<FoodEntry>();
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<FoodEntry>>(json) ?? new List<FoodEntry>();
            }
            catch { return new List<FoodEntry>(); }
        }

        public void Save(List<FoodEntry> entries)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(entries, opts));
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
