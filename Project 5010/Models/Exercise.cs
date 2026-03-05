namespace Project_5010.Models
{
    public class Exercise
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = ""; // Push/Pull/Legs/Upper/Lower/FullBody/Cardio etc.
        public string Muscle { get; set; } = "";
        public string Equipment { get; set; } = "";
    }
}