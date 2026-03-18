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
        private readonly SettingsFileService _settingsService;
        private UserSettings _settings;
        private readonly WorkoutFileService _workoutFileService;
        private readonly ObservableCollection<Workout> _workoutsSource;

        private DashboardView? _dashboardView;
        private UserControl? _workoutsView;
        private UserControl? _libraryView;
        private SettingsView? _settingsView;

        public MainWindow() : this(null)
        {
        }

        public MainWindow(string? displayName)
        {
            InitializeComponent();

            _settingsService = new SettingsFileService();
            _settings = _settingsService.Load();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                _settings.DisplayName = displayName.Trim();
                _settingsService.Save(_settings);
            }

            _workoutFileService = new WorkoutFileService();
            _workoutsSource = new ObservableCollection<Workout>(_workoutFileService.LoadWorkouts());

            UpdateHeader();
            NavigateDashboard();
        }

        private void DashboardNavButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateDashboard();
        }

        private void WorkoutsNavButton_Click(object sender, RoutedEventArgs e)
        {
            if (_workoutsView == null)
            {
                _workoutsView = new WorkoutsView(_workoutsSource, _workoutFileService);
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
                    _workoutsSource,
                    _workoutFileService)
                    ?? BuildPlaceholderView(
                        "LibraryView not found",
                        "This shell is ready to push split settings into LibraryView. If your current LibraryView has different methods, send it to me and I will adjust it.");
            }

            ApplySettingsToLibrary(_libraryView);

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
                _settingsView = new SettingsView(_settingsService);
                _settingsView.SettingsSaved += SettingsView_SettingsSaved;
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
                _dashboardView = new DashboardView(_workoutsSource, _settings);
            }
            else
            {
                _dashboardView.ApplySettings(_settings);
                _dashboardView.RefreshStats();
            }

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
            }
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
            if (view == null)
            {
                return;
            }

            InvokeIfPresent(view, "ApplyUserSettings", _settings);
            InvokeIfPresent(view, "ApplySettings", _settings);
            InvokeIfPresent(view, "ConfigureTabsForSplit", _settings.SplitPlanId);
            InvokeIfPresent(view, "ApplySplitSelection", _settings.SplitPlanId);
            InvokeIfPresent(view, "RefreshForSettings", _settings);
        }

        private static UserControl? TryCreateExternalView(string shortTypeName, params object?[] preferredArguments)
        {
            Type? viewType = FindTypeByShortName(shortTypeName);
            if (viewType == null || !typeof(UserControl).IsAssignableFrom(viewType))
            {
                return null;
            }

            List<object?[]> attempts = new List<object?[]>();

            object?[] full = preferredArguments.Where(a => a != null).ToArray();
            if (full.Length > 0)
            {
                attempts.Add(full);
            }

            foreach (object? argument in preferredArguments)
            {
                if (argument != null)
                {
                    attempts.Add(new[] { argument });
                }
            }

            attempts.Add(Array.Empty<object?>());

            foreach (ConstructorInfo constructor in viewType.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
            {
                foreach (object?[] attempt in attempts)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length != attempt.Length)
                    {
                        continue;
                    }

                    bool valid = true;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        object? suppliedValue = attempt[i];

                        if (suppliedValue == null)
                        {
                            if (parameters[i].ParameterType.IsValueType)
                            {
                                valid = false;
                                break;
                            }

                            continue;
                        }

                        if (!parameters[i].ParameterType.IsInstanceOfType(suppliedValue))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (!valid)
                    {
                        continue;
                    }

                    try
                    {
                        return (UserControl)constructor.Invoke(attempt);
                    }
                    catch
                    {
                    }
                }
            }

            return null;
        }

        private static Type? FindTypeByShortName(string shortTypeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
                }

                Type? found = types.FirstOrDefault(t => t.Name.Equals(shortTypeName, StringComparison.Ordinal));
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static void InvokeIfPresent(object target, string methodName, object? argument)
        {
            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods.Where(m => m.Name.Equals(methodName, StringComparison.Ordinal)))
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    continue;
                }

                if (argument == null)
                {
                    if (parameters[0].ParameterType.IsValueType)
                    {
                        continue;
                    }
                }
                else if (!parameters[0].ParameterType.IsInstanceOfType(argument))
                {
                    continue;
                }

                try
                {
                    method.Invoke(target, new[] { argument });
                    return;
                }
                catch
                {
                }
            }
        }

        private static string ToPrettySplit(string rawSplit)
        {
            if (string.IsNullOrWhiteSpace(rawSplit))
            {
                return "PPL";
            }

            string value = rawSplit.Trim().Replace("_", " ").Replace("-", " ");

            switch (value.ToLowerInvariant())
            {
                case "ppl":
                    return "PPL";
                case "upper lower":
                case "upperlower":
                    return "Upper / Lower";
                case "full body":
                case "fullbody":
                    return "Full Body";
                case "bro split":
                case "brosplit":
                    return "Bro Split";
                default:
                    return value;
            }
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

            return new UserControl
            {
                Content = card
            };
        }
    }
}