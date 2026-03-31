using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
        private readonly FoodFileService _foodFileService;
        private readonly ObservableCollection<Workout> _workouts;

        private DashboardView? _dashboardView;
        private UserControl? _workoutsView;
        private UserControl? _libraryView;
        private SettingsView? _settingsView;
        private GoalsView? _goalsView;

        private static readonly string[] DailyJokes =
        {
            "Why did the gym close down? It just didn't work out.",
            "I told my trainer I wanted to lose weight — he handed me dumbbells. Still not sure that was a compliment.",
            "Running late counts as cardio, right?",
            "Rest day: because even legends need to binge-watch something.",
            "I do leg day so that I can skip leg day next week.",
            "My protein shake has more personality than most people I know.",
            "The only bad workout is the one that didn't happen. Unless it's Monday."
        };

        public MainWindow() : this(null) { }

        public MainWindow(string? username)
        {
            InitializeComponent();

            _username = string.IsNullOrWhiteSpace(username) ? "default" : username.Trim();

            _settingsService = new SettingsFileService();
            _settings = _settingsService.Load(_username);

            // Set display name to username on first login if still default
            if (_settings.DisplayName == "Athlete" || string.IsNullOrWhiteSpace(_settings.DisplayName))
            {
                _settings.DisplayName = _username;
                _settingsService.Save(_settings, _username);
            }

            _workoutFileService = new WorkoutFileService(_username);
            _foodFileService = new FoodFileService(_username);
            _workouts = new ObservableCollection<Workout>(_workoutFileService.LoadWorkouts());

            ShellJokeText.Text = DailyJokes[(int)DateTime.Today.DayOfWeek];

            UpdateHeader();
            NavigateDashboard();
        }

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

        private void LibraryNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_libraryView == null)
            {
                _libraryView = TryCreateExternalView(
                    "LibraryView",
                    _settings,
                    _settingsService,
                    _workouts,
                    _workoutFileService)
                    ?? BuildPlaceholderView(
                        "LibraryView not found",
                        "This shell is ready to push split settings into LibraryView.");
            }

            ApplySettingsToLibrary(_libraryView);

            ShowView(
                _libraryView,
                "Library",
                "Exercise tabs can adapt to the split selected in Settings.",
                LibraryNavButton);
        }

        private void GoalsNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_goalsView == null)
            {
                _goalsView = new GoalsView(_settings, _foodFileService);
            }

            ShowView(
                _goalsView,
                "Goals",
                "Track daily calories and nutrition to support your training.",
                GoalsNavButton);
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

        private void NavigateDashboard()
        {
            if (_dashboardView == null)
            {
                _dashboardView = new DashboardView(_workouts, _settings);
            }
            else
            {
                _dashboardView.ApplySettings(_settings);
                _dashboardView.RefreshStats();
            }

            RefreshDashboardCalories();

            ShowView(
                _dashboardView,
                "Dashboard",
                "Last 7 days of activity and workout type breakdown.",
                DashboardNavButton);
        }

        private void SettingsView_SettingsSaved(UserSettings savedSettings)
        {
            _settings = savedSettings;
            UpdateHeader();
            ApplySettingsToLibrary(_libraryView);

            if (_dashboardView != null)
            {
                _dashboardView.ApplySettings(_settings);
                _dashboardView.RefreshStats();
                RefreshDashboardCalories();
            }

            _goalsView?.ApplySettings(_settings);
        }

        private void RefreshDashboardCalories()
        {
            if (_dashboardView == null) return;

            int goal = CalorieCalculator.CalculateDailyGoal(
                _settings.WeightKg, _settings.HeightCm, _settings.Age,
                _settings.Sex, _settings.ActivityLevel, _settings.GoalType);

            int consumed = _foodFileService.LoadForDate(DateTime.Today).Sum(e => e.Calories);
            _dashboardView.SetCalorieData(goal, consumed);
        }

        private void ShowView(UserControl view, string pageTitle, string pageSubtitle, Button activeButton)
        {
            MainContentHost.Content = view;
            PageTitleText.Text = pageTitle;
            PageSubtitleText.Text = pageSubtitle;
            ApplySelectedNavStyle(activeButton);
        }

        private void ApplySelectedNavStyle(Button activeButton)
        {
            Button[] buttons =
            {
                DashboardNavButton,
                WorkoutsNavButton,
                LibraryNavButton,
                GoalsNavButton,
                SettingsNavButton
            };

            foreach (Button button in buttons)
            {
                button.Background = Brushes.Transparent;
                button.BorderBrush = Brushes.Transparent;
                button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#525A73"));
            }

            activeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0EBFF"));
            activeButton.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DCCFFF"));
            activeButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6D4AFF"));
        }

        private void UpdateHeader()
        {
            string displayName = string.IsNullOrWhiteSpace(_settings.DisplayName) ? "Athlete" : _settings.DisplayName;
            string splitId = string.IsNullOrWhiteSpace(_settings.SplitPlanId) ? "PPL" : _settings.SplitPlanId;

            HeaderUserNameText.Text = displayName;
            HeaderSplitText.Text = ToPrettySplit(splitId) + " split";
            BrandSubtitleText.Text = "Hello, " + displayName;
        }

        private void ApplySettingsToLibrary(UserControl? view)
        {
            if (view == null) return;
            InvokeIfPresent(view, "ApplyUserSettings", _settings);
            InvokeIfPresent(view, "ApplySettings", _settings);
            InvokeIfPresent(view, "ConfigureTabsForSplit", _settings.SplitPlanId);
            InvokeIfPresent(view, "ApplySplitSelection", _settings.SplitPlanId);
            InvokeIfPresent(view, "RefreshForSettings", _settings);
        }

        private static UserControl? TryCreateExternalView(string shortTypeName, params object?[] preferredArguments)
        {
            Type? viewType = FindTypeByShortName(shortTypeName);
            if (viewType == null || !typeof(UserControl).IsAssignableFrom(viewType)) return null;

            List<object?[]> attempts = new List<object?[]>();
            object?[] full = preferredArguments.Where(a => a != null).ToArray();
            if (full.Length > 0) attempts.Add(full);
            foreach (object? argument in preferredArguments)
                if (argument != null) attempts.Add(new[] { argument });
            attempts.Add(Array.Empty<object?>());

            foreach (ConstructorInfo constructor in viewType.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
            {
                foreach (object?[] attempt in attempts)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length != attempt.Length) continue;

                    bool valid = true;
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        object? suppliedValue = attempt[i];
                        if (suppliedValue == null)
                        {
                            if (parameters[i].ParameterType.IsValueType) { valid = false; break; }
                            continue;
                        }
                        if (!parameters[i].ParameterType.IsInstanceOfType(suppliedValue)) { valid = false; break; }
                    }

                    if (!valid) continue;
                    try { return (UserControl)constructor.Invoke(attempt); }
                    catch { }
                }
            }

            return null;
        }

        private static Type? FindTypeByShortName(string shortTypeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).Cast<Type>().ToArray(); }

                Type? found = types.FirstOrDefault(t => t.Name.Equals(shortTypeName, StringComparison.Ordinal));
                if (found != null) return found;
            }
            return null;
        }

        private static void InvokeIfPresent(object target, string methodName, object? argument)
        {
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods.Where(m => m.Name.Equals(methodName, StringComparison.Ordinal)))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1) continue;
                if (argument == null)
                {
                    if (parameters[0].ParameterType.IsValueType) continue;
                }
                else if (!parameters[0].ParameterType.IsInstanceOfType(argument)) continue;

                try { method.Invoke(target, new[] { argument }); return; }
                catch { }
            }
        }

        private static string ToPrettySplit(string rawSplit)
        {
            if (string.IsNullOrWhiteSpace(rawSplit)) return "PPL";
            string value = rawSplit.Trim().Replace("_", " ").Replace("-", " ");
            return value.ToLowerInvariant() switch
            {
                "ppl"          => "PPL",
                "upper lower"  => "Upper / Lower",
                "upperlower"   => "Upper / Lower",
                "full body"    => "Full Body",
                "fullbody"     => "Full Body",
                "bro split"    => "Bro Split",
                "brosplit"     => "Bro Split",
                _              => value
            };
        }

        private static UserControl BuildPlaceholderView(string title, string message)
        {
            Border card = new Border
            {
                Margin = new Thickness(18),
                Padding = new Thickness(28),
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8EAF3")),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(24)
            };

            StackPanel stack = new StackPanel();
            stack.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 24,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E2435"))
            });
            stack.Children.Add(new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 10, 0, 0),
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6E7386"))
            });
            card.Child = stack;

            return new UserControl { Content = card };
        }
    }
}
