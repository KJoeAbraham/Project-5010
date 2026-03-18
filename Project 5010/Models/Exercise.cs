namespace Project_5010.Models
{
    public class Exercise
    {
        public string Name { get; set; } = "";
        public string Category { get; set; } = ""; 
        public string Muscle { get; set; } = "";
        public string Equipment { get; set; } = "";
        
        // These are the missing properties fixing your errors!
        public string SubMuscle { get; set; } = "";
        public string SplitPlan { get; set; } = "";
        public string Difficulty { get; set; } = "";
    }
}