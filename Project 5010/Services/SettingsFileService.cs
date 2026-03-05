using System;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class SettingsFileService
    {
        private readonly string filePath;

        public SettingsFileService()
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "settings.json");
        }

        public UserSettings Load()
        {
            if (!File.Exists(filePath))
            {
                var defaults = new UserSettings();
                Save(defaults);
                return defaults;
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
        }

        public void Save(UserSettings settings)
        {
            var opts = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, opts);
            File.WriteAllText(filePath, json);
        }
    }
}