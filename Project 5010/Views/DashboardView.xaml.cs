using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Project_5010.Models;

namespace Project_5010.Views
{
    public partial class DashboardView : UserControl
    {
        private readonly object? _workoutsSource;
        private readonly ObservableCollection<DailyActivityPoint> _activityPoints;
        private readonly ObservableCollection<WorkoutTypeBreakdown> _typeBreakdown;
        private UserSettings _settings;

        public DashboardView(object? workoutsSource, UserSettings settings)
        {
            InitializeComponent();

            _workoutsSource = workoutsSource;
            _settings = settings;
            _activityPoints = new ObservableCollection<DailyActivityPoint>();
            _typeBreakdown = new ObservableCollection<WorkoutTypeBreakdown>();

            ActivityItemsControl.ItemsSource = _activityPoints;
            TypeBreakdownItemsControl.ItemsSource = _typeBreakdown;

            if (_workoutsSource is INotifyCollectionChanged notifyCollectionChanged)
            {
                notifyCollectionChanged.CollectionChanged += Workouts_CollectionChanged;
            }

            ApplySettings(_settings);
            RefreshStats();
        }

        public void ApplySettings(UserSettings settings)
        {
            _settings = settings;
            string displayName = string.IsNullOrWhiteSpace(_settings.DisplayName) ? "Athlete" : _settings.DisplayName;
            WelcomeText.Text = "Welcome back, " + displayName;
        }

        public void RefreshStats()
        {
            List<object> allWorkouts = ExtractWorkouts(_workoutsSource);
            DateTime weekStart = StartOfWeek(DateTime.Today);

            List<object> thisWeek = allWorkouts
                .Where(workout => GetWorkoutDate(workout).Date >= weekStart && GetWorkoutDate(workout).Date < weekStart.AddDays(7))
                .ToList();

            TotalWorkoutsText.Text = allWorkouts.Count.ToString(CultureInfo.InvariantCulture);
            TotalMinutesText.Text = FormatMinutes(allWorkouts.Sum(GetWorkoutMinutes));
            ThisWeekText.Text = thisWeek.Count.ToString(CultureInfo.InvariantCulture);
            ActiveDaysText.Text = thisWeek
                .Select(workout => GetWorkoutDate(workout).Date)
                .Distinct()
                .Count()
                .ToString(CultureInfo.InvariantCulture);
            WeeklyMinutesText.Text = thisWeek.Sum(GetWorkoutMinutes).ToString(CultureInfo.InvariantCulture) + " min this week";

            PopulateActivityChart(allWorkouts);
            PopulateTypeBreakdown(allWorkouts);
            PopulateHighlights(allWorkouts, thisWeek);
        }

        public void SetCalorieData(int goalCalories, int consumedToday)
        {
            if (goalCalories <= 0)
            {
                CalGoalText.Text = "\u2014";
                CalConsumedText.Text = consumedToday.ToString();
                CalRemainingText.Text = "\u2014";
                CalProgressLabel.Text = "Set up profile in Settings";
                DashCalorieFill.Width = 0;
                return;
            }

            int remaining = goalCalories - consumedToday;
            CalGoalText.Text = goalCalories.ToString();
            CalConsumedText.Text = consumedToday.ToString();

            if (consumedToday > goalCalories)
            {
                CalRemainingText.Text = (consumedToday - goalCalories).ToString();
                CalRemainingLabel.Text = "kcal over";
                CalRemainingText.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                DashCalorieFill.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                CalProgressLabel.Text = $"Over by {consumedToday - goalCalories} kcal today";
                CalProgressLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
            else
            {
                CalRemainingText.Text = remaining.ToString();
                CalRemainingLabel.Text = "kcal left";
                CalRemainingText.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                DashCalorieFill.Background = new SolidColorBrush(Color.FromRgb(0x6D, 0x4A, 0xFF));
                CalProgressLabel.Text = consumedToday == 0
                    ? "Log food in Goals to track"
                    : $"{remaining} kcal remaining";
                CalProgressLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x95, 0x9C, 0xB0));
            }

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                double trackWidth = ((System.Windows.Controls.Border)DashCalorieFill.Parent).ActualWidth;
                double percent = goalCalories > 0 ? Math.Min(1.0, (double)consumedToday / goalCalories) : 0;
                DashCalorieFill.Width = trackWidth * percent;
            }));
        }

        private void PopulateActivityChart(List<object> allWorkouts)
        {
            _activityPoints.Clear();

            List<DailyActivityPoint> points = new List<DailyActivityPoint>();
            DateTime start = DateTime.Today.AddDays(-6);

            for (int i = 0; i < 7; i++)
            {
                DateTime day = start.AddDays(i);
                int minutes = allWorkouts
                    .Where(workout => GetWorkoutDate(workout).Date == day.Date)
                    .Sum(GetWorkoutMinutes);

                points.Add(new DailyActivityPoint
                {
                    DayLabel = day.ToString("ddd", CultureInfo.InvariantCulture),
                    Minutes = minutes
                });
            }

            int maxMinutes = Math.Max(1, points.Max(point => point.Minutes));

            foreach (DailyActivityPoint point in points)
            {
                point.BarHeight = 18 + ((double)point.Minutes / maxMinutes) * 160;
                point.MinutesLabel = point.Minutes + "m";
                _activityPoints.Add(point);
            }
        }

        private void PopulateTypeBreakdown(List<object> allWorkouts)
        {
            _typeBreakdown.Clear();

            int total = Math.Max(1, allWorkouts.Count);

            List<IGrouping<string, object>> grouped = allWorkouts
                .GroupBy(workout => NormalizeWorkoutType(ReadString(workout, "Type", "WorkoutType", "Category", "Name")))
                .OrderByDescending(group => group.Count())
                .ToList();

            if (grouped.Count == 0)
            {
                _typeBreakdown.Add(new WorkoutTypeBreakdown
                {
                    TypeName = "No workouts yet",
                    Count = 0,
                    PercentageLabel = "0% of your history",
                    BarWidth = 0
                });
                return;
            }

            foreach (IGrouping<string, object> group in grouped)
            {
                int count = group.Count();
                double percent = (double)count / total;

                _typeBreakdown.Add(new WorkoutTypeBreakdown
                {
                    TypeName = group.Key,
                    Count = count,
                    PercentageLabel = Math.Round(percent * 100).ToString(CultureInfo.InvariantCulture) + "% of your history",
                    BarWidth = 220 * percent
                });
            }
        }

        private void PopulateHighlights(List<object> allWorkouts, List<object> thisWeek)
        {
            int averageMinutes = allWorkouts.Count == 0
                ? 0
                : (int)Math.Round(allWorkouts.Average(GetWorkoutMinutes));

            AverageSessionText.Text = FormatMinutes(averageMinutes);

            string? favoriteType = allWorkouts
                .GroupBy(workout => NormalizeWorkoutType(ReadString(workout, "Type", "WorkoutType", "Category", "Name")))
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault();

            FavoriteTypeText.Text = string.IsNullOrWhiteSpace(favoriteType) ? "—" : favoriteType;

            DateTime? bestDay = thisWeek
                .GroupBy(workout => GetWorkoutDate(workout).Date)
                .OrderByDescending(group => group.Sum(GetWorkoutMinutes))
                .Select(group => (DateTime?)group.Key)
                .FirstOrDefault();

            BestDayText.Text = bestDay.HasValue
                ? bestDay.Value.ToString("ddd", CultureInfo.InvariantCulture)
                : "—";
        }

        private void Workouts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshStats();
        }

        private static List<object> ExtractWorkouts(object? source)
        {
            List<object> items = new List<object>();

            if (source is IEnumerable enumerable && source is not string)
            {
                foreach (object? item in enumerable)
                {
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        private static DateTime StartOfWeek(DateTime value)
        {
            int diff = (7 + (value.DayOfWeek - DayOfWeek.Monday)) % 7;
            return value.Date.AddDays(-diff);
        }

        private static string FormatMinutes(int totalMinutes)
        {
            if (totalMinutes < 60)
            {
                return totalMinutes + "m";
            }

            int hours = totalMinutes / 60;
            int minutes = totalMinutes % 60;

            return minutes == 0
                ? hours + "h"
                : hours + "h " + minutes + "m";
        }

        private static int GetWorkoutMinutes(object workout)
        {
            object? value = ReadProperty(workout, "DurationMinutes", "Duration_Minutes", "Duration", "Minutes");

            if (value == null)
            {
                return 0;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return (int)longValue;
            }

            if (value is double doubleValue)
            {
                return (int)Math.Round(doubleValue);
            }

            if (value is float floatValue)
            {
                return (int)Math.Round(floatValue);
            }

            return int.TryParse(value.ToString(), out int parsed) ? parsed : 0;
        }

        private static DateTime GetWorkoutDate(object workout)
        {
            object? value = ReadProperty(workout, "Date", "WorkoutDate", "LoggedAt", "CreatedAt");

            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            return DateTime.TryParse(value?.ToString(), out DateTime parsed)
                ? parsed
                : DateTime.Today;
        }

        private static string NormalizeWorkoutType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return "Other";
            }

            string value = type.Trim();

            switch (value.ToLowerInvariant())
            {
                case "strength":
                    return "Strength";
                case "cardio":
                    return "Cardio";
                case "flexibility":
                    return "Flexibility";
                case "running":
                    return "Running";
                case "cycling":
                    return "Cycling";
                default:
                    return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
            }
        }

        private static string ReadString(object source, params string[] propertyNames)
        {
            object? value = ReadProperty(source, propertyNames);
            return value?.ToString() ?? string.Empty;
        }

        private static object? ReadProperty(object source, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                PropertyInfo? property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (property != null)
                {
                    return property.GetValue(source);
                }
            }

            return null;
        }

        private class DailyActivityPoint
        {
            public string DayLabel { get; set; } = string.Empty;
            public int Minutes { get; set; }
            public string MinutesLabel { get; set; } = string.Empty;
            public double BarHeight { get; set; }
        }

        private class WorkoutTypeBreakdown
        {
            public string TypeName { get; set; } = string.Empty;
            public int Count { get; set; }
            public string PercentageLabel { get; set; } = string.Empty;
            public double BarWidth { get; set; }
        }
    }
}