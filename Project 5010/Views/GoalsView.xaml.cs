// GoalsView.cs
// This view handles daily calorie tracking and food logging.
// It shows a circular calorie ring, food entries grouped by meal type,
// date navigation, and lets users add/delete food with macro tracking.

using Project_5010.Models;
using Project_5010.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Project_5010.Views
{
    public partial class GoalsView : UserControl
    {
        private readonly FoodFileService _foodService;
        private UserSettings _settings;

        // The currently viewed date (user can navigate with arrows)
        private DateTime _viewDate = DateTime.Today;

        public GoalsView(UserSettings settings, FoodFileService foodService)
        {
            InitializeComponent();
            _settings = settings;
            _foodService = foodService;

            UpdateDateLabel();
            Refresh();
        }

        public void ApplyUserSettings(UserSettings settings)
        {
            _settings = settings;
            RefreshCalorieCard();
        }

        public void RefreshFood() => Refresh();

        // --- Date navigation ---

        private void PrevDay_Click(object sender, RoutedEventArgs e)
        {
            _viewDate = _viewDate.AddDays(-1);
            UpdateDateLabel();
            Refresh();
        }

        private void NextDay_Click(object sender, RoutedEventArgs e)
        {
            if (_viewDate.Date >= DateTime.Today) return;
            _viewDate = _viewDate.AddDays(1);
            UpdateDateLabel();
            Refresh();
        }

        private void UpdateDateLabel()
        {
            TodayDateText.Text = _viewDate.Date == DateTime.Today
                ? "Today — " + _viewDate.ToString("MMM d")
                : _viewDate.ToString("dddd, MMM d");

            // Disable forward arrow if viewing today
            NextDayBtn.IsEnabled = _viewDate.Date < DateTime.Today;
            NextDayBtn.Opacity = NextDayBtn.IsEnabled ? 1.0 : 0.3;
        }

        // --- Refresh everything ---

        private void Refresh()
        {
            var all = _foodService.Load();
            var dayEntries = all.Where(f => f.Date.Date == _viewDate.Date).ToList();

            // Build grouped list: one group per meal type that has entries
            var groups = dayEntries
                .GroupBy(f => f.MealType)
                .OrderBy(g => MealSortOrder(g.Key))
                .Select(g => new MealGroup
                {
                    MealType = g.Key,
                    TotalDisplay = g.Sum(f => f.Calories) + " kcal",
                    Items = new ObservableCollection<FoodEntryViewModel>(
                        g.Select(f => new FoodEntryViewModel(f)))
                })
                .ToList();

            GroupedFoodList.ItemsSource = groups;

            int totalConsumed = dayEntries.Sum(f => f.Calories);
            TotalConsumedText.Text = totalConsumed + " kcal";

            RefreshCalorieCard();
        }

        private static int MealSortOrder(string meal) => meal switch
        {
            "Breakfast" => 0,
            "Lunch" => 1,
            "Dinner" => 2,
            "Snack" => 3,
            _ => 4
        };

        // --- Circular calorie ring + stats ---

        private void RefreshCalorieCard()
        {
            bool hasProfile = _settings.Age > 0 && _settings.WeightKg > 0 && _settings.HeightCm > 0;

            if (!hasProfile)
            {
                SetupPromptBorder.Visibility = Visibility.Visible;
                CalorieBreakdownPanel.Visibility = Visibility.Collapsed;
                GoalKcalText.Text = "\u2014";
                RemainingKcalText.Text = "\u2014";
                RingConsumedText.Text = "0";
                GoalStatusText.Text = "Set up your profile in Settings.";
                GoalStatusText.Foreground = (Brush)FindResource("WarningText");
                UpdateArc(0, 1);
                return;
            }

            SetupPromptBorder.Visibility = Visibility.Collapsed;

            var (bmr, maintenance, goal) = CalorieCalculator.CalculateBreakdown(
                _settings.WeightKg, _settings.HeightCm, _settings.Age,
                _settings.Sex, _settings.ActivityLevel, _settings.GoalType);

            CalorieBreakdownPanel.Visibility = Visibility.Visible;
            BmrExplanation.Text = $"Your body burns {bmr} kcal at rest (BMR)";
            MaintenanceExplanation.Text = $"With activity, you need {maintenance} kcal to maintain";
            GoalExplanation.Text = $"Your \"{_settings.GoalType}\" target is {goal} kcal";

            var all = _foodService.Load();
            int consumed = all.Where(f => f.Date.Date == _viewDate.Date).Sum(f => f.Calories);
            int remaining = goal - consumed;

            GoalKcalText.Text = goal.ToString();
            RingConsumedText.Text = consumed.ToString();
            GoalSubtitleText.Text = $"Mifflin-St Jeor \u00b7 {_settings.GoalType} \u00b7 {_settings.ActivityLevel}";

            if (consumed > goal)
            {
                RemainingKcalText.Text = (consumed - goal).ToString();
                RemainingLabelText.Text = "Over";
                RemainingBorder.Background = (Brush)FindResource("DangerBg");
                RemainingKcalText.Foreground = (Brush)FindResource("DangerColor");
                GoalStatusText.Text = $"Over by {consumed - goal} kcal today.";
                GoalStatusText.Foreground = (Brush)FindResource("DangerColor");
                UpdateArc(1.0, -1);
            }
            else
            {
                RemainingKcalText.Text = remaining.ToString();
                RemainingLabelText.Text = "Remaining";
                RemainingBorder.Background = (Brush)FindResource("SuccessBg");
                RemainingKcalText.Foreground = (Brush)FindResource("SuccessColor");
                GoalStatusText.Text = consumed == 0
                    ? $"Start logging food to track your {goal} kcal goal."
                    : $"{remaining} kcal remaining. Keep it up!";
                GoalStatusText.Foreground = (Brush)FindResource("SuccessColor");
                double pct = goal > 0 ? Math.Min(1.0, (double)consumed / goal) : 0;
                UpdateArc(pct, 1); // purple arc
            }
        }

        // Draws the arc on the calorie ring.
        // percent: 0..1 how full the ring is
        // colorMode: 1=purple, -1=red
        private void UpdateArc(double percent, int colorMode)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
            {
                // Set arc color
                if (colorMode < 0)
                    CalorieArc.Stroke = (Brush)FindResource("DangerColor");
                else
                    CalorieArc.Stroke = (Brush)FindResource("AppAccent");

                if (percent <= 0.001)
                {
                    CalorieArc.Visibility = Visibility.Collapsed;
                    return;
                }
                CalorieArc.Visibility = Visibility.Visible;

                // Arc geometry: circle center = (80,80), radius = 74
                double cx = 80, cy = 80, r = 74;
                double clampedPct = Math.Min(percent, 0.999);
                double angle = clampedPct * 360.0;
                double rad = angle * Math.PI / 180.0;

                double endX = cx + r * Math.Sin(rad);
                double endY = cy - r * Math.Cos(rad);

                ArcFigure.StartPoint = new Point(cx, cy - r); // top of circle
                ArcSegment.Point = new Point(endX, endY);
                ArcSegment.Size = new Size(r, r);
                ArcSegment.IsLargeArc = angle > 180;
                ArcSegment.SweepDirection = SweepDirection.Clockwise;
            }));
        }

        // --- Add food ---

        private void AddFood_Click(object sender, RoutedEventArgs e)
        {
            string name = FoodNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowAddStatus("Enter a food name.", false);
                return;
            }

            if (!int.TryParse(FoodCaloriesBox.Text.Trim(), out int calories) || calories <= 0)
            {
                ShowAddStatus("Enter valid calories (positive number).", false);
                return;
            }

            string meal = ((ComboBoxItem)MealTypeCombo.SelectedItem)?.Content?.ToString() ?? "Breakfast";
            int.TryParse(FoodProteinBox.Text.Trim(), out int protein);
            int.TryParse(FoodCarbsBox.Text.Trim(), out int carbs);
            int.TryParse(FoodFatBox.Text.Trim(), out int fat);

            var entry = new FoodEntry
            {
                Name = name,
                Calories = calories,
                Date = _viewDate,
                MealType = meal,
                Protein = Math.Max(0, protein),
                Carbs = Math.Max(0, carbs),
                Fat = Math.Max(0, fat)
            };

            var all = _foodService.Load();
            all.Add(entry);
            _foodService.Save(all);

            FoodNameBox.Clear();
            FoodCaloriesBox.Clear();
            FoodProteinBox.Clear();
            FoodCarbsBox.Clear();
            FoodFatBox.Clear();
            ShowAddStatus($"{name} ({calories} kcal) added.", true);

            Refresh();
        }

        private void DeleteFood_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not FoodEntryViewModel vm) return;

            var all = _foodService.Load();
            var match = all.FirstOrDefault(f =>
                f.Name == vm.Name && f.Calories == vm.Calories &&
                f.MealType == vm.MealType && f.Date.Date == vm.Date.Date);

            if (match != null)
            {
                all.Remove(match);
                _foodService.Save(all);
                Refresh();
            }
        }

        private void ShowAddStatus(string message, bool success)
        {
            AddFoodStatusText.Text = message;
            AddFoodStatusText.Foreground = success
                ? (Brush)FindResource("SuccessColor")
                : (Brush)FindResource("DangerColor");
            AddFoodStatusText.Visibility = Visibility.Visible;
        }

        // --- View models ---

        private class FoodEntryViewModel
        {
            public string Name { get; }
            public int Calories { get; }
            public string MealType { get; }
            public DateTime Date { get; }

            public FoodEntryViewModel(FoodEntry entry)
            {
                Name = entry.Name;
                Calories = entry.Calories;
                MealType = entry.MealType;
                Date = entry.Date;
            }
        }

        private class MealGroup
        {
            public string MealType { get; set; } = "";
            public string TotalDisplay { get; set; } = "";
            public ObservableCollection<FoodEntryViewModel> Items { get; set; } = new();
        }
    }
}
