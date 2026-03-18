using System;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class SettingsFileService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _rootFolder;

        public SettingsFileService(string? rootFolder = null)
        {
            _rootFolder = string.IsNullOrWhiteSpace(rootFolder)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Momentum")
                : rootFolder;

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public UserSettings Load(string? userKey = null)
        {
            string path = GetFilePath(userKey);

            try
            {
                EnsureParentDirectory(path);

                if (!File.Exists(path) || new FileInfo(path).Length == 0)
                {
                    UserSettings defaults = CreateDefaultSettings();
                    Save(defaults, userKey);
                    return defaults;
                }

                string json = File.ReadAllText(path);
                UserSettings? loaded = JsonSerializer.Deserialize<UserSettings>(json, _jsonOptions);

                if (loaded == null)
                {
                    UserSettings defaults = CreateDefaultSettings();
                    Save(defaults, userKey);
                    return defaults;
                }

                SeedMissingDefaults(loaded);
                return loaded;
            }
            catch
            {
                UserSettings defaults = CreateDefaultSettings();
                Save(defaults, userKey);
                return defaults;
            }
        }

        public void Save(UserSettings settings, string? userKey = null)
        {
            SeedMissingDefaults(settings);

            string path = GetFilePath(userKey);
            EnsureParentDirectory(path);

            string json = JsonSerializer.Serialize(settings, _jsonOptions);
            File.WriteAllText(path, json);
        }

        public string GetProfileFolder(string? userKey = null)
        {
            return string.IsNullOrWhiteSpace(userKey)
                ? _rootFolder
                : Path.Combine(_rootFolder, "Profiles", SanitizeFolderName(userKey));
        }

        public string GetFilePath(string? userKey = null)
        {
            return Path.Combine(GetProfileFolder(userKey), "settings.json");
        }

        private static UserSettings CreateDefaultSettings()
        {
            return new UserSettings
            {
                DisplayName = "Athlete",
                HeightCm = 170,
                WeightKg = 70,
                SplitPlanId = "PPL"
            };
        }

        private static void SeedMissingDefaults(UserSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.DisplayName))
            {
                settings.DisplayName = "Athlete";
            }

            if (settings.HeightCm <= 0)
            {
                settings.HeightCm = 170;
            }

            if (settings.WeightKg <= 0)
            {
                settings.WeightKg = 70;
            }

            if (string.IsNullOrWhiteSpace(settings.SplitPlanId))
            {
                settings.SplitPlanId = "PPL";
            }
        }

        private static void EnsureParentDirectory(string path)
        {
            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string SanitizeFolderName(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return "default";
            }

            string sanitized = rawValue.Trim();

            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                sanitized = sanitized.Replace(invalid.ToString(), string.Empty);
            }

            return string.IsNullOrWhiteSpace(sanitized) ? "default" : sanitized;
        }
    }
}