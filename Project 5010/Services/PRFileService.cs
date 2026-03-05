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

        public PRFileService()
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
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
    }
}