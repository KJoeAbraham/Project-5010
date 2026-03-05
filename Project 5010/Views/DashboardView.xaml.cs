using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Project_5010.Models;

namespace Project_5010.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly ObservableCollection<Workout> workouts;

        public DashboardView(ObservableCollection<Workout> workouts)
        {
            InitializeComponent();
            this.workouts = workouts;

            RefreshStats();
            this.workouts.CollectionChanged += (_, __) => RefreshStats();
        }

        private void RefreshStats()
        {
            TotalWorkoutsText.Text = workouts.Count.ToString();

            int totalMinutes = workouts.Sum(w => w.DurationMinutes);
            TotalDurationText.Text = FormatMinutes(totalMinutes);

            DateTime weekStart = StartOfWeek(DateTime.Now, DayOfWeek.Monday);
            var thisWeek = workouts.Where(w => w.Date >= weekStart).ToList();

            WorkoutsWeekText.Text = $"+{thisWeek.Count} this week";
            DurationWeekText.Text = $"{thisWeek.Sum(w => w.DurationMinutes)}m this week";

            int activeDays = thisWeek.Select(w => w.Date.Date).Distinct().Count();
            ActiveDaysText.Text = activeDays.ToString();

            CaloriesText.Text = "—"; // later when you add calorie tracking
        }

        private static string FormatMinutes(int minutes)
        {
            int h = minutes / 60;
            int m = minutes % 60;
            return h > 0 ? $"{h}h {m}m" : $"{m}m";
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.Date.AddDays(-1 * diff);
        }
    }
}