using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class FoodFileService
    {
        private readonly string filePath;
        private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions { WriteIndented = true };

        public FoodFileService(string username = "default")
        {
            string sanitized = SanitizeName(username);
            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Momentum", "Profiles", sanitized);
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "food.json");
        }

        public List<FoodEntry> LoadAll()
        {
            if (!File.Exists(filePath)) return new List<FoodEntry>();
            try
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<FoodEntry>>(json) ?? new List<FoodEntry>();
            }
            catch
            {
                return new List<FoodEntry>();
            }
        }

        public List<FoodEntry> LoadForDate(DateTime date)
        {
            return LoadAll().Where(e => e.Date.Date == date.Date).ToList();
        }

        public void Save(List<FoodEntry> entries)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(entries, JsonOpts));
        }

        private static string SanitizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "default";
            string sanitized = name.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
                sanitized = sanitized.Replace(c.ToString(), string.Empty);
            return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized;
        }
    }
}
