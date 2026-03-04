namespace Project_5010.Models
{
    public class Workout
    {
        public string Type { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public int DurationMinutes { get; set; }
        public string Notes { get; set; } = "";
    }
}