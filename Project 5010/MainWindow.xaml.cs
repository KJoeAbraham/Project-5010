// MainWindow.cs
// This is the main shell of the app. It contains the sidebar navigation,
// the header bar, and a content area where different views (pages) are shown.
// It also loads the user's settings and workouts on startup.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Project_5010.Models;
using Project_5010.Services;
using Project_5010.Views;

namespace Project_5010
{
    public partial class MainWindow : Window
    {
        private readonly string _username;
        private readonly SettingsFileService _settingsService;
        private UserSettings _settings;
        private readonly WorkoutFileService _workoutFileService;
        private readonly ObservableCollection<Workout> _workouts;

        // View instances — we keep them alive so navigation is fast
        private DashboardView? _dashboardView;
        private UserControl? _workoutsView;
        private UserControl? _libraryView;
        private SettingsView? _settingsView;
        private readonly FoodFileService _foodFileService;
        private UserControl? _goalsView;

        // Motivational quotes shown in the sidebar.
        // A different one is picked each day based on the day of the year.
        private static readonly string[] DailyJokes =
        {
            "Rest day. Even champions need a Sunday. Your gains aren't going anywhere.",
            "New week, new PRs. Mondays hit different when you actually show up.",
            "Two days in. The people who quit? They're watching Netflix. You're here.",
            "Midweek grind. The bar doesn't care what day it is — neither should you.",
            "Thursday. The unsung hero of the gym week. Almost there, don't fold.",
            "Friday lifts hit different. End the week the same way you started: strong.",
            "Saturday gainz. The gym is half empty. The weights are full of potential.",
            "Discipline beats motivation every time. Show up even when you don't feel like it.",
            "You don't have to be extreme, just consistent.",
            "The only bad workout is the one that didn't happen.",
            "Small progress is still progress. Trust the process.",
            "Your future self will thank you for showing up today.",
            "Results happen over time, not overnight. Work hard, stay consistent, and be patient.",
            "The pain you feel today will be the strength you feel tomorrow.",
            "Don't count the days. Make the days count.",
            "Wake up with determination. Go to bed with satisfaction.",
            "Push yourself because no one else is going to do it for you.",
            "Success isn't always about greatness. It's about consistency.",
            "It never gets easier. You just get stronger.",
            "One workout at a time. One meal at a time. One day at a time.",
        };

        public MainWindow() : this(null)
        {
        }

        public MainWindow(string? username)
        {
            InitializeComponent();

            _username = string.IsNullOrWhiteSpace(username) ? "default" : username.Trim();

            // Load the user's saved settings (profile, goals, theme, etc.)
            _settingsService = new SettingsFileService();
            _settings = _settingsService.Load(_username);

            // Set display name to username on first login if still default
            if (_settings.DisplayName == "Athlete" || string.IsNullOrWhiteSpace(_settings.DisplayName))
            {
                _settings.DisplayName = _username;
                _settingsService.Save(_settings, _username);
            }

            // Load workout history and food service
            _workoutFileService = new WorkoutFileService(_username);
            _workouts = new ObservableCollection<Workout>(_workoutFileService.LoadWorkouts());
            _foodFileService = new FoodFileService(_username);

            // Apply the user's saved theme (Light or Dark)
            ThemeManager.ApplyTheme(_settings.ThemePreference);

            UpdateHeader();

            // Pick a motivational quote based on the day of the year
            // so users see a different one each day
            ShellJokeText.Text = DailyJokes[DateTime.Today.DayOfYear % DailyJokes.Length];

            NavigateDashboard();
        }

        // --- Navigation click handlers ---
        // Each button in the sidebar calls one of these to switch the view

        private void Logout()
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateDashboard();
        }

        private void WorkoutsNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_workoutsView == null)
            {
                _workoutsView = new WorkoutsView(_workouts, _workoutFileService);
            }

            ShowView(
                _workoutsView,
                "Workouts",
                "Log, edit, and manage sessions from the same shell.",
                WorkoutsNavButton);
        }

        private void CaloriesNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_goalsView == null)
            {
                _goalsView = new GoalsView(_settings, _foodFileService);
            }
            else
            {
                (_goalsView as GoalsView)?.RefreshFood();
            }

            ShowView(
                _goalsView,
                "Calories",
                "Daily calorie target and nutrition tracking.",
                CaloriesNavButton);
        }

        private void LibraryNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_libraryView == null)
            {
                // Create the library view directly with settings and services
                _libraryView = new LibraryView(_settings, _settingsService, _workouts, _workoutFileService);
            }

            // Push latest settings into the library view
            if (_libraryView is LibraryView lv)
                lv.ApplyUserSettings(_settings);

            ShowView(
                _libraryView,
                "Library",
                "Exercise tabs can adapt to the split selected in Settings.",
                LibraryNavButton);
        }

        private void SettingsNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsView == null)
            {
                _settingsView = new SettingsView(_settingsService, _username);
                _settingsView.SettingsSaved += SettingsView_SettingsSaved;
                _settingsView.LogoutRequested += Logout;
            }

            _settingsView.ReloadFromService();

            ShowView(
                _settingsView,
                "Settings",
                "Profile, body stats, and split preferences for Momentum.",
                SettingsNavButton);
        }

        // Creates or refreshes the dashboard view
        private void NavigateDashboard()
        {
            if (_dashboardView == null)
            {
                _dashboardView = new DashboardView(_workouts, _settings);
                _dashboardView.SetFoodService(_foodFileService);
                RefreshDashboardCalories();
            }
            else
            {
                _dashboardView.ApplySettings(_settings);
                _dashboardView.RefreshStats();
                RefreshDashboardCalories();
            }

            ShowView(
                _dashboardView,
                "Dashboard",
                "Last 7 days of activity and workout type breakdown.",
                DashboardNavButton);
        }

        // Called when the user saves settings — updates everything
        private void SettingsView_SettingsSaved(UserSettings savedSettings)
        {
            _settings = savedSettings;
            UpdateHeader();

            // Push updated settings to library if it's loaded
            if (_libraryView is LibraryView lv)
                lv.ApplyUserSettings(_settings);

            // Apply the theme change immediately
            ThemeManager.ApplyTheme(_settings.ThemePreference);

            if (_dashboardView != null)
            {
                _dashboardView.ApplySettings(_settings);
                _dashboardView.RefreshStats();
            }

            RefreshDashboardCalories();
        }

        // Calculates the calorie goal and updates the dashboard display
        private void RefreshDashboardCalories()
        {
            if (_dashboardView == null) return;
            int goal = CalorieCalculator.CalculateDailyGoal(
                _settings.WeightKg, _settings.HeightCm, _settings.Age,
                _settings.Sex, _settings.ActivityLevel, _settings.GoalType);

            var todayFood = _foodFileService.Load()
                .Where(f => f.Date.Date == DateTime.Today)
                .Sum(f => f.Calories);

            _dashboardView.SetCalorieData(goal, todayFood);
        }

        // Switches the main content area to show a different view
        private void ShowView(UserControl view, string pageTitle, string pageSubtitle, Button activeButton)
        {
            MainContentHost.Content = view;
            PageTitleText.Text = pageTitle;
            PageSubtitleText.Text = pageSubtitle;
            ApplySelectedNavStyle(activeButton);
        }

        // Highlights the active nav button and resets the others
        private void ApplySelectedNavStyle(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardNavButton,
                WorkoutsNavButton,
                LibraryNavButton,
                CaloriesNavButton,
                SettingsNavButton
            };

            foreach (Button button in buttons)
            {
                button.Background = Brushes.Transparent;
                button.BorderBrush = Brushes.Transparent;
                button.Foreground = (Brush)FindResource("TextSecondary");
            }

            // Active button gets a highlighted style using theme-aware resources
            activeButton.Background = (Brush)FindResource("NavActiveBg");
            activeButton.BorderBrush = (Brush)FindResource("NavActiveBorder");
            activeButton.Foreground = (Brush)FindResource("AppAccent");
        }

        // Updates the header bar with the user's name and split
        private void UpdateHeader()
        {
            string displayName = string.IsNullOrWhiteSpace(_settings.DisplayName) ? "Athlete" : _settings.DisplayName;
            string splitId = string.IsNullOrWhiteSpace(_settings.SplitPlanId) ? "PPL" : _settings.SplitPlanId;

            HeaderUserNameText.Text = displayName;
            HeaderSplitText.Text = ToPrettySplit(splitId) + " split";
            BrandSubtitleText.Text = "Hello, " + displayName;
        }


        // Converts split ID to a display name (e.g. "UpperLower" -> "Upper / Lower")
        private static string ToPrettySplit(string rawSplit)
        {
            if (string.IsNullOrWhiteSpace(rawSplit)) return "PPL";

            string value = rawSplit.Trim().Replace("_", " ").Replace("-", " ");
            switch (value.ToLowerInvariant())
            {
                case "ppl":          return "PPL";
                case "upper lower":
                case "upperlower":   return "Upper / Lower";
                case "full body":
                case "fullbody":     return "Full Body";
                case "bro split":
                case "brosplit":     return "Bro Split";
                default:             return value;
            }
        }

    }
}
