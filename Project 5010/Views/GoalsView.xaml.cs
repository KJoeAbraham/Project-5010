using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Project_5010.Models;
using Project_5010.Services;

namespace Project_5010.Views
{
    public partial class GoalsView : UserControl
    {
        private readonly FoodFileService _foodFileService;
        private UserSettings _settings;
        private readonly ObservableCollection<FoodEntryViewModel> _entries = new();

        public GoalsView(UserSettings settings, FoodFileService foodFileService)
        {
            InitializeComponent();
            _settings = settings;
            _foodFileService = foodFileService;

            FoodListBox.ItemsSource = _entries;
            Reload();
        }

        public void ApplySettings(UserSettings settings)
        {
            _settings = settings;
            RefreshCalorieCard();
        }

        private void Reload()
        {
            _entries.Clear();
            var today = _foodFileService.LoadForDate(DateTime.Today);
            foreach (var e in today)
                _entries.Add(new FoodEntryViewModel(e));
            RefreshCalorieCard();
        }

        private void RefreshCalorieCard()
        {
            int goal = CalorieCalculator.CalculateDailyGoal(
                _settings.WeightKg, _settings.HeightCm, _settings.Age,
                _settings.Sex, _settings.ActivityLevel, _settings.GoalType);

            int consumed = _entries.Sum(e => e.Calories);
            int remaining = goal - consumed;

            CalGoalText.Text = goal.ToString();
            CalConsumedText.Text = consumed.ToString();
            CalRemainingText.Text = Math.Abs(remaining).ToString();
            TotalCaloriesText.Text = $"{consumed} kcal";

            double pct = goal > 0 ? Math.Min(1.0, (double)consumed / goal) : 0;
            CalProgressLabel.Text = $"{(int)(pct * 100)}% of daily goal";

            if (consumed > goal)
            {
                CalRemainingText.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                CalStatusLabel.Text = $"{consumed - goal} kcal over goal";
                CalStatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                DashCalorieFill.Background = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
            else
            {
                CalRemainingText.Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                CalStatusLabel.Text = remaining == 0 ? "Goal reached!" : $"{remaining} kcal remaining";
                CalStatusLabel.Foreground = remaining == 0
                    ? new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81))
                    : new SolidColorBrush(Color.FromRgb(0x6D, 0x4A, 0xFF));
                DashCalorieFill.Background = new SolidColorBrush(Color.FromRgb(0x6D, 0x4A, 0xFF));
            }

            // Defer width calculation until layout is complete
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
            {
                double containerWidth = DashCalorieFill.Parent is Grid g ? g.ActualWidth : 200;
                DashCalorieFill.Width = containerWidth * pct;
            });
        }

        private void AddFood_Click(object sender, RoutedEventArgs e)
        {
            string name = FoodNameBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                FoodStatusText.Text = "Enter a food name.";
                return;
            }

            if (!int.TryParse(FoodCaloriesBox.Text, out int calories) || calories <= 0)
            {
                FoodStatusText.Text = "Enter valid calories.";
                return;
            }

            string mealType = ((ComboBoxItem)MealTypeCombo.SelectedItem)?.Content?.ToString() ?? "Snack";

            var entry = new FoodEntry
            {
                Name = name,
                Calories = calories,
                Date = DateTime.Today,
                MealType = mealType
            };

            var all = _foodFileService.LoadAll();
            all.Add(entry);
            _foodFileService.Save(all);

            _entries.Add(new FoodEntryViewModel(entry));
            RefreshCalorieCard();

            FoodNameBox.Clear();
            FoodCaloriesBox.Clear();
            FoodStatusText.Text = "Added!";
        }

        private void DeleteFood_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not FoodEntryViewModel vm) return;

            _entries.Remove(vm);

            var all = _foodFileService.LoadAll();
            var match = all.FirstOrDefault(x =>
                x.Name == vm.Name && x.Calories == vm.Calories &&
                x.Date.Date == vm.Date.Date && x.MealType == vm.MealType);
            if (match != null) all.Remove(match);
            _foodFileService.Save(all);

            RefreshCalorieCard();
        }

        // ========== VIEW MODEL ==========

        private class FoodEntryViewModel
        {
            public string Name { get; }
            public int Calories { get; }
            public string MealType { get; }
            public DateTime Date { get; }
            public string CaloriesLabel => $"{Calories} kcal";
            public string QuantityLabel => $"1 serving";
            public Brush MealColor => MealType switch
            {
                "Breakfast" => new SolidColorBrush(Color.FromRgb(0x6D, 0x4A, 0xFF)),
                "Lunch"     => new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)),
                "Dinner"    => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
                _           => new SolidColorBrush(Color.FromRgb(0x6B, 0x72, 0x80))
            };

            public FoodEntryViewModel(FoodEntry entry)
            {
                Name = entry.Name;
                Calories = entry.Calories;
                MealType = entry.MealType;
                Date = entry.Date;
            }
        }
    }
}
