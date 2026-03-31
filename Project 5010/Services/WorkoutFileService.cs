using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Project_5010.Models;

namespace Project_5010.Services
{
    public class WorkoutFileService
    {
        private readonly string filePath;

        public WorkoutFileService(string username = "default")
        {
            string sanitized = SanitizeName(username);
            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Momentum", "Profiles", sanitized);
            Directory.CreateDirectory(dataFolder);
            filePath = Path.Combine(dataFolder, "workouts.json");
        }

        public List<Workout> LoadWorkouts()
        {
            if (!File.Exists(filePath)) return new List<Workout>();
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<Workout>();
            return JsonSerializer.Deserialize<List<Workout>>(json) ?? new List<Workout>();
        }

        public void SaveWorkouts(List<Workout> workouts)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(workouts, options));
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
